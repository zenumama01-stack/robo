import { MJAIVendorEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Vendors') // Tell MemberJunction about this class
    selector: 'gen-mjaivendor-form',
    templateUrl: './mjaivendor.form.component.html'
export class MJAIVendorFormComponent extends BaseFormComponent {
    public record!: MJAIVendorEntity;
            { sectionKey: 'vendorDetails', sectionName: 'Vendor Details', isExpanded: true },
            { sectionKey: 'details', sectionName: 'Details', isExpanded: true },
            { sectionKey: 'mJAICredentialBindings', sectionName: 'MJ: AI Credential Bindings', isExpanded: false },
            { sectionKey: 'mJAIVendorTypes', sectionName: 'MJ: AI Vendor Types', isExpanded: false },
            { sectionKey: 'mJAIAgentRuns', sectionName: 'MJ: AI Agent Runs', isExpanded: false }
