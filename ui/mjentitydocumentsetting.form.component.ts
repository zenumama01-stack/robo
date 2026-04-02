import { MJEntityDocumentSettingEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Document Settings') // Tell MemberJunction about this class
    selector: 'gen-mjentitydocumentsetting-form',
    templateUrl: './mjentitydocumentsetting.form.component.html'
export class MJEntityDocumentSettingFormComponent extends BaseFormComponent {
    public record!: MJEntityDocumentSettingEntity;
            { sectionKey: 'documentIdentification', sectionName: 'Document Identification', isExpanded: true },
