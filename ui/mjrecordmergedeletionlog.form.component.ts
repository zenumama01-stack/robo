import { MJRecordMergeDeletionLogEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Record Merge Deletion Logs') // Tell MemberJunction about this class
    selector: 'gen-mjrecordmergedeletionlog-form',
    templateUrl: './mjrecordmergedeletionlog.form.component.html'
export class MJRecordMergeDeletionLogFormComponent extends BaseFormComponent {
    public record!: MJRecordMergeDeletionLogEntity;
            { sectionKey: 'deletionAudit', sectionName: 'Deletion Audit', isExpanded: true },
