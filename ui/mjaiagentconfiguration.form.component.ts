import { MJAIAgentConfigurationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Configurations') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentconfiguration-form',
    templateUrl: './mjaiagentconfiguration.form.component.html'
export class MJAIAgentConfigurationFormComponent extends BaseFormComponent {
    public record!: MJAIAgentConfigurationEntity;
            { sectionKey: 'presetDefinition', sectionName: 'Preset Definition', isExpanded: true },
            { sectionKey: 'operationalSettings', sectionName: 'Operational Settings', isExpanded: true },
