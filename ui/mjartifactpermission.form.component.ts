import { MJArtifactPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Artifact Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjartifactpermission-form',
    templateUrl: './mjartifactpermission.form.component.html'
export class MJArtifactPermissionFormComponent extends BaseFormComponent {
    public record!: MJArtifactPermissionEntity;
            { sectionKey: 'permissionRelationships', sectionName: 'Permission Relationships', isExpanded: true },
            { sectionKey: 'artifactAccessRights', sectionName: 'Artifact Access Rights', isExpanded: true },
