import { MJEntityFieldEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Fields') // Tell MemberJunction about this class
    selector: 'gen-mjentityfield-form',
    templateUrl: './mjentityfield.form.component.html'
export class MJEntityFieldFormComponent extends BaseFormComponent {
    public record!: MJEntityFieldEntity;
            { sectionKey: 'identificationKeys', sectionName: 'Identification & Keys', isExpanded: true },
            { sectionKey: 'userInterfaceDisplaySettings', sectionName: 'User Interface & Display Settings', isExpanded: true },
            { sectionKey: 'dataConstraintsValidation', sectionName: 'Data Constraints & Validation', isExpanded: false },
            { sectionKey: 'relationshipsLinking', sectionName: 'Relationships & Linking', isExpanded: false },
            { sectionKey: 'systemAuditMetadata', sectionName: 'System & Audit Metadata', isExpanded: false },
            { sectionKey: 'securityEncryption', sectionName: 'Security & Encryption', isExpanded: false },
            { sectionKey: 'mJEntityFieldValues', sectionName: 'MJ: Entity Field Values', isExpanded: false }
