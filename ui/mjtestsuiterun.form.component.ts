import { MJTestSuiteRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Test Suite Runs') // Tell MemberJunction about this class
    selector: 'gen-mjtestsuiterun-form',
    templateUrl: './mjtestsuiterun.form.component.html'
export class MJTestSuiteRunFormComponent extends BaseFormComponent {
    public record!: MJTestSuiteRunEntity;
            { sectionKey: 'executionTimelineStatus', sectionName: 'Execution Timeline & Status', isExpanded: true },
            { sectionKey: 'testMetrics', sectionName: 'Test Metrics', isExpanded: false },
            { sectionKey: 'technicalOutput', sectionName: 'Technical Output', isExpanded: false },
            { sectionKey: 'executionHost', sectionName: 'Execution Host', isExpanded: false },
            { sectionKey: 'userDetails', sectionName: 'User Details', isExpanded: false },
