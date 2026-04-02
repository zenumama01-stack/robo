import { MJProjectEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Projects') // Tell MemberJunction about this class
    selector: 'gen-mjproject-form',
    templateUrl: './mjproject.form.component.html'
export class MJProjectFormComponent extends BaseFormComponent {
    public record!: MJProjectEntity;
            { sectionKey: 'systemStatus', sectionName: 'System & Status', isExpanded: true },
            { sectionKey: 'projectHierarchy', sectionName: 'Project Hierarchy', isExpanded: true },
            { sectionKey: 'projectOverview', sectionName: 'Project Overview', isExpanded: false },
