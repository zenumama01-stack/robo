import { MJEncryptionKeyEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Encryption Keys') // Tell MemberJunction about this class
    selector: 'gen-mjencryptionkey-form',
    templateUrl: './mjencryptionkey.form.component.html'
export class MJEncryptionKeyFormComponent extends BaseFormComponent {
    public record!: MJEncryptionKeyEntity;
            { sectionKey: 'entityFields', sectionName: 'Entity Fields', isExpanded: false }
