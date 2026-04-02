import { MJTaskEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Tasks') // Tell MemberJunction about this class
    selector: 'gen-mjtask-form',
    templateUrl: './mjtask.form.component.html'
export class MJTaskFormComponent extends BaseFormComponent {
    public record!: MJTaskEntity;
            { sectionKey: 'relationshipsOwnership', sectionName: 'Relationships & Ownership', isExpanded: true },
            { sectionKey: 'taskDetails', sectionName: 'Task Details', isExpanded: true },
            { sectionKey: 'timelineMilestones', sectionName: 'Timeline & Milestones', isExpanded: false },
            { sectionKey: 'mJTaskDependencies', sectionName: 'MJ: Task Dependencies', isExpanded: false },
            { sectionKey: 'mJTaskDependencies1', sectionName: 'MJ: Task Dependencies', isExpanded: false },
