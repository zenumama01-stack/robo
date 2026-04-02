import { MJCredentialCategoryEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Credential Categories') // Tell MemberJunction about this class
    selector: 'gen-mjcredentialcategory-form',
    templateUrl: './mjcredentialcategory.form.component.html'
export class MJCredentialCategoryFormComponent extends BaseFormComponent {
    public record!: MJCredentialCategoryEntity;
            { sectionKey: 'mJCredentialCategories', sectionName: 'MJ: Credential Categories', isExpanded: false },
            { sectionKey: 'mJCredentials', sectionName: 'MJ: Credentials', isExpanded: false }
