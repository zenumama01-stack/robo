import { MJAIPromptCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Prompt Categories') // Tell MemberJunction about this class
    selector: 'gen-mjaipromptcategory-form',
    templateUrl: './mjaipromptcategory.form.component.html'
export class MJAIPromptCategoryFormComponent extends BaseFormComponent {
    public record!: MJAIPromptCategoryEntity;
            { sectionKey: 'categoryIdentification', sectionName: 'Category Identification', isExpanded: true },
            { sectionKey: 'hierarchyStructure', sectionName: 'Hierarchy Structure', isExpanded: true },
            { sectionKey: 'aIPromptCategories', sectionName: 'AI Prompt Categories', isExpanded: false }
