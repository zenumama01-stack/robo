import { MJUserRecordLogEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Record Logs') // Tell MemberJunction about this class
    selector: 'gen-mjuserrecordlog-form',
    templateUrl: './mjuserrecordlog.form.component.html'
export class MJUserRecordLogFormComponent extends BaseFormComponent {
    public record!: MJUserRecordLogEntity;
            { sectionKey: 'interactionSummary', sectionName: 'Interaction Summary', isExpanded: true },
            { sectionKey: 'userProfile', sectionName: 'User Profile', isExpanded: false },
