import { MJResourceTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Resource Types') // Tell MemberJunction about this class
    selector: 'gen-mjresourcetype-form',
    templateUrl: './mjresourcetype.form.component.html'
export class MJResourceTypeFormComponent extends BaseFormComponent {
    public record!: MJResourceTypeEntity;
            { sectionKey: 'resourceTypeDefinition', sectionName: 'Resource Type Definition', isExpanded: true },
            { sectionKey: 'entityAssociations', sectionName: 'Entity Associations', isExpanded: false },
            { sectionKey: 'workspaceItems', sectionName: 'Workspace Items', isExpanded: false },
            { sectionKey: 'userNotifications', sectionName: 'User Notifications', isExpanded: false },
            { sectionKey: 'resourceLinks', sectionName: 'Resource Links', isExpanded: false },
            { sectionKey: 'resourcePermissions', sectionName: 'Resource Permissions', isExpanded: false }
