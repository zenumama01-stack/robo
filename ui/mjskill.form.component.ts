import { MJSkillEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Skills') // Tell MemberJunction about this class
    selector: 'gen-mjskill-form',
    templateUrl: './mjskill.form.component.html'
export class MJSkillFormComponent extends BaseFormComponent {
    public record!: MJSkillEntity;
            { sectionKey: 'skillIdentification', sectionName: 'Skill Identification', isExpanded: true },
            { sectionKey: 'skillHierarchy', sectionName: 'Skill Hierarchy', isExpanded: true },
            { sectionKey: 'skills', sectionName: 'Skills', isExpanded: false }
