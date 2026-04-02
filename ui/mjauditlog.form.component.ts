@RegisterClass(BaseFormComponent, 'MJ: Audit Logs') // Tell MemberJunction about this class
    selector: 'gen-mjauditlog-form',
    templateUrl: './mjauditlog.form.component.html'
export class MJAuditLogFormComponent extends BaseFormComponent {
    public record!: MJAuditLogEntity;
            { sectionKey: 'auditEntry', sectionName: 'Audit Entry', isExpanded: true },
            { sectionKey: 'targetEntityReference', sectionName: 'Target Entity Reference', isExpanded: true },
