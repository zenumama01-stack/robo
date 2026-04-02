import { MJAIConfigurationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Configurations') // Tell MemberJunction about this class
    selector: 'gen-mjaiconfiguration-form',
    templateUrl: './mjaiconfiguration.form.component.html'
export class MJAIConfigurationFormComponent extends BaseFormComponent {
    public record!: MJAIConfigurationEntity;
            { sectionKey: 'basicInformation', sectionName: 'Basic Information', isExpanded: true },
            { sectionKey: 'configurationSettings', sectionName: 'Configuration Settings', isExpanded: true },
            { sectionKey: 'inheritanceSettings', sectionName: 'Inheritance Settings', isExpanded: false },
            { sectionKey: 'mJAIConfigurationParams', sectionName: 'MJ: AI Configuration Params', isExpanded: false },
            { sectionKey: 'mJAIPromptModels', sectionName: 'MJ: AI Prompt Models', isExpanded: false },
            { sectionKey: 'mJAIConfigurations', sectionName: 'MJ: AI Configurations', isExpanded: false }
