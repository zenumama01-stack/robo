import { MJUserRoleEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: User Roles') // Tell MemberJunction about this class
    selector: 'gen-mjuserrole-form',
    templateUrl: './mjuserrole.form.component.html'
export class MJUserRoleFormComponent extends BaseFormComponent {
    public record!: MJUserRoleEntity;
