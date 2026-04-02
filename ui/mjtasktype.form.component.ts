import { MJTaskTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Task Types') // Tell MemberJunction about this class
    selector: 'gen-mjtasktype-form',
    templateUrl: './mjtasktype.form.component.html'
export class MJTaskTypeFormComponent extends BaseFormComponent {
    public record!: MJTaskTypeEntity;
            { sectionKey: 'taskTypeDetails', sectionName: 'Task Type Details', isExpanded: true },
