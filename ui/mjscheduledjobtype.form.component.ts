import { MJScheduledJobTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Scheduled Job Types') // Tell MemberJunction about this class
    selector: 'gen-mjscheduledjobtype-form',
    templateUrl: './mjscheduledjobtype.form.component.html'
export class MJScheduledJobTypeFormComponent extends BaseFormComponent {
    public record!: MJScheduledJobTypeEntity;
            { sectionKey: 'jobTypeDetails', sectionName: 'Job Type Details', isExpanded: true },
            { sectionKey: 'executionIntegration', sectionName: 'Execution Integration', isExpanded: true },
            { sectionKey: 'mJScheduledJobs', sectionName: 'MJ: Scheduled Jobs', isExpanded: false }
