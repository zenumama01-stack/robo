import { MJCollectionPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Collection Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjcollectionpermission-form',
    templateUrl: './mjcollectionpermission.form.component.html'
export class MJCollectionPermissionFormComponent extends BaseFormComponent {
    public record!: MJCollectionPermissionEntity;
            { sectionKey: 'sharingRelationships', sectionName: 'Sharing Relationships', isExpanded: true },
            { sectionKey: 'permissionSettings', sectionName: 'Permission Settings', isExpanded: true },
