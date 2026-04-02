import { MJTemplateCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Template Categories') // Tell MemberJunction about this class
    selector: 'gen-mjtemplatecategory-form',
    templateUrl: './mjtemplatecategory.form.component.html'
export class MJTemplateCategoryFormComponent extends BaseFormComponent {
    public record!: MJTemplateCategoryEntity;
            { sectionKey: 'managementAudit', sectionName: 'Management Audit', isExpanded: false },
            { sectionKey: 'templateCategories', sectionName: 'Template Categories', isExpanded: false },
            { sectionKey: 'templates', sectionName: 'Templates', isExpanded: false }
