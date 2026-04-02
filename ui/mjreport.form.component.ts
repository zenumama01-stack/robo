import { MJReportEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Reports') // Tell MemberJunction about this class
    selector: 'gen-mjreport-form',
    templateUrl: './mjreport.form.component.html'
export class MJReportFormComponent extends BaseFormComponent {
    public record!: MJReportEntity;
            { sectionKey: 'reportDetails', sectionName: 'Report Details', isExpanded: true },
            { sectionKey: 'dataContextRelationships', sectionName: 'Data Context & Relationships', isExpanded: true },
            { sectionKey: 'outputScheduling', sectionName: 'Output & Scheduling', isExpanded: false },
            { sectionKey: 'reportSnapshots', sectionName: 'Report Snapshots', isExpanded: false },
            { sectionKey: 'mJReportVersions', sectionName: 'MJ: Report Versions', isExpanded: false },
            { sectionKey: 'mJReportUserStates', sectionName: 'MJ: Report User States', isExpanded: false }
