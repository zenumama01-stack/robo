import { MJMCPServerConnectionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: MCP Server Connections') // Tell MemberJunction about this class
    selector: 'gen-mjmcpserverconnection-form',
    templateUrl: './mjmcpserverconnection.form.component.html'
export class MJMCPServerConnectionFormComponent extends BaseFormComponent {
    public record!: MJMCPServerConnectionEntity;
            { sectionKey: 'automationControls', sectionName: 'Automation Controls', isExpanded: true },
            { sectionKey: 'loggingDiagnostics', sectionName: 'Logging & Diagnostics', isExpanded: false },
            { sectionKey: 'mJMCPServerConnectionTools', sectionName: 'MJ: MCP Server Connection Tools', isExpanded: false },
            { sectionKey: 'mJMCPToolExecutionLogs', sectionName: 'MJ: MCP Tool Execution Logs', isExpanded: false },
            { sectionKey: 'mJOAuthAuthorizationStates', sectionName: 'MJ: O Auth Authorization States', isExpanded: false },
            { sectionKey: 'mJMCPServerConnectionPermissions', sectionName: 'MJ: MCP Server Connection Permissions', isExpanded: false }
