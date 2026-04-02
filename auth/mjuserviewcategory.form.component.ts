import { MJUserViewCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User View Categories') // Tell MemberJunction about this class
    selector: 'gen-mjuserviewcategory-form',
    templateUrl: './mjuserviewcategory.form.component.html'
export class MJUserViewCategoryFormComponent extends BaseFormComponent {
    public record!: MJUserViewCategoryEntity;
            { sectionKey: 'organizationalHierarchy', sectionName: 'Organizational Hierarchy', isExpanded: true },
            { sectionKey: 'linkedEntities', sectionName: 'Linked Entities', isExpanded: false },
            { sectionKey: 'userViews', sectionName: 'User Views', isExpanded: false }
