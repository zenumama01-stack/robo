import { MJQueueEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Queues') // Tell MemberJunction about this class
    selector: 'gen-mjqueue-form',
    templateUrl: './mjqueue.form.component.html'
export class MJQueueFormComponent extends BaseFormComponent {
    public record!: MJQueueEntity;
            { sectionKey: 'queueDefinition', sectionName: 'Queue Definition', isExpanded: true },
            { sectionKey: 'processEnvironment', sectionName: 'Process Environment', isExpanded: false },
            { sectionKey: 'queueTasks', sectionName: 'Queue Tasks', isExpanded: false }
