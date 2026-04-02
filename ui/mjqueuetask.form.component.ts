import { MJQueueTaskEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Queue Tasks') // Tell MemberJunction about this class
    selector: 'gen-mjqueuetask-form',
    templateUrl: './mjqueuetask.form.component.html'
export class MJQueueTaskFormComponent extends BaseFormComponent {
    public record!: MJQueueTaskEntity;
            { sectionKey: 'taskIdentityQueue', sectionName: 'Task Identity & Queue', isExpanded: true },
            { sectionKey: 'executionStatusTimeline', sectionName: 'Execution Status & Timeline', isExpanded: true },
            { sectionKey: 'payloadOutcome', sectionName: 'Payload & Outcome', isExpanded: false },
