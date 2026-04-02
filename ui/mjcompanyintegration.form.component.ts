@RegisterClass(BaseFormComponent, 'MJ: Company Integrations') // Tell MemberJunction about this class
    selector: 'gen-mjcompanyintegration-form',
    templateUrl: './mjcompanyintegration.form.component.html'
export class MJCompanyIntegrationFormComponent extends BaseFormComponent {
    public record!: MJCompanyIntegrationEntity;
            { sectionKey: 'linkingCoreInfo', sectionName: 'Linking & Core Info', isExpanded: true },
            { sectionKey: 'credentialsTokens', sectionName: 'Credentials & Tokens', isExpanded: true },
            { sectionKey: 'externalSystemMapping', sectionName: 'External System Mapping', isExpanded: false },
            { sectionKey: 'runHistoryMonitoring', sectionName: 'Run History & Monitoring', isExpanded: false },
            { sectionKey: 'companyIntegrationRecordMaps', sectionName: 'Company Integration Record Maps', isExpanded: false },
            { sectionKey: 'companyIntegrationRuns', sectionName: 'Company Integration Runs', isExpanded: false },
            { sectionKey: 'employeeCompanyIntegrations', sectionName: 'Employee Company Integrations', isExpanded: false },
            { sectionKey: 'lists', sectionName: 'Lists', isExpanded: false }
