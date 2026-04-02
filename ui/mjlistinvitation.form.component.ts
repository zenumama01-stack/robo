import { MJListInvitationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: List Invitations') // Tell MemberJunction about this class
    selector: 'gen-mjlistinvitation-form',
    templateUrl: './mjlistinvitation.form.component.html'
export class MJListInvitationFormComponent extends BaseFormComponent {
    public record!: MJListInvitationEntity;
            { sectionKey: 'invitationDetails', sectionName: 'Invitation Details', isExpanded: true },
