import { MJAIAgentRunMediaEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Run Medias') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentrunmedia-form',
    templateUrl: './mjaiagentrunmedia.form.component.html'
export class MJAIAgentRunMediaFormComponent extends BaseFormComponent {
    public record!: MJAIAgentRunMediaEntity;
            { sectionKey: 'runContext', sectionName: 'Run Context', isExpanded: true },
            { sectionKey: 'fileAttributes', sectionName: 'File Attributes', isExpanded: true },
            { sectionKey: 'mediaContent', sectionName: 'Media Content', isExpanded: false },
