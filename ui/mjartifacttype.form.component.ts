import { MJArtifactTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Artifact Types') // Tell MemberJunction about this class
    selector: 'gen-mjartifacttype-form',
    templateUrl: './mjartifacttype.form.component.html'
export class MJArtifactTypeFormComponent extends BaseFormComponent {
    public record!: MJArtifactTypeEntity;
            { sectionKey: 'artifactTypeDefinition', sectionName: 'Artifact Type Definition', isExpanded: true },
            { sectionKey: 'hierarchyInheritance', sectionName: 'Hierarchy & Inheritance', isExpanded: true },
            { sectionKey: 'mJConversationArtifacts', sectionName: 'MJ: Conversation Artifacts', isExpanded: false },
            { sectionKey: 'mJArtifactTypes', sectionName: 'MJ: Artifact Types', isExpanded: false },
            { sectionKey: 'mJArtifacts', sectionName: 'MJ: Artifacts', isExpanded: false },
