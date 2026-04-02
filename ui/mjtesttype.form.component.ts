import { MJTestTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Test Types') // Tell MemberJunction about this class
    selector: 'gen-mjtesttype-form',
    templateUrl: './mjtesttype.form.component.html'
export class MJTestTypeFormComponent extends BaseFormComponent {
    public record!: MJTestTypeEntity;
            { sectionKey: 'testTypeDefinition', sectionName: 'Test Type Definition', isExpanded: true },
            { sectionKey: 'mJTestRubrics', sectionName: 'MJ: Test Rubrics', isExpanded: false },
            { sectionKey: 'mJTests', sectionName: 'MJ: Tests', isExpanded: false }
