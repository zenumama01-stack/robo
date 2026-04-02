import { MJTestSuiteRunEntity, MJTestSuiteEntity, MJTestRunEntity, MJTestRunFeedbackEntity, MJUserSettingEntity, UserInfoEngine } from '@memberjunction/core-entities';
import { MJTestSuiteRunFormComponent } from '../../generated/Entities/MJTestSuiteRun/mjtestsuiterun.form.component';
  EvaluationPreferences,
  EvaluationMetrics,
  TestRunWithFeedback,
  calculateEvaluationMetrics,
  normalizeExecutionStatus,
  getNeedsReviewItems,
  NeedsReviewItem
@RegisterClass(BaseFormComponent, 'MJ: Test Suite Runs')
  selector: 'mj-test-suite-run-form',
  templateUrl: './test-suite-run-form.component.html',
  styleUrls: ['./test-suite-run-form.component.css'],
export class TestSuiteRunFormComponentExtended extends MJTestSuiteRunFormComponent implements OnInit, OnDestroy {
  public override record!: MJTestSuiteRunEntity;
  loadingTestRuns = false;
  loadingFeedbacks = false;
  feedbacksLoaded = false;
  testSuite: MJTestSuiteEntity | null = null;
  feedbacks: Map<string, MJTestRunFeedbackEntity> = new Map();
  // Tags
  // Inline feedback
  expandedRunId: string | null = null;
  inlineRating: number = 0;
  inlineHoverRating: number = 0;
  inlineIsCorrect: boolean | null = null;
  inlineComments: string = '';
  savingInlineFeedback = false;
  // Filter for test runs
  runStatusFilter: string | null = null;
  // Evaluation system
  evalPreferences: EvaluationPreferences = {
    showExecution: true,
    showHuman: true,
    showAuto: false
  testRunsWithFeedback: TestRunWithFeedback[] = [];
  evaluationMetrics: EvaluationMetrics | null = null;
  needsReviewItems: NeedsReviewItem[] = [];
      this.parseTags();
      // Auto-refresh for running suite executions
  private parseTags(): void {
    // Cmd/Ctrl + Shift + R: Re-run suite
      this.reRunSuite();
    // Number keys for tabs (1-4)
        case '2': this.changeTab('runs'); break;
        case '3': this.changeTab('details'); break;
      // Load test suite
      if (this.record.SuiteID) {
        const suite = await md.GetEntityObject<MJTestSuiteEntity>('MJ: Test Suites');
        if (suite && await suite.Load(this.record.SuiteID)) {
          this.testSuite = suite;
    this.loadingTestRuns = true;
        ExtraFilter: `TestSuiteRunID='${this.record.ID}'`,
        OrderBy: 'Sequence ASC, StartedAt ASC',
      // Also load feedbacks for these runs
        await this.loadFeedbacks();
      this.loadingTestRuns = false;
    if ((tab === 'runs' || tab === 'analytics') && !this.testRunsLoaded) {
      case 'Completed': return 'fa-check-circle';
      case 'Cancelled': return 'fa-ban';
    if (!this.record.TotalDurationSeconds) return 'N/A';
    const seconds = this.record.TotalDurationSeconds;
    const total = this.record.TotalTests || 0;
    const passed = this.record.PassedTests || 0;
  openTestSuite() {
    if (this.testSuite) {
      SharedService.Instance.OpenEntityRecord('MJ: Test Suites', CompositeKey.FromID(this.testSuite.ID));
  async reRunSuite() {
    if (!this.record.SuiteID) {
      SharedService.Instance.CreateSimpleNotification('Cannot re-run: Suite ID not available', 'error', 3000);
    this.testingDialogService.OpenSuiteDialog(this.record.SuiteID, this.viewContainerRef);
  getRunStatusIcon(status: string): string {
      case 'Passed': return 'fa-check';
      case 'Failed': return 'fa-times';
      case 'Error': return 'fa-exclamation';
      case 'Timeout': return 'fa-clock';
  // Tag Management
    this.parseTags(); // Reset to original
        SharedService.Instance.CreateSimpleNotification('Tags saved successfully', 'success', 2000);
        SharedService.Instance.CreateSimpleNotification(
          this.record.LatestResult?.Message || 'Failed to save tags',
  // Test Run Filtering
  setRunStatusFilter(status: string | null): void {
    this.runStatusFilter = status;
  getFilteredTestRuns(): MJTestRunEntity[] {
    if (!this.runStatusFilter) return this.testRuns;
    return this.testRuns.filter(run => run.Status === this.runStatusFilter);
  getRunCountByStatus(status: string): number {
    return this.testRuns.filter(run => run.Status === status).length;
  // Inline Feedback
  private async loadFeedbacks(): Promise<void> {
    if (this.feedbacksLoaded) return;
    this.loadingFeedbacks = true;
      const testRunIds = this.testRuns.map(r => `'${r.ID}'`).join(',');
      if (!testRunIds) return;
        ExtraFilter: `TestRunID IN (${testRunIds})`,
        this.feedbacks.clear();
          this.feedbacks.set(feedback.TestRunID, feedback);
      // Build TestRunWithFeedback array and calculate metrics
      this.buildTestRunsWithFeedback();
      this.feedbacksLoaded = true;
      console.error('Error loading feedbacks:', error);
      this.loadingFeedbacks = false;
   * Build TestRunWithFeedback array from testRuns and feedbacks
  private buildTestRunsWithFeedback(): void {
    this.testRunsWithFeedback = this.testRuns.map(run => {
      const feedback = this.feedbacks.get(run.ID);
        id: run.ID,
        testId: run.TestID,
        testName: run.Test || 'Unknown Test',
        executionStatus: normalizeExecutionStatus(run.Status || 'Completed'),
        originalStatus: run.Status || 'Completed',
        duration: (run.DurationSeconds || 0) * 1000, // Convert to ms
        cost: run.CostUSD || 0,
        runDateTime: run.StartedAt ? new Date(run.StartedAt) : new Date(),
        autoScore: run.Score,
        passedChecks: null,
        failedChecks: null,
        totalChecks: null,
        humanRating: feedback?.Rating || null,
        humanIsCorrect: feedback?.IsCorrect ?? null,
        humanComments: feedback?.CorrectionSummary || null,
        hasHumanFeedback: !!feedback,
        feedbackId: feedback?.ID || null,
        targetType: null,
        targetLogID: null
    // Calculate metrics
    this.evaluationMetrics = calculateEvaluationMetrics(this.testRunsWithFeedback);
    // Get items needing review
    this.needsReviewItems = getNeedsReviewItems(this.testRunsWithFeedback);
  toggleRunExpanded(runId: string): void {
    if (this.expandedRunId === runId) {
      this.expandedRunId = null;
      this.expandedRunId = runId;
      this.initializeInlineFeedback(runId);
  private initializeInlineFeedback(runId: string): void {
    const existingFeedback = this.feedbacks.get(runId);
    if (existingFeedback) {
      this.inlineRating = existingFeedback.Rating || 0;
      this.inlineIsCorrect = existingFeedback.IsCorrect;
      this.inlineComments = existingFeedback.CorrectionSummary || '';
      this.inlineRating = 0;
      this.inlineIsCorrect = null;
      this.inlineComments = '';
    this.inlineHoverRating = 0;
  setInlineRating(value: number): void {
    this.inlineRating = value;
  getInlineRatingLabel(): string {
    if (this.inlineRating <= 3) return 'Poor';
    if (this.inlineRating <= 5) return 'Below Average';
    if (this.inlineRating <= 6) return 'Average';
    if (this.inlineRating <= 7) return 'Good';
    if (this.inlineRating <= 8) return 'Very Good';
    if (this.inlineRating <= 9) return 'Excellent';
    return 'Outstanding';
  canSubmitInlineFeedback(): boolean {
    return this.inlineRating > 0 && this.inlineComments.trim().length > 0;
  async saveInlineFeedback(): Promise<void> {
    if (!this.expandedRunId || !this.canSubmitInlineFeedback()) return;
    this.savingInlineFeedback = true;
      const currentUser = md.CurrentUser;
      let feedback = this.feedbacks.get(this.expandedRunId);
      if (!feedback) {
        feedback = await md.GetEntityObject<MJTestRunFeedbackEntity>('MJ: Test Run Feedbacks', currentUser);
        feedback.TestRunID = this.expandedRunId;
        feedback.ReviewerUserID = currentUser.ID;
      feedback.Rating = this.inlineRating;
      feedback.IsCorrect = this.inlineIsCorrect;
      feedback.CorrectionSummary = this.inlineComments.trim() || null;
      const result = await feedback.Save();
        this.feedbacks.set(this.expandedRunId, feedback);
        // Rebuild the metrics after feedback update
        SharedService.Instance.CreateSimpleNotification('Feedback saved', 'success', 2000);
          feedback.LatestResult?.Message || 'Failed to save feedback',
      SharedService.Instance.CreateSimpleNotification('Failed to save feedback', 'error', 3000);
      this.savingInlineFeedback = false;
  hasFeedback(runId: string): boolean {
    return this.feedbacks.has(runId);
  getFeedbackRating(runId: string): number {
    return this.feedbacks.get(runId)?.Rating || 0;
   * Get TestRunWithFeedback by run ID for template binding
  getRunWithFeedback(runId: string): TestRunWithFeedback | undefined {
    return this.testRunsWithFeedback.find(r => r.id === runId);
   * Get the human correctness status for a run
  getHumanIsCorrect(runId: string): boolean | null {
    return this.feedbacks.get(runId)?.IsCorrect ?? null;
  // Run Tags
  // Analytics Calculations
  getPassedCount(): number {
    return this.testRuns.filter(r => r.Status === 'Passed').length;
  getFailedCount(): number {
    return this.testRuns.filter(r => r.Status === 'Failed' || r.Status === 'Error').length;
  getPassedPercent(): number {
    return (this.getPassedCount() / this.testRuns.length) * 100;
  getFailedPercent(): number {
    return (this.getFailedCount() / this.testRuns.length) * 100;
  getAverageScore(): number {
    const runsWithScore = this.testRuns.filter(r => r.Score != null);
    if (runsWithScore.length === 0) return 0;
    const sum = runsWithScore.reduce((acc, r) => acc + (r.Score || 0), 0);
    return sum / runsWithScore.length;
    const runsWithDuration = this.testRuns.filter(r => r.DurationSeconds != null);
    if (runsWithDuration.length === 0) return 0;
    const sum = runsWithDuration.reduce((acc, r) => acc + (r.DurationSeconds || 0), 0);
    return sum / runsWithDuration.length;
    return this.testRuns.reduce((acc, r) => acc + (r.CostUSD || 0), 0);
  // Export
  exportToCSV(): void {
    const headers = ['Test Name', 'Status', 'Score', 'Duration (s)', 'Cost (USD)', 'Started At', 'Tags'];
    const rows = this.testRuns.map(run => [
      run.Test || '',
      run.Status || '',
      run.Score?.toFixed(4) || '',
      run.DurationSeconds?.toFixed(2) || '',
      run.CostUSD?.toFixed(6) || '',
      run.StartedAt ? new Date(run.StartedAt).toISOString() : '',
      this.getRunTags(run).join('; ')
    link.download = `test-suite-run-${this.record.ID.substring(0, 8)}-results.csv`;
