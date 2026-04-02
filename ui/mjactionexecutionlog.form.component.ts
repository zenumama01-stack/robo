import { MJActionExecutionLogEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Action Execution Logs') // Tell MemberJunction about this class
    selector: 'gen-mjactionexecutionlog-form',
    templateUrl: './mjactionexecutionlog.form.component.html'
export class MJActionExecutionLogFormComponent extends BaseFormComponent {
            { sectionKey: 'retentionAudit', sectionName: 'Retention & Audit', isExpanded: true },
            { sectionKey: 'associatedEntities', sectionName: 'Associated Entities', isExpanded: true },
            { sectionKey: 'executionDetails', sectionName: 'Execution Details', isExpanded: false },
