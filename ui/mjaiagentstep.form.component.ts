import { MJAIAgentStepEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Steps') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentstep-form',
    templateUrl: './mjaiagentstep.form.component.html'
export class MJAIAgentStepFormComponent extends BaseFormComponent {
    public record!: MJAIAgentStepEntity;
            { sectionKey: 'coreDefinition', sectionName: 'Core Definition', isExpanded: true },
            { sectionKey: 'executionControls', sectionName: 'Execution Controls', isExpanded: true },
            { sectionKey: 'targetResources', sectionName: 'Target Resources', isExpanded: false },
            { sectionKey: 'visualLayout', sectionName: 'Visual Layout', isExpanded: false },
            { sectionKey: 'mJAIAgentStepPaths', sectionName: 'MJ: AI Agent Step Paths', isExpanded: false },
            { sectionKey: 'mJAIAgentStepPaths1', sectionName: 'MJ: AI Agent Step Paths', isExpanded: false }
