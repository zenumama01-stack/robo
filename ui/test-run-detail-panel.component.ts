import { TestRunSummary } from '../../services/testing-instrumentation.service';
import { OracleResult } from './oracle-breakdown-table.component';
  selector: 'app-test-run-detail-panel',
    @if (testRun) {
      <div class="test-run-detail-panel">
            <h3>{{ testRun.testName }}</h3>
            <div class="header-meta">
                {{ testRun.testType }}
              <span class="run-time">
                {{ testRun.runDateTime | date:'medium' }}
            <app-test-status-badge [status]="testRun.status"></app-test-status-badge>
            @if (closeable) {
              <button class="close-btn" (click)="onClose()">
          <!-- Main Metrics -->
          <div class="metrics-section">
              <app-score-indicator [score]="testRun.score" [showBar]="true" [showIcon]="true"></app-score-indicator>
              <app-cost-display [cost]="testRun.cost" [showIcon]="true"></app-cost-display>
              <div class="metric-value">{{ formatDuration(testRun.duration) }}</div>
            @if (testRun.targetType) {
                <div class="metric-label">Target</div>
                <div class="metric-value target-link" (click)="onViewTarget()">
                  {{ testRun.targetType }}
          <!-- Oracle Breakdown -->
          @if (oracleResults && oracleResults.length > 0) {
            <div class="oracle-section">
              <app-oracle-breakdown-table [results]="oracleResults"></app-oracle-breakdown-table>
          @if (resultDetails) {
                  Result Details
                <button class="toggle-btn" (click)="toggleResultDetails()">
                  <i class="fa-solid" [class.fa-chevron-down]="!showResultDetails" [class.fa-chevron-up]="showResultDetails"></i>
              @if (showResultDetails) {
                  <pre class="json-viewer">{{ formatJSON(resultDetails) }}</pre>
          <!-- Feedback Section -->
                Human Feedback
                  <label>Rating (1-10)</label>
                    [(ngModel)]="feedbackRating"
                    max="10"
                    class="rating-input"
                      <input type="checkbox" [(ngModel)]="feedbackIsCorrect" />
                      <span>Yes, the automated result is correct</span>
                  [(ngModel)]="feedbackComments"
                  class="comments-textarea"
                  placeholder="Enter your feedback comments..."
              <button class="submit-btn" (click)="onSubmitFeedback()" [disabled]="submittingFeedback">
                {{ submittingFeedback ? 'Submitting...' : 'Submit Feedback' }}
    .test-run-detail-panel {
    .header-meta span {
    .metrics-section {
      border-left: 4px solid #2196f3;
    .target-link {
    .target-link:hover {
    .oracle-section,
    .details-section,
    .section-header h4 i {
    .details-content {
    .rating-input {
    .comments-textarea {
export class TestRunDetailPanelComponent {
  @Input() testRun!: TestRunSummary;
  @Input() oracleResults: OracleResult[] = [];
  @Input() resultDetails: any = null;
  @Input() closeable = true;
  @Output() viewTarget = new EventEmitter<{ type: string; id: string }>();
  @Output() submitFeedback = new EventEmitter<{
    rating: number;
    isCorrect: boolean;
  showResultDetails = false;
  feedbackIsCorrect = true;
  submittingFeedback = false;
    if (minutes > 0) {
  formatJSON(obj: any): string {
      return JSON.stringify(obj, null, 2);
      return String(obj);
  toggleResultDetails(): void {
    this.showResultDetails = !this.showResultDetails;
  onClose(): void {
  onViewTarget(): void {
    if (this.testRun.targetType && this.testRun.targetLogID) {
      this.viewTarget.emit({
        type: this.testRun.targetType,
        id: this.testRun.targetLogID
  async onSubmitFeedback(): Promise<void> {
    this.submittingFeedback = true;
    this.submitFeedback.emit({
      rating: this.feedbackRating,
      isCorrect: this.feedbackIsCorrect,
      comments: this.feedbackComments
    // Reset after submission
      this.submittingFeedback = false;
      this.feedbackIsCorrect = true;
