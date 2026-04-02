 * Review status for a test run or set of runs
export type ReviewStatus = 'reviewed' | 'needs-review' | 'not-reviewed';
 * Indicator showing the review/feedback status of a test run.
 * <app-review-status-indicator
 *   [hasReview]="true"
 *   [reviewedCount]="5"
 *   [totalCount]="10"
 *   [mode]="'badge'"
 * ></app-review-status-indicator>
  selector: 'app-review-status-indicator',
    <!-- Badge mode: single status -->
    @if (mode === 'badge') {
      <span class="review-indicator badge" [class]="getStatusClass()">
        <i [class]="getStatusIcon()"></i>
        @if (showText) {
          <span class="text">{{ getStatusText() }}</span>
    <!-- Count mode: X/Y reviewed -->
    @if (mode === 'count') {
      <span class="review-indicator count" [class]="getCountClass()">
        <i [class]="getCountIcon()"></i>
        <span class="numbers">{{ reviewedCount }}/{{ totalCount }}</span>
        @if (showLabel) {
          <span class="label">reviewed</span>
    <!-- Progress mode: visual bar -->
    @if (mode === 'progress') {
      <div class="review-indicator progress">
          <div class="progress-fill" [style.width.%]="getPercentage()"></div>
        <span class="progress-text">{{ reviewedCount }}/{{ totalCount }}</span>
    .review-indicator {
    /* Badge mode */
    .review-indicator.badge {
    .review-indicator.badge.reviewed {
    .review-indicator.badge.needs-review {
    .review-indicator.badge.not-reviewed {
    .review-indicator.badge i {
    /* Count mode */
    .review-indicator.count {
    .review-indicator.count.complete {
    .review-indicator.count.partial {
    .review-indicator.count.none {
    .review-indicator.count i {
    .review-indicator.count .numbers {
    .review-indicator.count .label {
    /* Progress mode */
    .review-indicator.progress {
      background: linear-gradient(90deg, #22c55e 0%, #16a34a 100%);
export class ReviewStatusIndicatorComponent {
  /** Whether this single item has been reviewed */
  @Input() hasReview: boolean = false;
  /** Count of reviewed items (for aggregate display) */
  @Input() reviewedCount: number = 0;
  /** Total count of items (for aggregate display) */
  @Input() totalCount: number = 0;
  /** Display mode */
  @Input() mode: 'badge' | 'count' | 'progress' = 'badge';
  /** Whether to show text label in badge mode */
  @Input() showText: boolean = true;
  /** Whether to show "reviewed" label in count mode */
  @Input() showLabel: boolean = false;
    if (this.hasReview) return 'reviewed';
    return 'not-reviewed';
    if (this.hasReview) return 'fa-solid fa-clipboard-check';
    return 'fa-solid fa-clipboard-question';
    if (this.hasReview) return 'Reviewed';
    return 'Needs Review';
  getCountClass(): string {
    if (this.totalCount === 0) return 'none';
    if (this.reviewedCount >= this.totalCount) return 'complete';
    if (this.reviewedCount > 0) return 'partial';
  getCountIcon(): string {
    if (this.totalCount === 0) return 'fa-solid fa-minus';
    if (this.reviewedCount >= this.totalCount) return 'fa-solid fa-check-circle';
    if (this.reviewedCount > 0) return 'fa-solid fa-clock';
    return 'fa-solid fa-clipboard';
  getPercentage(): number {
    if (this.totalCount === 0) return 0;
    return (this.reviewedCount / this.totalCount) * 100;
