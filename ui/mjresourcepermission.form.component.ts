import { MJResourcePermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Resource Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjresourcepermission-form',
    templateUrl: './mjresourcepermission.form.component.html'
export class MJResourcePermissionFormComponent extends BaseFormComponent {
    public record!: MJResourcePermissionEntity;
            { sectionKey: 'recipientAccessScope', sectionName: 'Recipient & Access Scope', isExpanded: true },
            { sectionKey: 'sharingScheduleStatus', sectionName: 'Sharing Schedule & Status', isExpanded: false },
            { sectionKey: 'resourceReference', sectionName: 'Resource Reference', isExpanded: false }
