import { MJAIAgentArtifactTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Artifact Types') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentartifacttype-form',
    templateUrl: './mjaiagentartifacttype.form.component.html'
export class MJAIAgentArtifactTypeFormComponent extends BaseFormComponent {
    public record!: MJAIAgentArtifactTypeEntity;
            { sectionKey: 'linkDefinition', sectionName: 'Link Definition', isExpanded: true },
            { sectionKey: 'displayNames', sectionName: 'Display Names', isExpanded: true },
