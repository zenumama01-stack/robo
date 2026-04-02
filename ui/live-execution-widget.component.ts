import { LiveExecution } from '../../services/ai-instrumentation.service';
  selector: 'app-live-execution-widget',
    <div class="live-execution-widget">
      <div class="widget-header">
        <h3 class="widget-title">
        <div class="active-count" [class.pulsing]="hasActiveExecutions()">
          {{ getActiveCount() }} active
      @if (executions.length > 0) {
        <div class="execution-list">
          @for (execution of executions.slice(0, maxVisible); track execution.id) {
              class="execution-item"
              [class]="'execution-item--' + execution.status"
              (click)="onExecutionClick(execution)"
          <div class="execution-icon">
            <i [class]="getExecutionIcon(execution)"></i>
            <div class="execution-name">{{ execution.name }}</div>
            <div class="execution-meta">
              <span class="execution-type">{{ execution.type }}</span>
              <span class="execution-duration">{{ formatDuration(execution.duration) }}</span>
              @if (execution.cost) {
                <span class="execution-cost">
                  {{ formatCurrency(execution.cost) }}
            @if (execution.status === 'running' && execution.progress) {
              <div class="progress-ring">
              <svg width="24" height="24">
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="#e0e0e0"
                  stroke-width="2"
                  stroke="#2196f3"
                  stroke-linecap="round"
                  [style.stroke-dasharray]="circumference"
                  [style.stroke-dashoffset]="getProgressOffset(execution.progress)"
                  class="progress-circle"
                <span class="progress-text">{{ execution.progress.toFixed(0) }}%</span>
            <div class="status-indicator" [class]="'status-indicator--' + execution.status">
              <i [class]="getStatusIcon(execution.status)"></i>
          @if (executions.length > maxVisible) {
            <div class="show-more">
            class="show-more-btn"
            (click)="toggleShowAll()"
              {{ showAll ? 'Show Less' : 'Show All (' + executions.length + ')' }}
              <i [class]="showAll ? 'fa-solid fa-chevron-up' : 'fa-solid fa-chevron-down'"></i>
        <div class="no-executions">
          <p>No recent executions</p>
    .live-execution-widget {
    .widget-header {
      padding: 20px 20px 16px;
    .widget-title {
    .widget-title i {
    .active-count {
    .active-count.pulsing {
      animation: pulse 2s infinite;
      0% { opacity: 1; }
      100% { opacity: 1; }
    .execution-list {
    .execution-item {
    .execution-item:hover {
    .execution-item--running {
    .execution-item--completed {
      border-left-color: #4caf50;
    .execution-item--failed {
      border-left-color: #f44336;
      background: rgba(244, 67, 54, 0.02);
    .execution-icon {
    .execution-item--running .execution-icon {
    .execution-item--completed .execution-icon {
      background: rgba(76, 175, 80, 0.1);
    .execution-item--failed .execution-icon {
      background: rgba(244, 67, 54, 0.1);
    .execution-name {
    .execution-meta {
    .execution-type {
    .execution-duration {
    .execution-cost {
      color: #ff9800;
    .progress-ring {
    .progress-circle {
      transform-origin: center;
      transition: stroke-dashoffset 0.3s ease;
    .progress-text {
    .status-indicator--running {
    .status-indicator--completed {
    .status-indicator--failed {
    .show-more {
    .show-more-btn {
    .show-more-btn:hover {
    .no-executions {
    .no-executions i {
    .no-executions p {
    /* Custom scrollbar */
    .execution-list::-webkit-scrollbar {
    .execution-list::-webkit-scrollbar-track {
    .execution-list::-webkit-scrollbar-thumb {
      background: #ccc;
    .execution-list::-webkit-scrollbar-thumb:hover {
      background: #999;
export class LiveExecutionWidgetComponent implements OnInit, OnDestroy {
  @Input() executions: LiveExecution[] = [];
  @Input() maxVisible = 8;
  @Output() executionClick = new EventEmitter<LiveExecution>();
  showAll = false;
  circumference = 2 * Math.PI * 10; // r=10
  ngOnInit() {}
  ngOnDestroy() {}
  trackByExecutionId(index: number, execution: LiveExecution): string {
    return execution.id;
  hasActiveExecutions(): boolean {
    return this.executions.some(e => e.status === 'running');
  getActiveCount(): number {
    return this.executions.filter(e => e.status === 'running').length;
  getExecutionIcon(execution: LiveExecution): string {
    if (execution.type === 'agent') {
        return 'fa-solid fa-play';
        return 'fa-solid fa-check';
        return 'fa-solid fa-times';
        return 'fa-solid fa-question';
  formatDuration(duration?: number): string {
    if (!duration) return '0s';
    const seconds = Math.floor(duration / 1000);
      return `${hours}h ${minutes % 60}m`;
  getProgressOffset(progress: number): number {
    return this.circumference - (progress / 100) * this.circumference;
  toggleShowAll(): void {
    this.showAll = !this.showAll;
    this.maxVisible = this.showAll ? this.executions.length : 8;
    this.executionClick.emit(execution);
  formatCurrency(amount: number): string {
    return `$${amount.toFixed(4)}`;
