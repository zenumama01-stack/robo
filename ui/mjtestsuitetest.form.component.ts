import { MJTestSuiteTestEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Test Suite Tests') // Tell MemberJunction about this class
    selector: 'gen-mjtestsuitetest-form',
    templateUrl: './mjtestsuitetest.form.component.html'
export class MJTestSuiteTestFormComponent extends BaseFormComponent {
    public record!: MJTestSuiteTestEntity;
