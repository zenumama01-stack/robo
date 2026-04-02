  ViewContainerRef,
import { Observable, Subject, BehaviorSubject, combineLatest } from 'rxjs';
import { EvaluationPreferences, DEFAULT_EVALUATION_PREFERENCES } from '@memberjunction/ng-testing';
type StatusFilter = 'all' | 'running' | 'passed' | 'failed' | 'error';
type TimeRange = 'today' | 'week' | 'month' | '90days';
interface RunsFilterState {
  status: StatusFilter;
  timeRange: TimeRange;
  searchText: string;
interface FilteredStats {
  selector: 'app-testing-runs',
    <div class="runs-container" (keydown.escape)="CloseDetailPanel()">
          <h2>Test Runs</h2>
          <button class="btn btn-secondary" (click)="Refresh()" [disabled]="IsRefreshing">
            <i class="fa-solid fa-sync-alt" [class.spinning]="IsRefreshing"></i>
          <button class="btn btn-primary" (click)="StartNewTest()">
            Run Test
      <div class="filter-bar">
        <div class="filter-left">
          <div class="filter-chips">
            @for (chip of StatusChips; track chip.value) {
                [class.active]="filterState.status === chip.value"
                [attr.data-status]="chip.value"
                (click)="SetStatusFilter(chip.value)"
                @if (chip.icon) {
                  <i [class]="chip.icon"></i>
                {{ chip.label }}
          <span class="result-count">{{ (FilteredRuns$ | async)?.length ?? 0 }} results</span>
        <div class="filter-right">
            class="time-select"
            [value]="filterState.timeRange"
            (change)="OnTimeRangeChange($event)"
            <option value="today">Today</option>
            <option value="week">This Week</option>
            <option value="90days">Last 90 Days</option>
              [value]="filterState.searchText"
            @if (filterState.searchText) {
      <!-- Summary Stats Row -->
      @if (FilteredStats$ | async; as stats) {
        <div class="stats-row">
            <div class="stat-icon total"><i class="fa-solid fa-hashtag"></i></div>
            <div class="stat-body">
              <div class="stat-value">{{ stats.totalRuns }}</div>
            <div class="stat-icon pass"><i class="fa-solid fa-check-circle"></i></div>
              <div class="stat-value">{{ FormatPercent(stats.passRate) }}</div>
              <div class="stat-label">Pass Rate</div>
            <div class="stat-icon duration"><i class="fa-solid fa-clock"></i></div>
              <div class="stat-value">{{ FormatDuration(stats.avgDuration) }}</div>
            <div class="stat-icon cost"><i class="fa-solid fa-dollar-sign"></i></div>
              <div class="stat-value">{{ FormatCostTotal(stats.totalCost) }}</div>
      <div class="runs-table-wrapper">
          <div class="col col-name">Test Name</div>
          <div class="col col-eval">Evaluation</div>
          <div class="col col-score">Score</div>
          <div class="col col-duration">Duration</div>
          <div class="col col-cost">Cost</div>
          <div class="col col-time">Started</div>
          <div class="col col-actions">Actions</div>
        @if (instrumentationService.isLoading$ | async) {
          <div class="table-loading">
            <mj-loading text="Loading test runs..."></mj-loading>
        } @else if ((FilteredRuns$ | async)?.length === 0) {
          <div class="table-empty">
            <p>No test runs found</p>
            <span class="empty-hint">Try adjusting your filters or run a new test.</span>
          @for (run of FilteredRuns$ | async; track TrackByRunId($index, run)) {
              [class.is-running]="run.status === 'Running'"
              [class.is-selected]="SelectedRun?.id === run.id"
              (click)="SelectRun(run)"
              <div class="col col-name">
                <span class="test-name-link" (click)="NavigateToTest(run, $event)">
                  {{ run.testName }}
              <div class="col col-eval">
                  [executionStatus]="run.status"
                  [originalStatus]="run.status"
                  [autoScore]="run.score"
                  [passedChecks]="run.passedChecks"
                  [failedChecks]="run.failedChecks"
                  [totalChecks]="run.totalChecks"
                  [humanRating]="run.humanRating"
                  [humanIsCorrect]="run.humanIsCorrect"
                  [hasHumanFeedback]="run.hasHumanFeedback"
                  [preferences]="EvalPreferences"
                  mode="compact"
                ></app-evaluation-badge>
              <div class="col col-score">
              <div class="col col-duration">{{ FormatDuration(run.duration) }}</div>
              <div class="col col-cost">
              <div class="col col-time">{{ FormatRelativeTime(run.runDateTime) }}</div>
              <div class="col col-actions">
                <button class="icon-btn" title="View details" (click)="SelectRun(run); $event.stopPropagation()">
                <button class="icon-btn" title="Re-run test" (click)="RerunTest(run); $event.stopPropagation()">
      <!-- Detail Panel (slide-in) -->
      @if (SelectedRun) {
        <div class="detail-overlay" (click)="CloseDetailPanel()"></div>
            <div class="detail-title-section">
              <h3>{{ SelectedRun.testName }}</h3>
              <app-test-status-badge [status]="SelectedRun.status"></app-test-status-badge>
            <button class="close-btn" (click)="CloseDetailPanel()">
          <div class="detail-panel-body">
            <!-- Metrics row -->
              <div class="detail-metric">
                <span class="dm-label">Score</span>
                <app-score-indicator [score]="SelectedRun.score" [showBar]="true"></app-score-indicator>
                <span class="dm-label">Duration</span>
                <span class="dm-value">{{ FormatDuration(SelectedRun.duration) }}</span>
                <span class="dm-label">Cost</span>
                <app-cost-display [cost]="SelectedRun.cost"></app-cost-display>
                <span class="dm-label">Started</span>
                <span class="dm-value">{{ FormatDateTime(SelectedRun.runDateTime) }}</span>
            <!-- Evaluation Badge (expanded) -->
              <h4><i class="fa-solid fa-clipboard-check"></i> Evaluation</h4>
                [executionStatus]="SelectedRun.status"
                [originalStatus]="SelectedRun.status"
                [autoScore]="SelectedRun.score"
                [passedChecks]="SelectedRun.passedChecks"
                [failedChecks]="SelectedRun.failedChecks"
                [totalChecks]="SelectedRun.totalChecks"
                [humanRating]="SelectedRun.humanRating"
                [humanIsCorrect]="SelectedRun.humanIsCorrect"
                [hasHumanFeedback]="SelectedRun.hasHumanFeedback"
                mode="expanded"
            <!-- Execution Context -->
            @if (SelectedRun.targetType) {
                <h4><i class="fa-solid fa-server"></i> Execution Context</h4>
                <mj-execution-context></mj-execution-context>
            <!-- Inline Feedback Form -->
            <div class="detail-section feedback-section">
              <h4><i class="fa-solid fa-comment-dots"></i> Human Feedback</h4>
              @if (SelectedRun.hasHumanFeedback) {
                <div class="existing-feedback">
                  <span class="feedback-label">Rating:</span>
                  <span class="feedback-value">{{ SelectedRun.humanRating }}/10</span>
                  <span class="feedback-label">Correct:</span>
                  <span class="feedback-value">
                    @if (SelectedRun.humanIsCorrect === true) {
                      <i class="fa-solid fa-check" style="color: #22c55e"></i> Yes
                    } @else if (SelectedRun.humanIsCorrect === false) {
                      <i class="fa-solid fa-times" style="color: #ef4444"></i> No
                      --
                  @if (SelectedRun.humanComments) {
                    <div class="feedback-comments">{{ SelectedRun.humanComments }}</div>
                    <label>Rating</label>
                    <div class="rating-buttons">
                      @for (n of RatingValues; track n) {
                          [class.selected]="feedbackRating === n"
                          (click)="feedbackRating = n"
                    <label>Is the result correct?</label>
                        [class.active]="feedbackIsCorrect === true"
                        (click)="feedbackIsCorrect = true"
                        <i class="fa-solid fa-check"></i> Yes
                        [class.active]="feedbackIsCorrect === false"
                        (click)="feedbackIsCorrect = false"
                        <i class="fa-solid fa-times"></i> No
                  <div class="comments-row">
                      placeholder="Optional feedback comments..."
                      [value]="feedbackComments"
                      (input)="OnFeedbackCommentInput($event)"
                    class="btn btn-primary submit-feedback-btn"
                    (click)="SubmitFeedback()"
                    [disabled]="IsSubmittingFeedback"
                    {{ IsSubmittingFeedback ? 'Submitting...' : 'Submit Feedback' }}
       Testing Runs Component
    .runs-container {
      box-shadow: 0 4px 12px rgba(59, 130, 246, 0.3);
    .filter-left {
    .filter-chips {
      padding: 7px 14px;
    .chip.active[data-status="running"] {
    .chip.active[data-status="passed"] {
    .chip.active[data-status="failed"] {
    .chip.active[data-status="error"] {
    .filter-right {
    .time-select {
    .time-select:focus {
    /* Stats Row */
    .stats-row {
    .stat-icon.total { background: rgba(99, 102, 241, 0.1); color: #6366f1; }
    .stat-icon.pass { background: rgba(34, 197, 94, 0.1); color: #22c55e; }
    .stat-icon.duration { background: rgba(139, 92, 246, 0.1); color: #8b5cf6; }
    .stat-icon.cost { background: rgba(245, 158, 11, 0.1); color: #f59e0b; }
    .stat-body {
    /* Runs Table */
    .runs-table-wrapper {
      grid-template-columns: 2fr 140px 120px 100px 100px 120px 90px;
    .table-row:last-child {
    .table-row.is-selected {
      background: rgba(99, 102, 241, 0.06);
    .table-row.is-running {
    .table-row.is-running .test-name-link::after {
      animation: pulse-dot 1.5s infinite;
    .col {
    .test-name-link {
    .test-name-link:hover {
    /* Table states */
    .table-loading {
    .table-empty {
    .table-empty i {
    .table-empty p {
    .detail-overlay {
      background: rgba(0, 0, 0, 0.2);
      box-shadow: -4px 0 24px rgba(0, 0, 0, 0.12);
      animation: slideIn 0.25s ease-out;
    .detail-title-section {
    .detail-title-section h3 {
    .detail-panel-body {
    .detail-metric {
    .dm-label {
    .dm-value {
    .detail-section h4 i {
    /* Feedback Section */
    .existing-feedback {
      gap: 8px 16px;
    .feedback-value {
      border-top: 1px solid #dcfce7;
    .feedback-form {
    .feedback-form label {
    .rating-buttons {
    .rating-btn:hover {
    .comments-row textarea {
    .comments-row textarea:focus {
    .comments-row textarea::placeholder {
    .submit-feedback-btn {
      .filter-left, .filter-right {
      .table-header, .table-row {
        grid-template-columns: 2fr 120px 100px 100px 80px;
      .col-eval, .col-time {
        max-width: 100vw;
export class TestingRunsComponent implements OnInit, OnDestroy {
  private filterTrigger$ = new BehaviorSubject<void>(undefined);
  // Filter state
  filterState: RunsFilterState = {
    timeRange: 'month',
    searchText: ''
  // Feedback form state
  feedbackRating = 5;
  feedbackIsCorrect: boolean | null = null;
  feedbackComments = '';
  IsSubmittingFeedback = false;
  SelectedRun: TestRunWithFeedbackSummary | null = null;
  EvalPreferences: EvaluationPreferences = { ...DEFAULT_EVALUATION_PREFERENCES, showAuto: true };
  // Observables
  FilteredRuns$!: Observable<TestRunWithFeedbackSummary[]>;
  FilteredStats$!: Observable<FilteredStats>;
  readonly StatusChips: Array<{ value: StatusFilter; label: string; icon: string }> = [
    { value: 'all', label: 'All', icon: '' },
    { value: 'running', label: 'Running', icon: 'fa-solid fa-spinner fa-spin' },
    { value: 'passed', label: 'Passed', icon: 'fa-solid fa-check' },
    { value: 'failed', label: 'Failed', icon: 'fa-solid fa-times' },
    { value: 'error', label: 'Error', icon: 'fa-solid fa-exclamation-triangle' }
  readonly RatingValues: number[] = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    public instrumentationService: TestingInstrumentationService,
    this.applyInitialState();
    this.updateServiceDateRange();
  OnEscapeKey(): void {
    if (this.SelectedRun) {
      this.CloseDetailPanel();
  // ------- Public Methods -------
  SetStatusFilter(status: StatusFilter): void {
    this.filterState.status = status;
    this.filterTrigger$.next();
  OnTimeRangeChange(event: Event): void {
    const select = event.target as HTMLSelectElement;
    this.filterState.timeRange = select.value as TimeRange;
    this.filterState.searchText = input.value;
    this.filterState.searchText = '';
  StartNewTest(): void {
    this.testingDialogService.OpenTestRunDialog({ viewContainerRef: this.viewContainerRef });
  SelectRun(run: TestRunWithFeedbackSummary): void {
    this.SelectedRun = run;
    this.resetFeedbackForm();
  CloseDetailPanel(): void {
    this.SelectedRun = null;
  NavigateToTest(run: TestRunWithFeedbackSummary, event: MouseEvent): void {
    SharedService.Instance.OpenEntityRecord('MJ: Test Runs', CompositeKey.FromID(run.id));
  RerunTest(run: TestRunWithFeedbackSummary): void {
    if (!run.testId) return;
    this.testingDialogService.OpenTestDialog(run.testId, this.viewContainerRef);
  async SubmitFeedback(): Promise<void> {
    if (!this.SelectedRun) return;
    this.IsSubmittingFeedback = true;
      this.SelectedRun.id,
      this.feedbackRating,
      this.feedbackIsCorrect === true,
      this.feedbackComments
    this.IsSubmittingFeedback = false;
      // Update the selected run to reflect feedback was submitted
      this.SelectedRun = {
        ...this.SelectedRun,
        hasHumanFeedback: true,
        humanRating: this.feedbackRating,
        humanIsCorrect: this.feedbackIsCorrect,
        humanComments: this.feedbackComments
  OnFeedbackCommentInput(event: Event): void {
    const textarea = event.target as HTMLTextAreaElement;
    this.feedbackComments = textarea.value;
  TrackByRunId(_index: number, run: TestRunWithFeedbackSummary): string {
    return run.id;
  // ------- Formatting Helpers -------
  FormatDuration(milliseconds: number): string {
    if (milliseconds < 1000) return `${Math.round(milliseconds)}ms`;
    if (minutes > 0) return `${minutes}m ${seconds % 60}s`;
    return this.formatShortDate(date);
  FormatDateTime(date: Date): string {
    return d.toLocaleString(undefined, {
  FormatCostTotal(cost: number): string {
    if (cost < 1) return `$${cost.toFixed(2)}`;
  // ------- Private Methods -------
    const data$ = this.instrumentationService.testRunsWithFeedback$.pipe(
    this.FilteredRuns$ = combineLatest([data$, this.filterTrigger$]).pipe(
      map(([runs]) => this.applyClientFilters(runs)),
    this.FilteredStats$ = this.FilteredRuns$.pipe(
      map(runs => this.computeStats(runs)),
  private applyClientFilters(runs: TestRunWithFeedbackSummary[]): TestRunWithFeedbackSummary[] {
    let filtered = runs;
    filtered = this.filterByStatus(filtered);
    filtered = this.filterBySearch(filtered);
    filtered = this.sortRunsWithRunningFirst(filtered);
  private filterByStatus(runs: TestRunWithFeedbackSummary[]): TestRunWithFeedbackSummary[] {
    const status = this.filterState.status;
    if (status === 'all') return runs;
    const statusMap: Record<string, string[]> = {
      running: ['Running'],
      passed: ['Passed'],
      failed: ['Failed'],
      error: ['Error', 'Timeout']
    const validStatuses = statusMap[status] ?? [];
    return runs.filter(r => validStatuses.includes(r.status));
  private filterBySearch(runs: TestRunWithFeedbackSummary[]): TestRunWithFeedbackSummary[] {
    const text = this.filterState.searchText.toLowerCase().trim();
    if (!text) return runs;
    return runs.filter(r => r.testName.toLowerCase().includes(text));
  private sortRunsWithRunningFirst(runs: TestRunWithFeedbackSummary[]): TestRunWithFeedbackSummary[] {
    return [...runs].sort((a, b) => {
      // Running tests at the top
      if (a.status === 'Running' && b.status !== 'Running') return -1;
      if (a.status !== 'Running' && b.status === 'Running') return 1;
      // Then by StartedAt DESC
  private computeStats(runs: TestRunWithFeedbackSummary[]): FilteredStats {
    const total = runs.length;
    if (total === 0) {
      return { totalRuns: 0, passRate: 0, avgDuration: 0, totalCost: 0 };
    const passed = runs.filter(r => r.status === 'Passed').length;
    const passRate = (passed / total) * 100;
    const completedRuns = runs.filter(r => r.duration > 0 && r.status !== 'Running');
    const avgDuration = completedRuns.length > 0
      ? completedRuns.reduce((sum, r) => sum + r.duration, 0) / completedRuns.length
    const totalCost = runs.reduce((sum, r) => sum + r.cost, 0);
    return { totalRuns: total, passRate, avgDuration, totalCost };
  private updateServiceDateRange(): void {
    switch (this.filterState.timeRange) {
      case 'today':
        startDate = new Date(now);
        startDate.setHours(0, 0, 0, 0);
        startDate = new Date(now.getTime() - 90 * 24 * 60 * 60 * 1000);
    this.instrumentationService.setDateRange(startDate, now);
  private applyInitialState(): void {
    const state = this.initialState;
    if (state['status'] && typeof state['status'] === 'string') {
      this.filterState.status = state['status'] as StatusFilter;
    if (state['timeRange'] && typeof state['timeRange'] === 'string') {
      this.filterState.timeRange = state['timeRange'] as TimeRange;
    if (state['searchText'] && typeof state['searchText'] === 'string') {
      this.filterState.searchText = state['searchText'] as string;
      status: this.filterState.status,
      timeRange: this.filterState.timeRange,
      searchText: this.filterState.searchText
  private resetFeedbackForm(): void {
    this.feedbackRating = 5;
    this.feedbackIsCorrect = null;
    this.feedbackComments = '';
  private formatShortDate(date: Date): string {
