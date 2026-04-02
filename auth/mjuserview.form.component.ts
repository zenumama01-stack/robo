import { MJUserViewEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Views') // Tell MemberJunction about this class
    selector: 'gen-mjuserview-form',
    templateUrl: './mjuserview.form.component.html'
export class MJUserViewFormComponent extends BaseFormComponent {
    public record!: MJUserViewEntity;
            { sectionKey: 'userOwnership', sectionName: 'User & Ownership', isExpanded: true },
            { sectionKey: 'entityContext', sectionName: 'Entity Context', isExpanded: true },
            { sectionKey: 'viewDefinitionSettings', sectionName: 'View Definition & Settings', isExpanded: false },
            { sectionKey: 'filteringSmartSearch', sectionName: 'Filtering & Smart Search', isExpanded: false },
            { sectionKey: 'displaySettings', sectionName: 'Display Settings', isExpanded: false },
            { sectionKey: 'entityRelationships', sectionName: 'Entity Relationships', isExpanded: false },
            { sectionKey: 'userViewRuns', sectionName: 'User View Runs', isExpanded: false }
