import { MJConversationArtifactEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Conversation Artifacts') // Tell MemberJunction about this class
    selector: 'gen-mjconversationartifact-form',
    templateUrl: './mjconversationartifact.form.component.html'
export class MJConversationArtifactFormComponent extends BaseFormComponent {
    public record!: MJConversationArtifactEntity;
            { sectionKey: 'conversationContext', sectionName: 'Conversation Context', isExpanded: true },
            { sectionKey: 'mJConversationArtifactPermissions', sectionName: 'MJ: Conversation Artifact Permissions', isExpanded: false },
            { sectionKey: 'mJConversationArtifactVersions', sectionName: 'MJ: Conversation Artifact Versions', isExpanded: false },
            { sectionKey: 'conversationDetails', sectionName: 'Conversation Details', isExpanded: false }
