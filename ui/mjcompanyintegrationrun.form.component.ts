import { MJCompanyIntegrationRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Company Integration Runs') // Tell MemberJunction about this class
    selector: 'gen-mjcompanyintegrationrun-form',
    templateUrl: './mjcompanyintegrationrun.form.component.html'
export class MJCompanyIntegrationRunFormComponent extends BaseFormComponent {
    public record!: MJCompanyIntegrationRunEntity;
            { sectionKey: 'runOverview', sectionName: 'Run Overview', isExpanded: true },
            { sectionKey: 'scheduleStatus', sectionName: 'Schedule & Status', isExpanded: true },
            { sectionKey: 'diagnosticDetails', sectionName: 'Diagnostic Details', isExpanded: false },
            { sectionKey: 'companyIntegrationRunAPILogs', sectionName: 'Company Integration Run API Logs', isExpanded: false },
            { sectionKey: 'companyIntegrationRunDetails', sectionName: 'Company Integration Run Details', isExpanded: false },
            { sectionKey: 'errorLogs', sectionName: 'Error Logs', isExpanded: false }
