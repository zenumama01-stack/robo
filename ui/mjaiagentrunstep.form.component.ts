@RegisterClass(BaseFormComponent, 'MJ: AI Agent Run Steps') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentrunstep-form',
    templateUrl: './mjaiagentrunstep.form.component.html'
export class MJAIAgentRunStepFormComponent extends BaseFormComponent {
    public record!: MJAIAgentRunStepEntity;
            { sectionKey: 'stepIdentificationHierarchy', sectionName: 'Step Identification & Hierarchy', isExpanded: true },
            { sectionKey: 'executionStatusValidation', sectionName: 'Execution Status & Validation', isExpanded: true },
            { sectionKey: 'dataPayload', sectionName: 'Data & Payload', isExpanded: false },
            { sectionKey: 'notesSystemMetadata', sectionName: 'Notes & System Metadata', isExpanded: false },
            { sectionKey: 'mJAIAgentRunSteps', sectionName: 'MJ: AI Agent Run Steps', isExpanded: false }
