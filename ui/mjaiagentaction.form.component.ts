import { MJAIAgentActionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Actions') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentaction-form',
    templateUrl: './mjaiagentaction.form.component.html'
export class MJAIAgentActionFormComponent extends BaseFormComponent {
    public record!: MJAIAgentActionEntity;
            { sectionKey: 'mappingRelationships', sectionName: 'Mapping & Relationships', isExpanded: true },
            { sectionKey: 'auditStatus', sectionName: 'Audit & Status', isExpanded: true },
            { sectionKey: 'executionConstraints', sectionName: 'Execution Constraints', isExpanded: false },
            { sectionKey: 'resultCompaction', sectionName: 'Result Compaction', isExpanded: false },
            { sectionKey: 'systemMetadata', sectionName: 'System Metadata', isExpanded: false }
