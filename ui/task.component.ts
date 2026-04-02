import { TaskViewMode } from '../models/task-view.models';
import { SimpleTaskViewerComponent } from './simple-task-viewer.component';
import { GanttTaskViewerComponent } from './gantt-task-viewer.component';
 * Main task component that composes SimpleTaskViewer and GanttTaskViewer
 * Allows switching between list and Gantt chart views
  selector: 'mj-task',
  imports: [SimpleTaskViewerComponent, GanttTaskViewerComponent],
    <div class="task-component">
      <!-- Header with View Toggle -->
              <h2 class="task-title">{{ title }}</h2>
              <p class="task-description">{{ description }}</p>
          @if (showViewToggle) {
                [class.active]="viewMode === 'gantt'"
                (click)="setViewMode('gantt')"
                title="Gantt Chart">
                <span>Gantt</span>
                [class.active]="viewMode === 'simple'"
                (click)="setViewMode('simple')"
                <span>List</span>
      <!-- Task Viewer (Simple or Gantt) -->
      <div class="task-viewer">
        @if (viewMode === 'simple') {
          <mj-simple-task-viewer
            [tasks]="tasks"
          </mj-simple-task-viewer>
        @if (viewMode === 'gantt') {
          <mj-gantt-task-viewer
            [tasks]="ganttTasks || tasks"
            [taskDependencies]="taskDependencies || []"
          </mj-gantt-task-viewer>
    .task-component {
    .task-viewer {
export class TaskComponent {
  @Input() ganttTasks?: MJTaskEntity[]; // Optional separate task list for Gantt (includes parent)
  @Input() taskDependencies?: MJTaskDependencyEntity[]; // Task dependencies for Gantt links
  @Input() description?: string;
  @Input() showHeader: boolean = true;
  @Input() showViewToggle: boolean = true; // Show Gantt/List toggle
  @Input() viewMode: TaskViewMode = 'simple';
  @Output() viewModeChanged = new EventEmitter<TaskViewMode>();
  public setViewMode(mode: TaskViewMode): void {
    this.viewModeChanged.emit(mode);
  public onTaskClicked(task: MJTaskEntity): void {
