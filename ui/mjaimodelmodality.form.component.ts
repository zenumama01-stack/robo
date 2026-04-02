import { MJAIModelModalityEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Model Modalities') // Tell MemberJunction about this class
    selector: 'gen-mjaimodelmodality-form',
    templateUrl: './mjaimodelmodality.form.component.html'
export class MJAIModelModalityFormComponent extends BaseFormComponent {
    public record!: MJAIModelModalityEntity;
            { sectionKey: 'modelModalityLink', sectionName: 'Model & Modality Link', isExpanded: true },
            { sectionKey: 'capabilitySettings', sectionName: 'Capability Settings', isExpanded: true },
            { sectionKey: 'technicalConstraints', sectionName: 'Technical Constraints', isExpanded: false },
