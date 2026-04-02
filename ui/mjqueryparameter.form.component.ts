import { MJQueryParameterEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Query Parameters') // Tell MemberJunction about this class
    selector: 'gen-mjqueryparameter-form',
    templateUrl: './mjqueryparameter.form.component.html'
export class MJQueryParameterFormComponent extends BaseFormComponent {
    public record!: MJQueryParameterEntity;
            { sectionKey: 'parameterCore', sectionName: 'Parameter Core', isExpanded: true },
            { sectionKey: 'parameterGuidanceValidation', sectionName: 'Parameter Guidance & Validation', isExpanded: true },
