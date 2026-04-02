import { MJAIAgentModalityEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Modalities') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentmodality-form',
    templateUrl: './mjaiagentmodality.form.component.html'
export class MJAIAgentModalityFormComponent extends BaseFormComponent {
    public record!: MJAIAgentModalityEntity;
            { sectionKey: 'modalityConfiguration', sectionName: 'Modality Configuration', isExpanded: true },
