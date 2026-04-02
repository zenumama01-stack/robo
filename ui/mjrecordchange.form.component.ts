import { MJRecordChangeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Record Changes') // Tell MemberJunction about this class
    selector: 'gen-mjrecordchange-form',
    templateUrl: './mjrecordchange.form.component.html'
export class MJRecordChangeFormComponent extends BaseFormComponent {
    public record!: MJRecordChangeEntity;
            { sectionKey: 'recordContext', sectionName: 'Record Context', isExpanded: true },
            { sectionKey: 'changeSummary', sectionName: 'Change Summary', isExpanded: true },
            { sectionKey: 'changeContent', sectionName: 'Change Content', isExpanded: false },
            { sectionKey: 'mJVersionLabelItems', sectionName: 'MJ: Version Label Items', isExpanded: false }
