import { MJOAuthTokenEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: O Auth Tokens') // Tell MemberJunction about this class
    selector: 'gen-mjoauthtoken-form',
    templateUrl: './mjoauthtoken.form.component.html'
export class MJOAuthTokenFormComponent extends BaseFormComponent {
    public record!: MJOAuthTokenEntity;
