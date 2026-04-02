import { Subject, Observable, combineLatest, BehaviorSubject } from 'rxjs';
  FeedbackPending,
  FeedbackStats,
  TestRunWithFeedbackSummary,
  EvaluationSummaryMetrics
type ViewMode = 'queue' | 'history';
type HistorySort = 'date' | 'rating' | 'test-name';
interface ReviewFormState {
  isCorrect: boolean | null;
  comments: string;
  selector: 'app-testing-review',
    <div class="review-page">
            <i class="fa-solid fa-clipboard-check"></i>
            Human Review
          @if (PendingCount > 0) {
            <div class="pending-badge">
              <span class="badge-count">{{ PendingCount }}</span>
              <span class="badge-text">pending</span>
        <button class="refresh-btn" (click)="Refresh()" [disabled]="IsRefreshing">
          <i class="fa-solid fa-arrows-rotate" [class.fa-spin]="IsRefreshing"></i>
          {{ IsRefreshing ? 'Refreshing...' : 'Refresh' }}
      <!-- KPI Summary Row -->
      @if (Metrics) {
            <div class="kpi-icon orange">
              <i class="fa-solid fa-hourglass-half"></i>
              <div class="kpi-value">{{ Metrics.humanPendingCount }}</div>
              <div class="kpi-label">Pending Review</div>
              <div class="kpi-value">{{ Metrics.humanReviewedCount }}</div>
              <div class="kpi-label">Reviewed</div>
            <div class="kpi-icon gold">
              <div class="kpi-value">{{ FormatDecimal(Metrics.humanAvgRating, 1) }}<span class="kpi-unit">/10</span></div>
              <div class="kpi-label">Avg Rating</div>
              <i class="fa-solid fa-handshake"></i>
              <div class="kpi-value">{{ FormatDecimal(Metrics.agreementRate, 0) }}<span class="kpi-unit">%</span></div>
              <div class="kpi-label">Agreement Rate</div>
      <div class="view-toggle-bar">
          <button class="toggle-btn" [class.active]="CurrentView === 'queue'" (click)="SetView('queue')">
            Review Queue
          <button class="toggle-btn" [class.active]="CurrentView === 'history'" (click)="SetView('history')">
      <!-- Review Queue View -->
      @if (CurrentView === 'queue') {
        <div class="content-card">
          @if (PendingItems.length === 0) {
              <h3>All caught up!</h3>
              <p>No tests currently require human review.</p>
            <div class="queue-list">
              @for (item of PendingItems; track item.testRunID) {
                <div class="queue-item" [class.expanded]="ExpandedItemId === item.testRunID">
                  <div class="queue-item-header" (click)="ToggleExpand(item.testRunID)">
                      <div class="item-name">{{ item.testName }}</div>
                        <span class="meta-time">
                          {{ FormatRelativeTime(item.runDateTime) }}
                        <app-score-indicator [score]="item.automatedScore" [showBar]="true"></app-score-indicator>
                        <app-test-status-badge [status]="$any(item.automatedStatus)"></app-test-status-badge>
                    <div class="item-actions-area">
                      <span class="reason-badge" [class]="'reason-' + item.reason">
                        @if (item.reason === 'no-feedback') {
                          <i class="fa-solid fa-comment-slash"></i> No Feedback
                        } @else if (item.reason === 'high-score-failed') {
                          <i class="fa-solid fa-triangle-exclamation"></i> Score Mismatch
                          <i class="fa-solid fa-circle-question"></i> Needs Verification
                      <button class="expand-toggle">
                        <i class="fa-solid" [class.fa-chevron-down]="ExpandedItemId !== item.testRunID"
                           [class.fa-chevron-up]="ExpandedItemId === item.testRunID"></i>
                  @if (ExpandedItemId === item.testRunID) {
                    <div class="review-form-panel">
                      <!-- Rating -->
                        <label class="form-label">Rating</label>
                        <div class="rating-row">
                          @for (n of RatingNumbers; track n) {
                            <button class="rating-circle"
                                    [class.selected]="FormState.rating != null && n <= FormState.rating"
                                    [class.current]="FormState.rating === n"
                                    (click)="SelectRating(n)">
                              {{ n }}
                          @if (FormState.rating != null) {
                            <span class="rating-display">{{ FormState.rating }}/10</span>
                      <!-- Correctness -->
                        <label class="form-label">Is the automated result correct?</label>
                          <button class="correctness-btn correct"
                                  [class.active]="FormState.isCorrect === true"
                                  (click)="SelectCorrectness(true)">
                            <i class="fa-solid fa-check"></i> Correct
                          <button class="correctness-btn incorrect"
                                  [class.active]="FormState.isCorrect === false"
                                  (click)="SelectCorrectness(false)">
                            <i class="fa-solid fa-xmark"></i> Incorrect
                      <!-- Notes -->
                        <label class="form-label">Notes (optional)</label>
                        <textarea class="notes-textarea"
                                  placeholder="Add any comments about this evaluation..."
                                  [value]="FormState.comments"
                                  (input)="OnCommentsChange($event)"></textarea>
                      <div class="form-actions">
                        <button class="submit-btn"
                                [disabled]="!IsFormValid || IsSubmitting"
                                (click)="SubmitReview(item)">
                          {{ IsSubmitting ? 'Submitting...' : 'Submit' }}
                        <button class="skip-btn" (click)="SkipItem()">
                          <i class="fa-solid fa-forward"></i> Skip
      <!-- History View -->
      @if (CurrentView === 'history') {
            <div class="search-wrapper">
                     placeholder="Search by test name..."
                     [value]="HistorySearchText"
                     (input)="OnHistorySearch($event)" />
            <div class="sort-control">
              <label>Sort by:</label>
              <select [value]="HistorySortField" (change)="OnHistorySortChange($event)">
                <option value="date">Date</option>
                <option value="rating">Rating</option>
                <option value="test-name">Test Name</option>
          @if (FilteredHistoryItems.length === 0) {
              <h3>No reviewed items</h3>
              <p>Reviewed tests will appear here once feedback is submitted.</p>
              @for (item of FilteredHistoryItems; track item.id) {
                <div class="history-item">
                  <div class="history-item-main">
                    <div class="history-name">{{ item.testName }}</div>
                    <div class="history-date">
                  <div class="history-rating">
                    <div class="rating-dots">
                        <span class="rating-dot" [class.filled]="item.humanRating != null && n <= item.humanRating"></span>
                    <span class="rating-label">{{ item.humanRating ?? 0 }}/10</span>
                  <div class="history-verdict">
                    @if (item.humanIsCorrect === true) {
                      <span class="verdict correct">
                    } @else if (item.humanIsCorrect === false) {
                      <span class="verdict incorrect">
                  <div class="history-auto-score">
                    <app-score-indicator [score]="item.score" [showBar]="true"></app-score-indicator>
                  @if (item.humanComments) {
                    <div class="history-comments"
                         [class.expanded]="ExpandedHistoryId === item.id"
                         (click)="ToggleHistoryComment(item.id)">
                      <p>{{ item.humanComments }}</p>
      <!-- Calibration Insights -->
      <div class="calibration-section">
          <i class="fa-solid fa-scale-balanced"></i>
          Human vs Auto Agreement
        <div class="calibration-body">
          <div class="gauge-display">
            <div class="gauge-ring">
              <svg viewBox="0 0 120 120" class="gauge-svg">
                <circle cx="60" cy="60" r="52" fill="none" stroke="#e2e8f0" stroke-width="10" />
                <circle cx="60" cy="60" r="52" fill="none"
                        [attr.stroke]="AgreementColor"
                        stroke-width="10"
                        [attr.stroke-dasharray]="AgreementDash"
                        stroke-dashoffset="0"
                        transform="rotate(-90 60 60)" />
              <div class="gauge-value">{{ FormatDecimal(AgreementRate, 0) }}%</div>
          <div class="calibration-text">
            <p class="calibration-description">
              Measures how often human reviewers agree with automated evaluation scores.
            @if (AgreementRate < 70) {
              <div class="calibration-warning">
                Low agreement may indicate evaluation criteria need refinement.
    .review-page {
    /* Page Header */
    .header-left h2 i {
      background: linear-gradient(135deg, #fff7ed 0%, #ffedd5 100%);
      border: 1px solid #fb923c;
      background: linear-gradient(135deg, #f97316 0%, #ea580c 100%);
    .badge-text {
      color: #ea580c;
    /* KPI Row */
    .kpi-icon.orange { background: linear-gradient(135deg, #f97316 0%, #ea580c 100%); }
    .kpi-icon.green { background: linear-gradient(135deg, #22c55e 0%, #16a34a 100%); }
    .kpi-icon.gold { background: linear-gradient(135deg, #eab308 0%, #ca8a04 100%); }
    .kpi-icon.blue { background: linear-gradient(135deg, #3b82f6 0%, #2563eb 100%); }
    .kpi-unit {
    .view-toggle-bar {
    /* Content Card */
    .content-card {
    /* Queue List */
    .queue-list {
    .queue-item {
    .queue-item.expanded {
    .queue-item:last-child {
    .queue-item-header {
    .queue-item-header:hover {
    .meta-time {
    .item-actions-area {
    .reason-badge {
    .reason-no-feedback {
    .reason-high-score-failed {
      background: #ffedd5;
      color: #c2410c;
    .reason-low-score-passed {
    /* Review Form */
    .review-form-panel {
      padding: 20px 20px 20px 36px;
    .form-section:last-of-type {
    /* Rating Circles */
    .rating-row {
    .rating-circle {
    .rating-circle:hover {
      border-color: #3b82f6;
    .rating-circle.selected {
    .rating-circle.current {
      border-color: #1d4ed8;
      box-shadow: 0 2px 6px rgba(37, 99, 235, 0.35);
    .rating-display {
    /* Correctness Buttons */
    .correctness-btn {
    .correctness-btn.correct:hover,
    .correctness-btn.correct.active {
      border-color: #22c55e;
    .correctness-btn.incorrect:hover,
    .correctness-btn.incorrect.active {
    /* Notes */
    .notes-textarea {
    .notes-textarea:focus {
    /* Form Actions */
    .form-actions {
    .submit-btn {
      padding: 11px 22px;
    .submit-btn:hover:not(:disabled) {
      box-shadow: 0 4px 12px rgba(37, 99, 235, 0.3);
    .submit-btn:disabled {
    .skip-btn {
      padding: 11px 20px;
    .skip-btn:hover {
    .search-wrapper {
    .search-wrapper > i {
    .search-wrapper input {
    .search-wrapper input:focus {
    .sort-control {
    .sort-control select {
    .history-item:last-child {
    .history-item-main {
    .history-name {
    .history-date {
    .history-rating {
    .rating-dots {
    .rating-dot {
    .rating-dot.filled {
    .rating-label {
    .history-verdict {
    .verdict {
    .verdict.correct {
    .verdict.incorrect {
    .history-auto-score {
    .history-comments {
    .history-comments p {
    .history-comments.expanded p {
      font-size: 52px;
    /* Calibration Section */
    .calibration-section {
    .calibration-section h3 {
    .calibration-section h3 i {
    .calibration-body {
    .gauge-display {
    .gauge-ring {
    .gauge-svg {
    .gauge-value {
      inset: 0;
    .calibration-text {
    .calibration-description {
    .calibration-warning {
      border: 1px solid #fbbf24;
    .calibration-warning i {
    /* Success toast animation */
      from { opacity: 0; transform: translateY(-4px); }
export class TestingReviewComponent implements OnInit, OnDestroy {
  CurrentView: ViewMode = 'queue';
  ExpandedItemId: string | null = null;
  ExpandedHistoryId: string | null = null;
  IsRefreshing = false;
  IsSubmitting = false;
  HistorySearchText = '';
  HistorySortField: HistorySort = 'date';
  // Form state for active review
  FormState: ReviewFormState = { rating: null, isCorrect: null, comments: '' };
  PendingItems: FeedbackPending[] = [];
  HistoryItems: TestRunWithFeedbackSummary[] = [];
  FilteredHistoryItems: TestRunWithFeedbackSummary[] = [];
  Metrics: EvaluationSummaryMetrics | null = null;
  PendingCount = 0;
  readonly RatingNumbers = [1, 2, 3, 4, 5, 6, 7, 8, 9, 10];
    this.setupSubscriptions();
  // ------------------------------------------------------------------
  //  Computed properties
    return this.FormState.rating != null && this.FormState.isCorrect != null;
  get AgreementRate(): number {
    return this.Metrics?.agreementRate ?? 0;
  get AgreementColor(): string {
    const rate = this.AgreementRate;
    if (rate >= 80) return '#22c55e';
    if (rate >= 60) return '#eab308';
  get AgreementDash(): string {
    const circumference = 2 * Math.PI * 52; // r=52
    const filled = (this.AgreementRate / 100) * circumference;
    return `${filled} ${circumference}`;
  //  Setup
    if (!this.initialState) return;
    const view = this.initialState['viewMode'] as string | undefined;
    if (view === 'queue' || view === 'history') {
      this.CurrentView = view;
  private setupSubscriptions(): void {
    this.instrumentationService.pendingFeedback$
        this.PendingItems = items;
        this.PendingCount = items.length;
    this.instrumentationService.evaluationMetrics$
      .subscribe(metrics => {
        this.Metrics = metrics;
    this.instrumentationService.testRunsWithFeedback$
      .subscribe(runs => {
        this.HistoryItems = runs.filter(r => r.hasHumanFeedback);
        this.applyHistoryFilters();
  //  View toggling
  SetView(mode: ViewMode): void {
    this.CurrentView = mode;
    this.ExpandedItemId = null;
  //  Queue interactions
  ToggleExpand(testRunID: string): void {
    if (this.ExpandedItemId === testRunID) {
      this.ExpandedItemId = testRunID;
  SelectRating(n: number): void {
    this.FormState = { ...this.FormState, rating: n };
  SelectCorrectness(value: boolean): void {
    this.FormState = { ...this.FormState, isCorrect: value };
  OnCommentsChange(event: Event): void {
    const target = event.target as HTMLTextAreaElement;
    this.FormState = { ...this.FormState, comments: target.value };
  async SubmitReview(item: FeedbackPending): Promise<void> {
    if (!this.IsFormValid || this.IsSubmitting) return;
    this.IsSubmitting = true;
      const success = await this.instrumentationService.submitFeedback(
        item.testRunID,
        this.FormState.rating!,
        this.FormState.isCorrect!,
        this.FormState.comments
      this.IsSubmitting = false;
  SkipItem(): void {
    const currentIndex = this.PendingItems.findIndex(
      i => i.testRunID === this.ExpandedItemId
    // Expand the next item if available
    if (currentIndex >= 0 && currentIndex + 1 < this.PendingItems.length) {
      this.ExpandedItemId = this.PendingItems[currentIndex + 1].testRunID;
  //  History interactions
  OnHistorySearch(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.HistorySearchText = target.value;
  OnHistorySortChange(event: Event): void {
    const target = event.target as HTMLSelectElement;
    this.HistorySortField = target.value as HistorySort;
  ToggleHistoryComment(id: string): void {
    this.ExpandedHistoryId = this.ExpandedHistoryId === id ? null : id;
  //  Refresh
    this.IsRefreshing = true;
      this.IsRefreshing = false;
    }, 1500);
  //  Formatting helpers
  FormatDecimal(value: number | undefined | null, decimals: number): string {
    if (value == null) return '0';
    return value.toFixed(decimals);
  FormatRelativeTime(date: Date): string {
    const then = date instanceof Date ? date.getTime() : new Date(date).getTime();
    const diffMs = now - then;
    if (diffMin < 1) return 'just now';
    const diffHours = Math.floor(diffMin / 60);
    const diffWeeks = Math.floor(diffDays / 7);
    if (diffWeeks < 4) return `${diffWeeks}w ago`;
    const diffMonths = Math.floor(diffDays / 30);
    return `${diffMonths}mo ago`;
  //  Private helpers
    this.FormState = { rating: null, isCorrect: null, comments: '' };
  private applyHistoryFilters(): void {
    let items = [...this.HistoryItems];
    if (this.HistorySearchText) {
      const term = this.HistorySearchText.toLowerCase();
      items = items.filter(i => i.testName.toLowerCase().includes(term));
    items.sort((a, b) => {
      switch (this.HistorySortField) {
          return b.runDateTime.getTime() - a.runDateTime.getTime();
        case 'rating':
          return (b.humanRating ?? 0) - (a.humanRating ?? 0);
        case 'test-name':
          return a.testName.localeCompare(b.testName);
    this.FilteredHistoryItems = items;
    this.stateChange.emit({ viewMode: this.CurrentView });
