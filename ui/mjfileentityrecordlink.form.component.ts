import { MJFileEntityRecordLinkEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: File Entity Record Links') // Tell MemberJunction about this class
    selector: 'gen-mjfileentityrecordlink-form',
    templateUrl: './mjfileentityrecordlink.form.component.html'
export class MJFileEntityRecordLinkFormComponent extends BaseFormComponent {
    public record!: MJFileEntityRecordLinkEntity;
            { sectionKey: 'technicalIdentifiers', sectionName: 'Technical Identifiers', isExpanded: true },
