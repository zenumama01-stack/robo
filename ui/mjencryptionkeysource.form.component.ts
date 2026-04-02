import { MJEncryptionKeySourceEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Encryption Key Sources') // Tell MemberJunction about this class
    selector: 'gen-mjencryptionkeysource-form',
    templateUrl: './mjencryptionkeysource.form.component.html'
export class MJEncryptionKeySourceFormComponent extends BaseFormComponent {
    public record!: MJEncryptionKeySourceEntity;
