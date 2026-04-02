import { MJCollectionArtifactEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Collection Artifacts') // Tell MemberJunction about this class
    selector: 'gen-mjcollectionartifact-form',
    templateUrl: './mjcollectionartifact.form.component.html'
export class MJCollectionArtifactFormComponent extends BaseFormComponent {
    public record!: MJCollectionArtifactEntity;
            { sectionKey: 'linkIdentifiers', sectionName: 'Link Identifiers', isExpanded: true },
            { sectionKey: 'linkDetails', sectionName: 'Link Details', isExpanded: true },
