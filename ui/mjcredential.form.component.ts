import { MJCredentialEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Credentials') // Tell MemberJunction about this class
    selector: 'gen-mjcredential-form',
    templateUrl: './mjcredential.form.component.html'
export class MJCredentialFormComponent extends BaseFormComponent {
    public record!: MJCredentialEntity;
            { sectionKey: 'classification', sectionName: 'Classification', isExpanded: true },
            { sectionKey: 'accessDetails', sectionName: 'Access Details', isExpanded: false },
            { sectionKey: 'mJOAuthTokens', sectionName: 'MJ: O Auth Tokens', isExpanded: false },
            { sectionKey: 'mJFileStorageAccounts', sectionName: 'MJ: File Storage Accounts', isExpanded: false },
