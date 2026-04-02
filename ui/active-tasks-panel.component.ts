import { ActiveTasksService, ActiveTask } from '../../services/active-tasks.service';
 * Panel component that displays currently running agent tasks.
 * Shows as a floating panel in bottom-right corner when tasks are active.
  selector: 'mj-active-tasks-panel',
    @if ((taskCount$ | async)! > 0) {
      <div class="active-tasks-panel">
        <div class="panel-header" (click)="toggleExpanded()">
          <span>Active Tasks ({{ taskCount$ | async }})</span>
          <i class="fas" [ngClass]="isExpanded ? 'fa-chevron-up' : 'fa-chevron-down'"></i>
        @if (isExpanded) {
            @for (task of (tasks$ | async); track task) {
              <div class="task-item">
                <div class="task-header">
                  <span class="task-agent">{{ task.agentName }}</span>
                  <span class="task-elapsed">{{ getElapsedTime(task) }}</span>
                <div class="task-status">{{ getTrimmedStatus(task.status) }}</div>
    .active-tasks-panel {
    .panel-header span {
    .panel-header i:last-child {
    .task-item {
      border-bottom: 1px solid #F3F4F6;
    .task-item:last-child {
    .task-header {
    .task-header i {
    .task-elapsed {
      padding-left: 22px;
export class ActiveTasksPanelComponent {
  tasks$: Observable<ActiveTask[]>;
  taskCount$: Observable<number>;
  isExpanded = true;
  constructor(private activeTasksService: ActiveTasksService) {
    this.tasks$ = this.activeTasksService.tasks$;
    this.taskCount$ = this.activeTasksService.taskCount$;
  toggleExpanded(): void {
    this.isExpanded = !this.isExpanded;
  getElapsedTime(task: ActiveTask): string {
    const elapsed = Date.now() - task.startTime;
    return `${minutes}:${remainingSeconds.toString().padStart(2, '0')}`;
  getTrimmedStatus(status: string): string {
    const maxLength = 50;
    if (status.length <= maxLength) {
    return status.substring(0, maxLength) + '...';
