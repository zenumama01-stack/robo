@RegisterClass(BaseFormComponent, 'MJ: Test Rubrics') // Tell MemberJunction about this class
    selector: 'gen-mjtestrubric-form',
    templateUrl: './mjtestrubric.form.component.html'
export class MJTestRubricFormComponent extends BaseFormComponent {
    public record!: MJTestRubricEntity;
