import { MJQueueTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Queue Types') // Tell MemberJunction about this class
    selector: 'gen-mjqueuetype-form',
    templateUrl: './mjqueuetype.form.component.html'
export class MJQueueTypeFormComponent extends BaseFormComponent {
    public record!: MJQueueTypeEntity;
            { sectionKey: 'processingDriverSettings', sectionName: 'Processing Driver Settings', isExpanded: true },
            { sectionKey: 'queues', sectionName: 'Queues', isExpanded: false }
