import { Subject, interval } from 'rxjs';
import { MJTestRunEntity, MJTestEntity, MJTestSuiteRunEntity, MJAIAgentRunEntity, MJAIPromptRunEntity, MJTestRunFeedbackEntity } from '@memberjunction/core-entities';
import { MJTestRunFormComponent } from '../../generated/Entities/MJTestRun/mjtestrun.form.component';
import { TestingDialogService, TagsHelper } from '@memberjunction/ng-testing';
interface ParsedData {
  input?: Record<string, unknown>;
  expected?: Record<string, unknown>;
  actual?: Record<string, unknown>;
  resultDetails?: Record<string, unknown>;
interface CheckResult {
@RegisterClass(BaseFormComponent, 'MJ: Test Runs')
  selector: 'mj-test-run-form',
  templateUrl: './test-run-form.component.html',
  styleUrls: ['./test-run-form.component.css'],
export class TestRunFormComponentExtended extends MJTestRunFormComponent implements OnInit, OnDestroy {
  public override record!: MJTestRunEntity;
  loadingAIRuns = false;
  loadingFeedback = false;
  aiRunsLoaded = false;
  feedbackLoaded = false;
  autoRefreshEnabled = false;
  test: MJTestEntity | null = null;
  testSuiteRun: MJTestSuiteRunEntity | null = null;
  aiAgentRuns: MJAIAgentRunEntity[] = [];
  aiPromptRuns: MJAIPromptRunEntity[] = [];
  feedbacks: MJTestRunFeedbackEntity[] = [];
  // Parsed JSON data
  parsedData: ParsedData = {};
  // Active comparison view
  comparisonView: 'input' | 'expected' | 'actual' = 'input';
  // Keyboard shortcuts active
  // Tags management
  tags: string[] = [];
  newTag = '';
  editingTags = false;
  savingTags = false;
  private originalTags: string[] = [];
      this.loadTags();
      // Auto-refresh for running tests
      if (this.record.Status === 'Running' || this.record.Status === 'Pending') {
        this.startAutoRefresh();
  private loadTags(): void {
    this.tags = TagsHelper.parseTags(this.record.Tags);
    this.originalTags = [...this.tags];
  startEditingTags(): void {
    this.editingTags = true;
  cancelEditingTags(): void {
    this.tags = [...this.originalTags];
    this.newTag = '';
    this.editingTags = false;
  addTag(): void {
    const tag = this.newTag.trim();
    if (tag && !this.tags.includes(tag)) {
      this.tags = [...this.tags, tag];
  removeTag(tag: string): void {
    this.tags = this.tags.filter(t => t !== tag);
  async saveTags(): Promise<void> {
    // Auto-add any pending tag in the input before saving
    const pendingTag = this.newTag.trim();
    if (pendingTag && !this.tags.includes(pendingTag)) {
      this.tags = [...this.tags, pendingTag];
    this.savingTags = true;
      this.record.Tags = TagsHelper.toJson(this.tags);
      const result = await this.record.Save();
        SharedService.Instance.CreateSimpleNotification('Tags saved', 'success', 2000);
        SharedService.Instance.CreateSimpleNotification('Failed to save tags', 'error', 3000);
      this.savingTags = false;
    // Cmd/Ctrl + Shift + R: Re-run test
    if ((event.metaKey || event.ctrlKey) && event.shiftKey && event.key === 'r') {
      this.reRunTest();
        case '2': this.changeTab('details'); break;
        case '3': this.changeTab('ai-runs'); break;
        case '4': this.changeTab('feedback'); break;
        case '5': if (this.record.Log) this.changeTab('log'); break;
  private startAutoRefresh() {
    this.autoRefreshEnabled = true;
    interval(5000)
        if (this.autoRefreshEnabled && (this.record.Status === 'Running' || this.record.Status === 'Pending')) {
          this.silentRefresh();
          this.autoRefreshEnabled = false;
  private async silentRefresh() {
      // Silently fail on auto-refresh
    this.loading = true;
    this.error = null;
      // Load test
      if (this.record.TestID) {
        const test = await md.GetEntityObject<MJTestEntity>('MJ: Tests');
        if (test && await test.Load(this.record.TestID)) {
          this.test = test;
      // Load test suite run if part of a suite
      if (this.record.TestSuiteRunID) {
        const suiteRun = await md.GetEntityObject<MJTestSuiteRunEntity>('MJ: Test Suite Runs');
        if (suiteRun && await suiteRun.Load(this.record.TestSuiteRunID)) {
          this.testSuiteRun = suiteRun;
      this.error = 'Failed to load related data. Click to retry.';
      this.loading = false;
  async retryLoad() {
  private async loadAIRuns() {
    if (this.aiRunsLoaded) return;
    this.loadingAIRuns = true;
      const [agentRuns, promptRuns] = await rv.RunViews([
          ExtraFilter: `TestRunID='${this.record.ID}'`,
          OrderBy: 'StartedAt',
          OrderBy: 'RunAt',
      if (agentRuns.Success) {
        this.aiAgentRuns = agentRuns.Results || [];
      if (promptRuns.Success) {
        this.aiPromptRuns = promptRuns.Results || [];
      this.aiRunsLoaded = true;
      console.error('Error loading AI runs:', error);
      SharedService.Instance.CreateSimpleNotification('Failed to load AI runs', 'error', 3000);
      this.loadingAIRuns = false;
  private async loadFeedback() {
    if (this.feedbackLoaded) return;
    this.loadingFeedback = true;
        this.feedbacks = result.Results || [];
      this.feedbackLoaded = true;
      console.error('Error loading feedback:', error);
      SharedService.Instance.CreateSimpleNotification('Failed to load feedback', 'error', 3000);
      this.loadingFeedback = false;
      if (this.record.InputData) {
        this.parsedData.input = JSON.parse(this.record.InputData);
      if (this.record.ExpectedOutputData) {
        this.parsedData.expected = JSON.parse(this.record.ExpectedOutputData);
      if (this.record.ActualOutputData) {
        this.parsedData.actual = JSON.parse(this.record.ActualOutputData);
      if (this.record.ResultDetails) {
        this.parsedData.resultDetails = JSON.parse(this.record.ResultDetails);
    if (tab === 'ai-runs' && !this.aiRunsLoaded) {
      this.loadAIRuns();
    if (tab === 'feedback' && !this.feedbackLoaded) {
      this.loadFeedback();
  setComparisonView(view: 'input' | 'expected' | 'actual') {
    this.comparisonView = view;
      case 'Skipped': return '#6b7280';
      case 'Passed': return 'fa-check-circle';
      case 'Failed': return 'fa-times-circle';
      case 'Error': return 'fa-exclamation-triangle';
      case 'Timeout': return 'fa-stopwatch';
      case 'Running': return 'fa-circle-notch fa-spin';
      case 'Pending': return 'fa-hourglass-half';
      case 'Skipped': return 'fa-forward';
  calculateDuration(): string {
    if (!this.record.DurationSeconds) return 'N/A';
    const seconds = this.record.DurationSeconds;
  formatScore(score: number | null): string {
    if (score === null || score === undefined) return 'N/A';
    return score.toFixed(4);
    if (cost === null || cost === undefined) return 'N/A';
  getScorePercentage(): number {
    if (this.record.Score === null || this.record.Score === undefined) return 0;
    return Math.round(this.record.Score * 100);
  getPassRatePercentage(): number {
    const total = this.record.TotalChecks || 0;
    const passed = this.record.PassedChecks || 0;
    return Math.round((passed / total) * 100);
  openTest() {
    if (this.test) {
      SharedService.Instance.OpenEntityRecord('MJ: Tests', CompositeKey.FromID(this.test.ID));
  navigateToTestingDashboard() {
  openTestSuiteRun() {
    if (this.testSuiteRun) {
      SharedService.Instance.OpenEntityRecord('MJ: Test Suite Runs', CompositeKey.FromID(this.testSuiteRun.ID));
  openAIAgentRun(runId: string) {
    SharedService.Instance.OpenEntityRecord('MJ: AI Agent Runs', CompositeKey.FromID(runId));
  openAIPromptRun(runId: string) {
  async reRunTest() {
    if (!this.record.TestID) {
      SharedService.Instance.CreateSimpleNotification('Cannot re-run: Test ID not available', 'error', 3000);
    this.testingDialogService.OpenTestDialog(this.record.TestID, this.viewContainerRef);
      this.aiRunsLoaded = false;
      this.feedbackLoaded = false;
      this.aiAgentRuns = [];
      this.aiPromptRuns = [];
      this.feedbacks = [];
      // Reload current tab data if needed
      if (this.activeTab === 'ai-runs') {
        await this.loadAIRuns();
      } else if (this.activeTab === 'feedback') {
        await this.loadFeedback();
  getComparisonData(): string {
    let data: Record<string, unknown> | undefined;
    switch (this.comparisonView) {
      case 'input': data = this.parsedData.input; break;
      case 'expected': data = this.parsedData.expected; break;
      case 'actual': data = this.parsedData.actual; break;
  getCheckResults(): CheckResult[] {
    const details = this.parsedData.resultDetails as Record<string, unknown> | undefined;
    if (!details?.checkResults) return [];
    return details.checkResults as CheckResult[];
    return (passed / total) * 100;
  async copyLogToClipboard(): Promise<void> {
    if (this.record.Log) {
        await navigator.clipboard.writeText(this.record.Log);
        SharedService.Instance.CreateSimpleNotification('Log copied to clipboard', 'success', 2000);
        SharedService.Instance.CreateSimpleNotification('Failed to copy log', 'error', 2000);
  getFormattedResultDetails(): string {
    return this.parsedData.resultDetails
      ? JSON.stringify(this.parsedData.resultDetails, null, 2)
      : '// No result details available';
  // Helper for relative time display
