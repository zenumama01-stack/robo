@RegisterClass(BaseFormComponent, 'MJ: AI Agent Types') // Tell MemberJunction about this class
    selector: 'gen-mjaiagenttype-form',
    templateUrl: './mjaiagenttype.form.component.html'
export class MJAIAgentTypeFormComponent extends BaseFormComponent {
    public record!: MJAIAgentTypeEntity;
            { sectionKey: 'basicDefinition', sectionName: 'Basic Definition', isExpanded: true },
            { sectionKey: 'promptConfiguration', sectionName: 'Prompt Configuration', isExpanded: true },
            { sectionKey: 'behaviorUISettings', sectionName: 'Behavior & UI Settings', isExpanded: false },
