import { MJMCPServerConnectionPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: MCP Server Connection Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjmcpserverconnectionpermission-form',
    templateUrl: './mjmcpserverconnectionpermission.form.component.html'
export class MJMCPServerConnectionPermissionFormComponent extends BaseFormComponent {
    public record!: MJMCPServerConnectionPermissionEntity;
            { sectionKey: 'connectionPermissions', sectionName: 'Connection Permissions', isExpanded: true },
            { sectionKey: 'accessAssignment', sectionName: 'Access Assignment', isExpanded: true },
