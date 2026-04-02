import { MJActionParamEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Params') // Tell MemberJunction about this class
    selector: 'gen-mjactionparam-form',
    templateUrl: './mjactionparam.form.component.html'
export class MJActionParamFormComponent extends BaseFormComponent {
    public record!: MJActionParamEntity;
            { sectionKey: 'actionAssociation', sectionName: 'Action Association', isExpanded: true },
            { sectionKey: 'parameterDefinition', sectionName: 'Parameter Definition', isExpanded: true },
            { sectionKey: 'entityActionParams', sectionName: 'Entity Action Params', isExpanded: false },
            { sectionKey: 'scheduledActionParams', sectionName: 'Scheduled Action Params', isExpanded: false }
