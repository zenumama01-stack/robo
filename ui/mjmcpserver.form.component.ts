import { MJMCPServerEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: MCP Servers') // Tell MemberJunction about this class
    selector: 'gen-mjmcpserver-form',
    templateUrl: './mjmcpserver.form.component.html'
export class MJMCPServerFormComponent extends BaseFormComponent {
    public record!: MJMCPServerEntity;
            { sectionKey: 'serverIdentificationDetails', sectionName: 'Server Identification & Details', isExpanded: true },
            { sectionKey: 'connectionSettings', sectionName: 'Connection Settings', isExpanded: true },
            { sectionKey: 'authenticationCredentials', sectionName: 'Authentication & Credentials', isExpanded: false },
            { sectionKey: 'performanceLimits', sectionName: 'Performance & Limits', isExpanded: false },
            { sectionKey: 'mJOAuthClientRegistrations', sectionName: 'MJ: O Auth Client Registrations', isExpanded: false },
