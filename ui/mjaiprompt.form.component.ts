import { MJAIPromptEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Prompts') // Tell MemberJunction about this class
    selector: 'gen-mjaiprompt-form',
    templateUrl: './mjaiprompt.form.component.html'
export class MJAIPromptFormComponent extends BaseFormComponent {
    public record!: MJAIPromptEntity;
            { sectionKey: 'promptDefinitionMetadata', sectionName: 'Prompt Definition & Metadata', isExpanded: false },
            { sectionKey: 'modelSelectionExecutionSettings', sectionName: 'Model Selection & Execution Settings', isExpanded: true },
            { sectionKey: 'outputValidation', sectionName: 'Output & Validation', isExpanded: false },
            { sectionKey: 'retryFailoverPolicies', sectionName: 'Retry & Failover Policies', isExpanded: false },
            { sectionKey: 'cachingPerformance', sectionName: 'Caching & Performance', isExpanded: false },
            { sectionKey: 'aIAgentActions', sectionName: 'AI Agent Actions', isExpanded: false },
            { sectionKey: 'mJAIAgentTypes', sectionName: 'MJ: AI Agent Types', isExpanded: false },
            { sectionKey: 'mJAIConfigurations', sectionName: 'MJ: AI Configurations', isExpanded: false },
            { sectionKey: 'mJAIConfigurations1', sectionName: 'MJ: AI Configurations', isExpanded: false },
            { sectionKey: 'aIPrompts', sectionName: 'AI Prompts', isExpanded: false },
            { sectionKey: 'aIAgents', sectionName: 'AI Agents', isExpanded: false },
            { sectionKey: 'actions', sectionName: 'Actions', isExpanded: false }
