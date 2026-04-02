@RegisterClass(BaseFormComponent, 'MJ: AI Prompt Runs') // Tell MemberJunction about this class
    selector: 'gen-mjaipromptrun-form',
    templateUrl: './mjaipromptrun.form.component.html'
export class MJAIPromptRunFormComponent extends BaseFormComponent {
    public record!: MJAIPromptRunEntity;
            { sectionKey: 'runExecutionCore', sectionName: 'Run Execution Core', isExpanded: true },
            { sectionKey: 'promptResultContent', sectionName: 'Prompt & Result Content', isExpanded: true },
            { sectionKey: 'performanceCostMetrics', sectionName: 'Performance & Cost Metrics', isExpanded: false },
            { sectionKey: 'modelParametersSettings', sectionName: 'Model Parameters & Settings', isExpanded: false },
            { sectionKey: 'validationRetryDetails', sectionName: 'Validation & Retry Details', isExpanded: false },
            { sectionKey: 'aIResultCache', sectionName: 'AI Result Cache', isExpanded: false }
