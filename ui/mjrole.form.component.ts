import { MJRoleEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Roles') // Tell MemberJunction about this class
    selector: 'gen-mjrole-form',
    templateUrl: './mjrole.form.component.html'
export class MJRoleFormComponent extends BaseFormComponent {
    public record!: MJRoleEntity;
            { sectionKey: 'coreRoleDetails', sectionName: 'Core Role Details', isExpanded: true },
            { sectionKey: 'authorizationRoles', sectionName: 'Authorization Roles', isExpanded: false },
            { sectionKey: 'entityPermissions', sectionName: 'Entity Permissions', isExpanded: false },
            { sectionKey: 'userRoles', sectionName: 'User Roles', isExpanded: false },
            { sectionKey: 'mJMCPServerConnectionPermissions', sectionName: 'MJ: MCP Server Connection Permissions', isExpanded: false },
            { sectionKey: 'resourcePermissions', sectionName: 'Resource Permissions', isExpanded: false },
            { sectionKey: 'mJAIAgentPermissions', sectionName: 'MJ: AI Agent Permissions', isExpanded: false }
