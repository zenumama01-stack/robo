import { MJCompanyIntegrationRunDetailEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Company Integration Run Details') // Tell MemberJunction about this class
    selector: 'gen-mjcompanyintegrationrundetail-form',
    templateUrl: './mjcompanyintegrationrundetail.form.component.html'
export class MJCompanyIntegrationRunDetailFormComponent extends BaseFormComponent {
    public record!: MJCompanyIntegrationRunDetailEntity;
            { sectionKey: 'identifiersReferences', sectionName: 'Identifiers & References', isExpanded: true },
            { sectionKey: 'operationExecution', sectionName: 'Operation Execution', isExpanded: true },
            { sectionKey: 'runAudit', sectionName: 'Run Audit', isExpanded: false },
