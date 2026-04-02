@RegisterClass(BaseFormComponent, 'MJ: Test Run Feedbacks') // Tell MemberJunction about this class
    selector: 'gen-mjtestrunfeedback-form',
    templateUrl: './mjtestrunfeedback.form.component.html'
export class MJTestRunFeedbackFormComponent extends BaseFormComponent {
    public record!: MJTestRunFeedbackEntity;
            { sectionKey: 'reviewContext', sectionName: 'Review Context', isExpanded: true },
            { sectionKey: 'feedbackContent', sectionName: 'Feedback Content', isExpanded: true },
