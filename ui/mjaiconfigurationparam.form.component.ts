import { MJAIConfigurationParamEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Configuration Params') // Tell MemberJunction about this class
    selector: 'gen-mjaiconfigurationparam-form',
    templateUrl: './mjaiconfigurationparam.form.component.html'
export class MJAIConfigurationParamFormComponent extends BaseFormComponent {
    public record!: MJAIConfigurationParamEntity;
            { sectionKey: 'parameterAssignment', sectionName: 'Parameter Assignment', isExpanded: true },
            { sectionKey: 'parameterDetails', sectionName: 'Parameter Details', isExpanded: true },
