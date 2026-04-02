import { MJTestSuiteEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Test Suites') // Tell MemberJunction about this class
    selector: 'gen-mjtestsuite-form',
    templateUrl: './mjtestsuite.form.component.html'
export class MJTestSuiteFormComponent extends BaseFormComponent {
    public record!: MJTestSuiteEntity;
            { sectionKey: 'suiteIdentification', sectionName: 'Suite Identification', isExpanded: true },
            { sectionKey: 'executionConfiguration', sectionName: 'Execution Configuration', isExpanded: false },
            { sectionKey: 'mJTestSuites', sectionName: 'MJ: Test Suites', isExpanded: false },
            { sectionKey: 'mJTestSuiteRuns', sectionName: 'MJ: Test Suite Runs', isExpanded: false },
