@RegisterClass(BaseFormComponent, 'MJ: MCP Tool Execution Logs') // Tell MemberJunction about this class
    selector: 'gen-mjmcptoolexecutionlog-form',
    templateUrl: './mjmcptoolexecutionlog.form.component.html'
export class MJMCPToolExecutionLogFormComponent extends BaseFormComponent {
    public record!: MJMCPToolExecutionLogEntity;
            { sectionKey: 'connectionContext', sectionName: 'Connection Context', isExpanded: true },
            { sectionKey: 'userContext', sectionName: 'User Context', isExpanded: false },
            { sectionKey: 'payloadErrors', sectionName: 'Payload & Errors', isExpanded: false },
