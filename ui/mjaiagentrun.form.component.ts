@RegisterClass(BaseFormComponent, 'MJ: AI Agent Runs') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentrun-form',
    templateUrl: './mjaiagentrun.form.component.html'
export class MJAIAgentRunFormComponent extends BaseFormComponent {
    public record!: MJAIAgentRunEntity;
            { sectionKey: 'runIdentificationHierarchy', sectionName: 'Run Identification & Hierarchy', isExpanded: true },
            { sectionKey: 'executionDetailsOutcome', sectionName: 'Execution Details & Outcome', isExpanded: true },
            { sectionKey: 'contextualRelationships', sectionName: 'Contextual Relationships', isExpanded: false },
            { sectionKey: 'resourceUsageCost', sectionName: 'Resource Usage & Cost', isExpanded: false },
            { sectionKey: 'configurationOverrides', sectionName: 'Configuration & Overrides', isExpanded: false },
            { sectionKey: 'testingValidation', sectionName: 'Testing & Validation', isExpanded: false },
            { sectionKey: 'scopeMultiTenant', sectionName: 'Scope & Multi-Tenant', isExpanded: false },
            { sectionKey: 'details', sectionName: 'Details', isExpanded: false },
            { sectionKey: 'aIAgentNotes', sectionName: 'AI Agent Notes', isExpanded: false },
            { sectionKey: 'mJAIAgentRunMedias', sectionName: 'MJ: AI Agent Run Medias', isExpanded: false },
            { sectionKey: 'mJAIAgentRunSteps', sectionName: 'MJ: AI Agent Run Steps', isExpanded: false },
            { sectionKey: 'mJAIPromptRuns', sectionName: 'MJ: AI Prompt Runs', isExpanded: false }
