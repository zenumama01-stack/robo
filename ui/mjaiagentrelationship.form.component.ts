import { MJAIAgentRelationshipEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Relationships') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentrelationship-form',
    templateUrl: './mjaiagentrelationship.form.component.html'
export class MJAIAgentRelationshipFormComponent extends BaseFormComponent {
    public record!: MJAIAgentRelationshipEntity;
            { sectionKey: 'agentRelationship', sectionName: 'Agent Relationship', isExpanded: true },
            { sectionKey: 'payloadMappingContext', sectionName: 'Payload Mapping & Context', isExpanded: true },
            { sectionKey: 'conversationSettings', sectionName: 'Conversation Settings', isExpanded: false },
