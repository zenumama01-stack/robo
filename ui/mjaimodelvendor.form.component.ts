import { MJAIModelVendorEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Model Vendors') // Tell MemberJunction about this class
    selector: 'gen-mjaimodelvendor-form',
    templateUrl: './mjaimodelvendor.form.component.html'
export class MJAIModelVendorFormComponent extends BaseFormComponent {
    public record!: MJAIModelVendorEntity;
            { sectionKey: 'modelVendorLinkage', sectionName: 'Model-Vendor Linkage', isExpanded: true },
            { sectionKey: 'implementationConfiguration', sectionName: 'Implementation Configuration', isExpanded: true },
            { sectionKey: 'mJAICredentialBindings', sectionName: 'MJ: AI Credential Bindings', isExpanded: false }
