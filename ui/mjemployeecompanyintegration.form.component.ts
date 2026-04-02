import { MJEmployeeCompanyIntegrationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Employee Company Integrations') // Tell MemberJunction about this class
    selector: 'gen-mjemployeecompanyintegration-form',
    templateUrl: './mjemployeecompanyintegration.form.component.html'
export class MJEmployeeCompanyIntegrationFormComponent extends BaseFormComponent {
    public record!: MJEmployeeCompanyIntegrationEntity;
            { sectionKey: 'integrationMapping', sectionName: 'Integration Mapping', isExpanded: true },
            { sectionKey: 'externalIdentifier', sectionName: 'External Identifier', isExpanded: true },
