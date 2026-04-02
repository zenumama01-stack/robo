 * Scheduling Jobs Resource - manage and configure scheduled jobs with slideout panels
@RegisterClass(BaseResourceComponent, 'SchedulingJobsResource')
  selector: 'mj-scheduling-jobs-resource',
      <app-scheduling-jobs></app-scheduling-jobs>
export class SchedulingJobsResourceComponent extends BaseResourceComponent implements OnInit {
    return 'Jobs';
    return 'fa-solid fa-calendar-check';
