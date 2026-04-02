import { MJRecordMergeLogEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Record Merge Logs') // Tell MemberJunction about this class
    selector: 'gen-mjrecordmergelog-form',
    templateUrl: './mjrecordmergelog.form.component.html'
export class MJRecordMergeLogFormComponent extends BaseFormComponent {
    public record!: MJRecordMergeLogEntity;
            { sectionKey: 'mergeIdentification', sectionName: 'Merge Identification', isExpanded: true },
            { sectionKey: 'userActionsApprovals', sectionName: 'User Actions & Approvals', isExpanded: true },
            { sectionKey: 'duplicateRunDetailMatches', sectionName: 'Duplicate Run Detail Matches', isExpanded: false },
            { sectionKey: 'recordMergeDeletionLogs', sectionName: 'Record Merge Deletion Logs', isExpanded: false }
