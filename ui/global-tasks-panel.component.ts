 * Global floating tasks panel that shows all active tasks across all conversations
 * Appears in bottom-right corner, minimizable, shows only when tasks > 0
  selector: 'mj-global-tasks-panel',
    @if (tasks.length > 0) {
      <div class="global-tasks-panel">
        <!-- Minimized State -->
        @if (isMinimized) {
            class="minimized-badge"
            (click)="expand()"
            title="View active tasks">
            <span class="task-count">{{ tasks.length }}</span>
        <!-- Expanded State -->
          <div class="expanded-panel">
                <span>Active Tasks ({{ tasks.length }})</span>
              <button class="minimize-btn" (click)="minimize()" title="Minimize">
              @for (task of tasks; track task) {
                <div class="task-item" (click)="onTaskClick(task)">
                  @if (task.conversationName) {
                    <div class="task-conversation">
                      <i class="fas fa-message"></i>
                      <span>{{ task.conversationName }}</span>
                    <span class="status-indicator active"></span>
                    <span class="status-text">{{ getTrimmedStatus(task.status) }}</span>
                  <div class="task-elapsed">{{ getElapsedTime(task) }}</div>
    .global-tasks-panel {
    /* Minimized Badge */
    .minimized-badge {
    .minimized-badge:hover {
      box-shadow: 0 6px 16px rgba(0, 0, 0, 0.2);
    .minimized-badge i {
    .minimized-badge .task-count {
        box-shadow: 0 4px 20px rgba(102, 126, 234, 0.4);
    /* Expanded Panel */
    .expanded-panel {
      max-height: 480px;
      animation: slideIn 0.3s ease-out;
        transform: translateY(20px) scale(0.95);
    .header-left i {
    .minimize-btn {
    .minimize-btn:hover {
      max-height: 420px;
    .task-item:hover {
    .task-conversation {
    .task-conversation i {
      animation: blink 2s ease-in-out infinite;
    .status-text {
      font-variant-numeric: tabular-nums;
    .panel-content::-webkit-scrollbar {
    .panel-content::-webkit-scrollbar-track {
    .panel-content::-webkit-scrollbar-thumb {
    .panel-content::-webkit-scrollbar-thumb:hover {
export class GlobalTasksPanelComponent implements OnInit, OnDestroy {
  public tasks: ActiveTask[] = [];
  @Output() taskClicked = new EventEmitter<ActiveTask>();
  constructor(private activeTasksService: ActiveTasksService) {}
    // Subscribe to active tasks
    this.activeTasksService.tasks$.pipe(
    ).subscribe(tasks => {
      this.tasks = tasks;
      // Auto-expand if tasks appear and panel was minimized
      if (this.tasks.length > 0 && this.isMinimized) {
        // Keep minimized, let user expand manually
  expand() {
  onTaskClick(task: ActiveTask) {
    // Remove emojis and trim to 50 chars
    const cleaned = status.replace(/[\u{1F600}-\u{1F64F}]/gu, '').trim();
    return cleaned.length > 50 ? cleaned.substring(0, 47) + '...' : cleaned;
