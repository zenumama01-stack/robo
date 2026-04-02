import { MJActionCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Categories') // Tell MemberJunction about this class
    selector: 'gen-mjactioncategory-form',
    templateUrl: './mjactioncategory.form.component.html'
export class MJActionCategoryFormComponent extends BaseFormComponent {
    public record!: MJActionCategoryEntity;
            { sectionKey: 'categoryDetails', sectionName: 'Category Details', isExpanded: true },
            { sectionKey: 'hierarchyInformation', sectionName: 'Hierarchy Information', isExpanded: true },
            { sectionKey: 'actionCategories', sectionName: 'Action Categories', isExpanded: false },
            { sectionKey: 'actions', sectionName: 'Actions', isExpanded: false },
            { sectionKey: 'mJMCPServerTools', sectionName: 'MJ: MCP Server Tools', isExpanded: false }
