import { MJArtifactUseEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Artifact Uses') // Tell MemberJunction about this class
    selector: 'gen-mjartifactuse-form',
    templateUrl: './mjartifactuse.form.component.html'
export class MJArtifactUseFormComponent extends BaseFormComponent {
    public record!: MJArtifactUseEntity;
            { sectionKey: 'artifactDetails', sectionName: 'Artifact Details', isExpanded: true },
            { sectionKey: 'userInteraction', sectionName: 'User Interaction', isExpanded: true },
