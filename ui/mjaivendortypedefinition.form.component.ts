import { MJAIVendorTypeDefinitionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Vendor Type Definitions') // Tell MemberJunction about this class
    selector: 'gen-mjaivendortypedefinition-form',
    templateUrl: './mjaivendortypedefinition.form.component.html'
export class MJAIVendorTypeDefinitionFormComponent extends BaseFormComponent {
    public record!: MJAIVendorTypeDefinitionEntity;
            { sectionKey: 'vendorTypeInformation', sectionName: 'Vendor Type Information', isExpanded: true },
            { sectionKey: 'mJAIVendorTypes', sectionName: 'MJ: AI Vendor Types', isExpanded: false }
