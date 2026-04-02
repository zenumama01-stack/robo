import { MJReportSnapshotEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Report Snapshots') // Tell MemberJunction about this class
    selector: 'gen-mjreportsnapshot-form',
    templateUrl: './mjreportsnapshot.form.component.html'
export class MJReportSnapshotFormComponent extends BaseFormComponent {
    public record!: MJReportSnapshotEntity;
            { sectionKey: 'snapshotIdentification', sectionName: 'Snapshot Identification', isExpanded: true },
            { sectionKey: 'snapshotDescriptors', sectionName: 'Snapshot Descriptors', isExpanded: true },
