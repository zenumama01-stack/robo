import { MJAIAgentModelEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Models') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentmodel-form',
    templateUrl: './mjaiagentmodel.form.component.html'
export class MJAIAgentModelFormComponent extends BaseFormComponent {
    public record!: MJAIAgentModelEntity;
            { sectionKey: 'mappingIdentifiers', sectionName: 'Mapping Identifiers', isExpanded: true },
            { sectionKey: 'agentModelConfiguration', sectionName: 'Agent Model Configuration', isExpanded: true },
