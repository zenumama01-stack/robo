import { MJScheduledJobEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Scheduled Jobs') // Tell MemberJunction about this class
    selector: 'gen-mjscheduledjob-form',
    templateUrl: './mjscheduledjob.form.component.html'
export class MJScheduledJobFormComponent extends BaseFormComponent {
    public record!: MJScheduledJobEntity;
            { sectionKey: 'jobDetails', sectionName: 'Job Details', isExpanded: true },
            { sectionKey: 'scheduleTiming', sectionName: 'Schedule & Timing', isExpanded: true },
            { sectionKey: 'executionMetrics', sectionName: 'Execution Metrics', isExpanded: false },
            { sectionKey: 'notificationSettings', sectionName: 'Notification Settings', isExpanded: false },
            { sectionKey: 'distributedLocking', sectionName: 'Distributed Locking', isExpanded: false },
            { sectionKey: 'mJScheduledJobRuns', sectionName: 'MJ: Scheduled Job Runs', isExpanded: false }
