import { MJMCPServerToolEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: MCP Server Tools') // Tell MemberJunction about this class
    selector: 'gen-mjmcpservertool-form',
    templateUrl: './mjmcpservertool.form.component.html'
export class MJMCPServerToolFormComponent extends BaseFormComponent {
    public record!: MJMCPServerToolEntity;
            { sectionKey: 'toolOverview', sectionName: 'Tool Overview', isExpanded: true },
            { sectionKey: 'schemasAnnotations', sectionName: 'Schemas & Annotations', isExpanded: true },
            { sectionKey: 'automation', sectionName: 'Automation', isExpanded: false },
            { sectionKey: 'mJMCPToolExecutionLogs', sectionName: 'MJ: MCP Tool Execution Logs', isExpanded: false }
