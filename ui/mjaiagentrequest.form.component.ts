import { MJAIAgentRequestEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Requests') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentrequest-form',
    templateUrl: './mjaiagentrequest.form.component.html'
export class MJAIAgentRequestFormComponent extends BaseFormComponent {
    public record!: MJAIAgentRequestEntity;
            { sectionKey: 'requestSummary', sectionName: 'Request Summary', isExpanded: true },
            { sectionKey: 'responseSummary', sectionName: 'Response Summary', isExpanded: true },
