 * Enhanced tasks dropdown component for chat header.
 * Shows ALL active tasks across ALL conversations, grouped by current vs other
  selector: 'mj-tasks-dropdown',
    <div class="tasks-dropdown-container">
        class="active-tasks-btn"
        [class.active]="isOpen"
        [class.has-tasks]="allTasks.length > 0"
        [title]="allTasks.length > 0 ? allTasks.length + ' active task' + (allTasks.length > 1 ? 's' : '') : 'View tasks'">
        @if (allTasks.length > 0) {
          <span class="task-count-badge">{{ allTasks.length }}</span>
        <div class="active-tasks-dropdown">
          <div class="dropdown-header">
              @if (allTasks.length === 0) {
              <span>Active Tasks ({{ allTasks.length }})</span>
            <button class="close-btn" (click)="closeDropdown()">
            <!-- Current Conversation Tasks -->
            @if (currentConversationTasks.length > 0) {
              <div class="section">
                  <i class="fas fa-comment"></i>
                  <span>Current Conversation ({{ currentConversationTasks.length }})</span>
                @for (task of currentConversationTasks; track task) {
                  <div class="active-task-item"
                    (click)="onTaskClick(task)">
                    <div class="task-status-indicator active"></div>
                    <div class="task-content">
                      <div class="task-title">
                        @if (getAgentLogoUrl(task.agentName)) {
                            [src]="getAgentLogoUrl(task.agentName)"
                            class="agent-logo"
                            [alt]="task.agentName" />
                        @if (!getAgentLogoUrl(task.agentName)) {
                          [class]="getAgentIconClass(task.agentName)"></i>
                        {{ task.agentName }}
                      <div class="task-status-text">{{ getTrimmedStatus(task.status) }}</div>
            <!-- Other Conversations Tasks -->
            @if (otherConversationTasks.length > 0) {
                  <span>Other Conversations ({{ otherConversationTasks.length }})</span>
                @for (task of otherConversationTasks; track task) {
                  <div class="active-task-item clickable"
                        <span class="go-btn">
                          {{ task.conversationName }}
            <!-- No Tasks State -->
              <div class="no-tasks">
                <p>No active tasks</p>
    .tasks-dropdown-container {
    .active-tasks-btn {
    .active-tasks-btn:hover {
    .active-tasks-btn.active {
    .task-count-badge {
    .active-tasks-dropdown {
      min-width: 420px;
    .dropdown-header {
      color: var(--accent, #1e40af);
    .section:last-child {
    .active-task-item.clickable {
    .active-task-item.clickable:hover {
      border-color: #C7D2FE;
    .active-task-item:last-child {
    .task-status-indicator.active {
    .task-content {
    .task-title i {
    .task-title .agent-logo {
    .go-btn {
    .active-task-item.clickable:hover .go-btn {
    .task-status-text {
    .no-tasks {
    .no-tasks i {
    .no-tasks p {
    mj-task-widget {
    mj-task-widget:last-child {
        max-width: unset;
        max-height: calc(60vh - 48px);
        bottom: 4px;
        max-height: calc(70vh - 44px);
        padding: 30px 12px;
export class TasksDropdownComponent implements OnInit, OnDestroy {
  public isOpen: boolean = false;
  public allTasks: ActiveTask[] = [];
  public currentConversationTasks: ActiveTask[] = [];
  public otherConversationTasks: ActiveTask[] = [];
    private activeTasksService: ActiveTasksService
    // Subscribe to ALL active tasks across ALL conversations
    this.activeTasksService.tasks$
      .subscribe(tasks => {
        this.allTasks = tasks;
        this.groupTasks();
  private groupTasks(): void {
    this.currentConversationTasks = this.allTasks.filter(
      task => task.conversationId === this.conversationId
    this.otherConversationTasks = this.allTasks.filter(
      task => task.conversationId && task.conversationId !== this.conversationId
    this.isOpen = !this.isOpen;
    this.isOpen = false;
  onTaskClick(task: ActiveTask): void {
    // If task is from another conversation, emit navigation event
    if (task.conversationId && task.conversationId !== this.conversationId) {
      this.navigateToConversation.emit({
        taskId: task.id
   * Get agent icon class by looking up agent in AIEngineBase cache
   * Similar to message-item component's aiAgentInfo getter
  getAgentIconClass(agentName: string): string {
    // Look up agent from AIEngineBase cache by name
    if (AIEngineBase.Instance?.Agents) {
      if (agent?.IconClass) {
    // Default fallback icon
    return 'fas fa-robot';
   * Get agent logo URL by looking up agent in AIEngineBase cache
   * Returns null if no logo URL is available
  getAgentLogoUrl(agentName: string): string | null {
        return agent.LogoURL;
