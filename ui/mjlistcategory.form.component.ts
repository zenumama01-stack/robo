import { MJListCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: List Categories') // Tell MemberJunction about this class
    selector: 'gen-mjlistcategory-form',
    templateUrl: './mjlistcategory.form.component.html'
export class MJListCategoryFormComponent extends BaseFormComponent {
    public record!: MJListCategoryEntity;
            { sectionKey: 'categoryHierarchy', sectionName: 'Category Hierarchy', isExpanded: true },
            { sectionKey: 'ownershipAudit', sectionName: 'Ownership & Audit', isExpanded: false },
            { sectionKey: 'listCategories', sectionName: 'List Categories', isExpanded: false },
