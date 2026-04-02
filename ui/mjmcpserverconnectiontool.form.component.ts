import { MJMCPServerConnectionToolEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: MCP Server Connection Tools') // Tell MemberJunction about this class
    selector: 'gen-mjmcpserverconnectiontool-form',
    templateUrl: './mjmcpserverconnectiontool.form.component.html'
export class MJMCPServerConnectionToolFormComponent extends BaseFormComponent {
    public record!: MJMCPServerConnectionToolEntity;
            { sectionKey: 'connectionMapping', sectionName: 'Connection Mapping', isExpanded: true },
