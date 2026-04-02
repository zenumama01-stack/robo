import { MJConversationArtifactVersionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Conversation Artifact Versions') // Tell MemberJunction about this class
    selector: 'gen-mjconversationartifactversion-form',
    templateUrl: './mjconversationartifactversion.form.component.html'
export class MJConversationArtifactVersionFormComponent extends BaseFormComponent {
    public record!: MJConversationArtifactVersionEntity;
            { sectionKey: 'artifactIdentification', sectionName: 'Artifact Identification', isExpanded: true },
            { sectionKey: 'versionContent', sectionName: 'Version Content', isExpanded: true },
