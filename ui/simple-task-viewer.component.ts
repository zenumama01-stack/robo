import { Component, Input, Output, EventEmitter, OnChanges, HostListener } from '@angular/core';
 * Simple list view for tasks
  selector: 'mj-simple-task-viewer',
  imports: [CommonModule, TaskDetailPanelComponent],
    <div class="simple-task-viewer">
      <div class="list-layout">
        <div class="task-list" [class.with-detail]="selectedTask">
              class="task-item"
              [class.completed]="task.Status === 'Complete'"
              [class.selected]="selectedTask?.ID === task.ID"
              <div class="status-icon" [class.complete]="task.Status === 'Complete'">
                <i class="fas" [ngClass]="getStatusIcon(task.Status)"></i>
                  <div class="task-title-row">
                    <span class="task-title" [class.completed-text]="task.Status === 'Complete'">
                      {{ task.Name }}
                      @if (task.Status === 'Complete') {
                        <i class="fas fa-check completed-check"></i>
                    <!-- Compact progress indicator for all tasks -->
                    @if (task.PercentComplete != null) {
                      <div class="task-progress-compact">
                        <div class="progress-bar-compact">
                          <div class="progress-fill-compact"
                          [class.complete]="task.Status === 'Complete'"></div>
                        <span class="progress-text-compact">{{ task.PercentComplete }}%</span>
                  <span class="task-meta">
                    @if (task.DueAt) {
                      <span class="due-date">
                        <i class="far fa-calendar"></i>
                      <span class="assigned-to">
                        <i class="far fa-user"></i>
                        {{ task.User }}
              <p>No tasks to display</p>
          <div class="list-resizer"
    .simple-task-viewer {
    .list-layout {
    .task-list {
    .task-list.with-detail {
      border-right: 1px solid #E5E7EB;
    .list-resizer {
    .list-resizer:hover {
      box-shadow: 0 2px 8px rgba(59, 130, 246, 0.1);
    .task-item.selected {
    .task-item.completed {
    .status-icon.complete {
    .task-title-row {
    .task-title.completed-text {
      text-decoration-thickness: 1.5px;
    .completed-check {
    .task-meta i {
    .task-progress-compact {
    .progress-bar-compact {
    .progress-fill-compact {
    .progress-fill-compact.complete {
    .progress-text-compact {
export class SimpleTaskViewerComponent implements OnChanges {
    // Tasks are already loaded
  public onTaskClick(task: MJTaskEntity): void {
      case 'Complete': return 'fa-check-circle';
      case 'In Progress': return 'fa-spinner';
      case 'Blocked': return 'fa-ban';
