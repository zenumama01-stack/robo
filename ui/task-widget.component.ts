 * Reusable task widget component that displays task information
 * in a consistent, polished format across the application.
 * Can be used in:
 * - Tasks dropdown (active tasks)
 * - Gear icon dropdown (tasks for conversation detail)
 * - Tasks tab (full task list with filters)
  selector: 'mj-task-widget',
      class="task-widget"
      [class.clickable]="clickable"
      [class.compact]="compact"
      (click)="onTaskClick()">
      <div class="task-status-indicator" [attr.data-status]="task.Status"></div>
      <!-- Task Content -->
      <div class="task-main">
          <div class="task-title">{{ task.Name }}</div>
          <div class="task-badges">
            @if (task.Type) {
                {{ task.Type }}
            <!-- Status Badge -->
              class="badge badge-status"
              [attr.data-status]="task.Status">
              {{ task.Status }}
        @if (!compact && task.Description) {
          <div class="task-description">
            {{ task.Description }}
        <!-- Progress Bar (if in progress or has completion %) -->
        @if (showProgress && task.PercentComplete != null) {
          <div class="task-progress-container">
                class="progress-fill"
                [style.width.%]="task.PercentComplete"
            <span class="progress-text">{{ task.PercentComplete }}%</span>
        <!-- Meta Information -->
        <div class="task-meta">
          <!-- Assignment -->
          @if (task.User) {
              <span class="meta-label">User:</span>
              <span class="meta-value">{{ task.User }}</span>
          @if (task.Agent) {
              <span class="meta-label">Agent:</span>
              <span class="meta-value">{{ task.Agent }}</span>
          @if (task.StartedAt) {
              <span class="meta-label">Started:</span>
              <span class="meta-value">{{ formatDate(task.StartedAt) }}</span>
          @if (task.CompletedAt) {
              <span class="meta-label">Completed:</span>
              <span class="meta-value">{{ formatDate(task.CompletedAt) }}</span>
          @if (task.DueAt && !task.CompletedAt) {
              <i class="fas fa-calendar-alt"></i>
              <span class="meta-label">Due:</span>
              <span class="meta-value" [class.overdue]="isOverdue(task.DueAt)">
                {{ formatDate(task.DueAt) }}
          <!-- Duration (for completed tasks) -->
          @if (showDuration && task.StartedAt && task.CompletedAt) {
              <span class="meta-label">Duration:</span>
              <span class="meta-value">{{ getDuration(task.StartedAt, task.CompletedAt) }}</span>
          <!-- Elapsed time (for active tasks) -->
          @if (isActive && task.StartedAt && !task.CompletedAt) {
            <span class="meta-item meta-elapsed">
              <span class="meta-value">{{ getElapsedTime(task.StartedAt) }}</span>
    .task-widget {
    .task-widget.clickable {
    .task-widget.clickable:hover {
    .task-widget.compact {
    .task-status-indicator {
    .task-status-indicator[data-status="Pending"] {
      background: #9CA3AF;
    .task-status-indicator[data-status="In Progress"] {
    .task-status-indicator[data-status="Complete"] {
    .task-status-indicator[data-status="Blocked"] {
    .task-status-indicator[data-status="Failed"] {
    .task-status-indicator[data-status="Cancelled"],
    .task-status-indicator[data-status="Deferred"] {
      background: #6B7280;
    .task-main {
    .task-title {
    .task-badges {
    .badge-status[data-status="Pending"] {
    .badge-status[data-status="In Progress"] {
    .badge-status[data-status="Complete"] {
    .badge-status[data-status="Blocked"] {
    .badge-status[data-status="Failed"] {
      color: #7F1D1D;
    .badge-status[data-status="Cancelled"],
    .badge-status[data-status="Deferred"] {
    .task-description {
    .task-progress-container {
      transition: width 300ms ease;
    .progress-fill[data-status="In Progress"] {
    .progress-fill[data-status="Complete"] {
    .progress-fill[data-status="Blocked"],
    .progress-fill[data-status="Failed"] {
    .task-meta {
    .meta-value.overdue {
    .meta-elapsed {
export class TaskWidgetComponent {
  @Input() task!: MJTaskEntity;
  @Input() clickable: boolean = false;
  @Input() compact: boolean = false;
  @Input() showProgress: boolean = true;
  @Input() showDuration: boolean = true;
  @Output() taskClick = new EventEmitter<MJTaskEntity>();
  get isActive(): boolean {
    return this.task.Status === 'In Progress';
  onTaskClick(): void {
    if (this.clickable) {
      this.taskClick.emit(this.task);
  formatDate(date: Date | null): string {
    const taskDate = new Date(date);
    const diffMs = now.getTime() - taskDate.getTime();
    // Less than 1 hour ago
    if (diffMins < 60) {
      return diffMins <= 1 ? 'just now' : `${diffMins}m ago`;
    // Less than 24 hours ago
    if (diffHours < 24) {
      return `${diffHours}h ago`;
    // Less than 7 days ago
    if (diffDays < 7) {
    // Format as date
    return taskDate.toLocaleDateString();
  isOverdue(dueDate: Date | null): boolean {
    if (!dueDate) return false;
    return new Date(dueDate).getTime() < Date.now();
  getDuration(start: Date | null, end: Date | null): string {
    if (!start || !end) return '';
    const diffMs = new Date(end).getTime() - new Date(start).getTime();
    if (diffSecs < 60) {
      return `${diffSecs}s`;
      return `${diffMins}m`;
    const remainingMins = diffMins % 60;
      return remainingMins > 0 ? `${diffHours}h ${remainingMins}m` : `${diffHours}h`;
    const remainingHours = diffHours % 24;
    return remainingHours > 0 ? `${diffDays}d ${remainingHours}h` : `${diffDays}d`;
  getElapsedTime(start: Date | null): string {
    if (!start) return '';
    const diffMs = Date.now() - new Date(start).getTime();
    const remainingSecs = diffSecs % 60;
      return `${diffMins}:${remainingSecs.toString().padStart(2, '0')}`;
    return `${diffHours}:${remainingMins.toString().padStart(2, '0')}`;
