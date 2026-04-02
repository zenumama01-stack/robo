import { MJScheduledJobRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Scheduled Job Runs') // Tell MemberJunction about this class
    selector: 'gen-mjscheduledjobrun-form',
    templateUrl: './mjscheduledjobrun.form.component.html'
export class MJScheduledJobRunFormComponent extends BaseFormComponent {
    public record!: MJScheduledJobRunEntity;
            { sectionKey: 'timingQueue', sectionName: 'Timing & Queue', isExpanded: true },
            { sectionKey: 'outcomeStatus', sectionName: 'Outcome & Status', isExpanded: false },
