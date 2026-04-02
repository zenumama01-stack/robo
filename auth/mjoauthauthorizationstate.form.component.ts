import { MJOAuthAuthorizationStateEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: O Auth Authorization States') // Tell MemberJunction about this class
    selector: 'gen-mjoauthauthorizationstate-form',
    templateUrl: './mjoauthauthorizationstate.form.component.html'
export class MJOAuthAuthorizationStateFormComponent extends BaseFormComponent {
    public record!: MJOAuthAuthorizationStateEntity;
