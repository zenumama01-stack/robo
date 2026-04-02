import { MJAIAgentLearningCycleEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Learning Cycles') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentlearningcycle-form',
    templateUrl: './mjaiagentlearningcycle.form.component.html'
export class MJAIAgentLearningCycleFormComponent extends BaseFormComponent {
    public record!: MJAIAgentLearningCycleEntity;
            { sectionKey: 'agentReference', sectionName: 'Agent Reference', isExpanded: true },
            { sectionKey: 'cycleTimingStatus', sectionName: 'Cycle Timing & Status', isExpanded: true },
            { sectionKey: 'learningDetailsAudit', sectionName: 'Learning Details & Audit', isExpanded: false },
