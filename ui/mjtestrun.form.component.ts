import { MJTestRunEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Test Runs') // Tell MemberJunction about this class
    selector: 'gen-mjtestrun-form',
    templateUrl: './mjtestrun.form.component.html'
export class MJTestRunFormComponent extends BaseFormComponent {
    public record!: MJTestRunEntity;
            { sectionKey: 'testTargetInfo', sectionName: 'Test & Target Info', isExpanded: true },
            { sectionKey: 'inputExpectedOutput', sectionName: 'Input & Expected Output', isExpanded: false },
            { sectionKey: 'resultAnalysis', sectionName: 'Result Analysis', isExpanded: false },
            { sectionKey: 'mJTestRunFeedbacks', sectionName: 'MJ: Test Run Feedbacks', isExpanded: false },
