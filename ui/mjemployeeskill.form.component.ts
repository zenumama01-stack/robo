import { MJEmployeeSkillEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Employee Skills') // Tell MemberJunction about this class
    selector: 'gen-mjemployeeskill-form',
    templateUrl: './mjemployeeskill.form.component.html'
export class MJEmployeeSkillFormComponent extends BaseFormComponent {
    public record!: MJEmployeeSkillEntity;
            { sectionKey: 'skillAssignment', sectionName: 'Skill Assignment', isExpanded: true },
