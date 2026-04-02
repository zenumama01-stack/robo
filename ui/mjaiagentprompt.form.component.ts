import { MJAIAgentPromptEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Prompts') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentprompt-form',
    templateUrl: './mjaiagentprompt.form.component.html'
export class MJAIAgentPromptFormComponent extends BaseFormComponent {
    public record!: MJAIAgentPromptEntity;
            { sectionKey: 'technicalIdentification', sectionName: 'Technical Identification', isExpanded: true },
            { sectionKey: 'executionConfiguration', sectionName: 'Execution Configuration', isExpanded: true },
            { sectionKey: 'descriptiveLabels', sectionName: 'Descriptive Labels', isExpanded: false },
