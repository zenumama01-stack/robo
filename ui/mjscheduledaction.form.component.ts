import { MJScheduledActionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Scheduled Actions') // Tell MemberJunction about this class
    selector: 'gen-mjscheduledaction-form',
    templateUrl: './mjscheduledaction.form.component.html'
export class MJScheduledActionFormComponent extends BaseFormComponent {
    public record!: MJScheduledActionEntity;
            { sectionKey: 'actionInformation', sectionName: 'Action Information', isExpanded: true },
            { sectionKey: 'ownershipStatus', sectionName: 'Ownership & Status', isExpanded: true },
            { sectionKey: 'scheduleSettings', sectionName: 'Schedule Settings', isExpanded: false },
