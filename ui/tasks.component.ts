  selector: 'app-tasks',
    <div class="tasks-container">
      <h2>Tasks</h2>
        <div class="task-item" *ngFor="let task of tasks" [class.completed]="task.completed">
          <input type="checkbox" [checked]="task.completed">
            <h3>{{ task.title }}</h3>
            <p>{{ task.description }}</p>
            <span class="due-date">Due: {{ task.dueDate }}</span>
    .tasks-container {
      &.completed {
      input[type="checkbox"] {
        .due-date {
export class TasksComponent {
  tasks = [
      title: 'Review prototype designs',
      description: 'Check the new UX mockups and provide feedback',
      dueDate: 'Today',
      completed: false
      title: 'Update documentation',
      description: 'Add notes about the new tab system',
      dueDate: 'Tomorrow',
      title: 'Test Golden Layout integration',
      description: 'Verify tab management works correctly',
      dueDate: 'Nov 15',
      completed: true
