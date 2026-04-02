import { MJAIVendorTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Vendor Types') // Tell MemberJunction about this class
    selector: 'gen-mjaivendortype-form',
    templateUrl: './mjaivendortype.form.component.html'
export class MJAIVendorTypeFormComponent extends BaseFormComponent {
    public record!: MJAIVendorTypeEntity;
            { sectionKey: 'vendorIdentification', sectionName: 'Vendor Identification', isExpanded: true },
            { sectionKey: 'typeSpecification', sectionName: 'Type Specification', isExpanded: true },
            { sectionKey: 'statusAudit', sectionName: 'Status & Audit', isExpanded: false },
