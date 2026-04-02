import { MJReportVersionEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Report Versions') // Tell MemberJunction about this class
    selector: 'gen-mjreportversion-form',
    templateUrl: './mjreportversion.form.component.html'
export class MJReportVersionFormComponent extends BaseFormComponent {
    public record!: MJReportVersionEntity;
            { sectionKey: 'reportLinkage', sectionName: 'Report Linkage', isExpanded: true },
            { sectionKey: 'versionDetails', sectionName: 'Version Details', isExpanded: true },
