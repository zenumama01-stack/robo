import { MJAIModalityEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Modalities') // Tell MemberJunction about this class
    selector: 'gen-mjaimodality-form',
    templateUrl: './mjaimodality.form.component.html'
export class MJAIModalityFormComponent extends BaseFormComponent {
    public record!: MJAIModalityEntity;
            { sectionKey: 'modalityDefinition', sectionName: 'Modality Definition', isExpanded: true },
            { sectionKey: 'technicalConstraints', sectionName: 'Technical Constraints', isExpanded: true },
            { sectionKey: 'mJAIModelModalities', sectionName: 'MJ: AI Model Modalities', isExpanded: false },
            { sectionKey: 'aIModelTypes', sectionName: 'AI Model Types', isExpanded: false },
            { sectionKey: 'mJAIPromptRunMedias', sectionName: 'MJ: AI Prompt Run Medias', isExpanded: false },
            { sectionKey: 'mJConversationDetailAttachments', sectionName: 'MJ: Conversation Detail Attachments', isExpanded: false },
            { sectionKey: 'aIModelTypes1', sectionName: 'AI Model Types', isExpanded: false }
