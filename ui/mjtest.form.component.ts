import { MJTestEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Tests') // Tell MemberJunction about this class
    selector: 'gen-mjtest-form',
    templateUrl: './mjtest.form.component.html'
export class MJTestFormComponent extends BaseFormComponent {
    public record!: MJTestEntity;
            { sectionKey: 'testDefinition', sectionName: 'Test Definition', isExpanded: true },
            { sectionKey: 'testLogic', sectionName: 'Test Logic', isExpanded: true },
            { sectionKey: 'executionSettings', sectionName: 'Execution Settings', isExpanded: false },
            { sectionKey: 'mJTestRuns', sectionName: 'MJ: Test Runs', isExpanded: false },
            { sectionKey: 'mJTestSuiteTests', sectionName: 'MJ: Test Suite Tests', isExpanded: false }
