import { MJAIPromptTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Prompt Types') // Tell MemberJunction about this class
    selector: 'gen-mjaiprompttype-form',
    templateUrl: './mjaiprompttype.form.component.html'
export class MJAIPromptTypeFormComponent extends BaseFormComponent {
    public record!: MJAIPromptTypeEntity;
            { sectionKey: 'promptTypeInformation', sectionName: 'Prompt Type Information', isExpanded: true },
