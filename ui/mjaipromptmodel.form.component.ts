@RegisterClass(BaseFormComponent, 'MJ: AI Prompt Models') // Tell MemberJunction about this class
    selector: 'gen-mjaipromptmodel-form',
    templateUrl: './mjaipromptmodel.form.component.html'
export class MJAIPromptModelFormComponent extends BaseFormComponent {
    public record!: MJAIPromptModelEntity;
            { sectionKey: 'promptModelMapping', sectionName: 'Prompt & Model Mapping', isExpanded: true },
            { sectionKey: 'vendorConfiguration', sectionName: 'Vendor & Configuration', isExpanded: true },
            { sectionKey: 'executionParallelSettings', sectionName: 'Execution & Parallel Settings', isExpanded: false },
