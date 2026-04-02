import { MJConversationArtifactPermissionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Conversation Artifact Permissions') // Tell MemberJunction about this class
    selector: 'gen-mjconversationartifactpermission-form',
    templateUrl: './mjconversationartifactpermission.form.component.html'
export class MJConversationArtifactPermissionFormComponent extends BaseFormComponent {
    public record!: MJConversationArtifactPermissionEntity;
