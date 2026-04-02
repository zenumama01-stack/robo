import { MJGeneratedCodeCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Generated Code Categories') // Tell MemberJunction about this class
    selector: 'gen-mjgeneratedcodecategory-form',
    templateUrl: './mjgeneratedcodecategory.form.component.html'
export class MJGeneratedCodeCategoryFormComponent extends BaseFormComponent {
    public record!: MJGeneratedCodeCategoryEntity;
            { sectionKey: 'hierarchyRelationships', sectionName: 'Hierarchy Relationships', isExpanded: true },
            { sectionKey: 'generatedCodeCategories', sectionName: 'Generated Code Categories', isExpanded: false },
            { sectionKey: 'generatedCodes', sectionName: 'Generated Codes', isExpanded: false }
