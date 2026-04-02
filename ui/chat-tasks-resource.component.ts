import { Component, ViewEncapsulation, OnDestroy, ViewChild } from '@angular/core';
import { EnvironmentEntityExtended } from '@memberjunction/core-entities';
import { TasksFullViewComponent } from '@memberjunction/ng-conversations';
 * Chat Tasks Resource - displays the tasks full view for tab-based display
 * Shows all tasks associated with conversations and artifacts
 * Supports URL deep-linking via taskId query parameter
@RegisterClass(BaseResourceComponent, 'ChatTasksResource')
  selector: 'mj-chat-tasks-resource',
    <div class="chat-tasks-container">
        <mj-tasks-full-view
          #tasksView
          [baseFilter]="'1=1'"
          [activeTaskId]="activeTaskId"
          (taskSelected)="onTaskSelected($any($event))"
          style="height: 100%;">
        </mj-tasks-full-view>
    .chat-tasks-container {
export class ChatTasksResource extends BaseResourceComponent implements OnDestroy {
  @ViewChild('tasksView') tasksView?: TasksFullViewComponent;
  public activeTaskId?: string;
    if (urlState?.taskId) {
      this.activeTaskId = urlState.taskId;
      // Check for navigation params from config
   * Parse URL query string for task state.
   * Query params: taskId
  private parseUrlState(): { taskId?: string } | null {
    const taskId = params.get('taskId');
    if (!taskId) return null;
    return { taskId };
    if (config.taskId) {
      this.activeTaskId = config.taskId as string;
   * Handle task selection from the tasks view.
  onTaskSelected(taskId: string | null): void {
    this.activeTaskId = taskId || undefined;
    if (this.activeTaskId) {
      queryParams['taskId'] = this.activeTaskId;
      queryParams['taskId'] = null;
      // Notify the tasks view component if it exists
      if (this.tasksView) {
        this.tasksView.activeTaskId = urlState.taskId;
      this.activeTaskId = undefined;
        this.tasksView.activeTaskId = undefined;
  private parseUrlFromString(url: string): { taskId?: string } | null {
   * Get the display name for chat tasks
    return 'Tasks';
   * Get the icon class for chat tasks
    return 'fa-solid fa-tasks';
