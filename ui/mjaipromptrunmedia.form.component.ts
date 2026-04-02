import { MJAIPromptRunMediaEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Prompt Run Medias') // Tell MemberJunction about this class
    selector: 'gen-mjaipromptrunmedia-form',
    templateUrl: './mjaipromptrunmedia.form.component.html'
export class MJAIPromptRunMediaFormComponent extends BaseFormComponent {
    public record!: MJAIPromptRunMediaEntity;
            { sectionKey: 'association', sectionName: 'Association', isExpanded: true },
            { sectionKey: 'mediaMetadata', sectionName: 'Media Metadata', isExpanded: false },
            { sectionKey: 'contentData', sectionName: 'Content Data', isExpanded: false },
            { sectionKey: 'mJAIAgentRunMedias', sectionName: 'MJ: AI Agent Run Medias', isExpanded: false }
