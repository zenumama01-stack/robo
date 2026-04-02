import { MJOAuthClientRegistrationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: O Auth Client Registrations') // Tell MemberJunction about this class
    selector: 'gen-mjoauthclientregistration-form',
    templateUrl: './mjoauthclientregistration.form.component.html'
export class MJOAuthClientRegistrationFormComponent extends BaseFormComponent {
    public record!: MJOAuthClientRegistrationEntity;
