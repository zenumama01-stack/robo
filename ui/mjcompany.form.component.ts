import { MJCompanyEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Companies') // Tell MemberJunction about this class
    selector: 'gen-mjcompany-form',
    templateUrl: './mjcompany.form.component.html'
export class MJCompanyFormComponent extends BaseFormComponent {
    public record!: MJCompanyEntity;
            { sectionKey: 'coreCompanyInfo', sectionName: 'Core Company Info', isExpanded: true },
            { sectionKey: 'brandingDigitalPresence', sectionName: 'Branding & Digital Presence', isExpanded: true },
            { sectionKey: 'companyIntegrations', sectionName: 'Company Integrations', isExpanded: false },
            { sectionKey: 'mJEmployees', sectionName: 'MJ: Employees', isExpanded: false },
            { sectionKey: 'workflows', sectionName: 'Workflows', isExpanded: false },
            { sectionKey: 'mJMCPServerConnections', sectionName: 'MJ: MCP Server Connections', isExpanded: false },
            { sectionKey: 'mJAIAgentExamples', sectionName: 'MJ: AI Agent Examples', isExpanded: false }
