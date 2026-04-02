 * Scheduling Activity Resource - displays execution history, trends, and job type statistics
@RegisterClass(BaseResourceComponent, 'SchedulingActivityResource')
  selector: 'mj-scheduling-activity-resource',
    <div class="resource-container">
      <app-scheduling-activity></app-scheduling-activity>
    .resource-container {
      padding: 7px;
export class SchedulingActivityResourceComponent extends BaseResourceComponent implements OnInit {
    return 'Activity';
    return 'fa-solid fa-clock-rotate-left';
