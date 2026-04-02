import { MJListEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Lists') // Tell MemberJunction about this class
    selector: 'gen-mjlist-form',
    templateUrl: './mjlist.form.component.html'
export class MJListFormComponent extends BaseFormComponent {
    public record!: MJListEntity;
            { sectionKey: 'listDefinition', sectionName: 'List Definition', isExpanded: true },
            { sectionKey: 'associations', sectionName: 'Associations', isExpanded: true },
            { sectionKey: 'mJListInvitations', sectionName: 'MJ: List Invitations', isExpanded: false },
            { sectionKey: 'mJListShares', sectionName: 'MJ: List Shares', isExpanded: false }
