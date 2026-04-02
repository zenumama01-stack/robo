import { MJArtifactEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Artifacts') // Tell MemberJunction about this class
    selector: 'gen-mjartifact-form',
    templateUrl: './mjartifact.form.component.html'
export class MJArtifactFormComponent extends BaseFormComponent {
    public record!: MJArtifactEntity;
            { sectionKey: 'ownershipContext', sectionName: 'Ownership & Context', isExpanded: true },
            { sectionKey: 'artifactCore', sectionName: 'Artifact Core', isExpanded: true },
            { sectionKey: 'mJArtifactVersions', sectionName: 'MJ: Artifact Versions', isExpanded: false },
            { sectionKey: 'mJCollectionArtifacts', sectionName: 'MJ: Collection Artifacts', isExpanded: false },
            { sectionKey: 'mJArtifactPermissions', sectionName: 'MJ: Artifact Permissions', isExpanded: false }
