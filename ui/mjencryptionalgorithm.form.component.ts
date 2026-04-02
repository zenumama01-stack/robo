import { MJEncryptionAlgorithmEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Encryption Algorithms') // Tell MemberJunction about this class
    selector: 'gen-mjencryptionalgorithm-form',
    templateUrl: './mjencryptionalgorithm.form.component.html'
export class MJEncryptionAlgorithmFormComponent extends BaseFormComponent {
    public record!: MJEncryptionAlgorithmEntity;
            { sectionKey: 'mJEncryptionKeys', sectionName: 'MJ: Encryption Keys', isExpanded: false }
