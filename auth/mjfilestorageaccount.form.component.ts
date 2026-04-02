import { MJFileStorageAccountEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: File Storage Accounts') // Tell MemberJunction about this class
    selector: 'gen-mjfilestorageaccount-form',
    templateUrl: './mjfilestorageaccount.form.component.html'
export class MJFileStorageAccountFormComponent extends BaseFormComponent {
    public record!: MJFileStorageAccountEntity;
            { sectionKey: 'accountOverview', sectionName: 'Account Overview', isExpanded: true },
            { sectionKey: 'connectionDetails', sectionName: 'Connection Details', isExpanded: true },
