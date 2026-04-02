import { MJCollectionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Collections') // Tell MemberJunction about this class
    selector: 'gen-mjcollection-form',
    templateUrl: './mjcollection.form.component.html'
export class MJCollectionFormComponent extends BaseFormComponent {
    public record!: MJCollectionEntity;
            { sectionKey: 'collectionBasics', sectionName: 'Collection Basics', isExpanded: true },
            { sectionKey: 'structuralHierarchy', sectionName: 'Structural Hierarchy', isExpanded: true },
            { sectionKey: 'ownershipAccess', sectionName: 'Ownership & Access', isExpanded: false },
            { sectionKey: 'mJCollectionPermissions', sectionName: 'MJ: Collection Permissions', isExpanded: false },
            { sectionKey: 'mJCollections', sectionName: 'MJ: Collections', isExpanded: false }
