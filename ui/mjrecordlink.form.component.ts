import { MJRecordLinkEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Record Links') // Tell MemberJunction about this class
    selector: 'gen-mjrecordlink-form',
    templateUrl: './mjrecordlink.form.component.html'
export class MJRecordLinkFormComponent extends BaseFormComponent {
    public record!: MJRecordLinkEntity;
            { sectionKey: 'recordReferences', sectionName: 'Record References', isExpanded: true },
            { sectionKey: 'linkDetails', sectionName: 'Link Details', isExpanded: false },
