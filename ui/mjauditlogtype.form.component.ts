import { MJAuditLogTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Audit Log Types') // Tell MemberJunction about this class
    selector: 'gen-mjauditlogtype-form',
    templateUrl: './mjauditlogtype.form.component.html'
export class MJAuditLogTypeFormComponent extends BaseFormComponent {
    public record!: MJAuditLogTypeEntity;
            { sectionKey: 'logTypeDefinition', sectionName: 'Log Type Definition', isExpanded: true },
            { sectionKey: 'auditLogTypes', sectionName: 'Audit Log Types', isExpanded: false },
            { sectionKey: 'auditLogs', sectionName: 'Audit Logs', isExpanded: false }
