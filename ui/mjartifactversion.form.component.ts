import { MJArtifactVersionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Artifact Versions') // Tell MemberJunction about this class
    selector: 'gen-mjartifactversion-form',
    templateUrl: './mjartifactversion.form.component.html'
export class MJArtifactVersionFormComponent extends BaseFormComponent {
    public record!: MJArtifactVersionEntity;
            { sectionKey: 'versionIdentity', sectionName: 'Version Identity', isExpanded: true },
            { sectionKey: 'contentMetadata', sectionName: 'Content & Metadata', isExpanded: false },
            { sectionKey: 'ownershipAttribution', sectionName: 'Ownership & Attribution', isExpanded: false },
            { sectionKey: 'mJArtifactVersionAttributes', sectionName: 'MJ: Artifact Version Attributes', isExpanded: false },
            { sectionKey: 'mJArtifactUses', sectionName: 'MJ: Artifact Uses', isExpanded: false },
            { sectionKey: 'mJConversationDetailArtifacts', sectionName: 'MJ: Conversation Detail Artifacts', isExpanded: false }
