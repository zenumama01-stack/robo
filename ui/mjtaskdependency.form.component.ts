import { MJTaskDependencyEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Task Dependencies') // Tell MemberJunction about this class
    selector: 'gen-mjtaskdependency-form',
    templateUrl: './mjtaskdependency.form.component.html'
export class MJTaskDependencyFormComponent extends BaseFormComponent {
    public record!: MJTaskDependencyEntity;
            { sectionKey: 'taskReference', sectionName: 'Task Reference', isExpanded: true },
            { sectionKey: 'dependencyLink', sectionName: 'Dependency Link', isExpanded: true },
