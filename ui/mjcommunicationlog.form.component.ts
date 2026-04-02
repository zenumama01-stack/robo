import { MJCommunicationLogEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Communication Logs') // Tell MemberJunction about this class
    selector: 'gen-mjcommunicationlog-form',
    templateUrl: './mjcommunicationlog.form.component.html'
export class MJCommunicationLogFormComponent extends BaseFormComponent {
    public record!: MJCommunicationLogEntity;
            { sectionKey: 'messageIdentification', sectionName: 'Message Identification', isExpanded: true },
            { sectionKey: 'messageDetails', sectionName: 'Message Details', isExpanded: true },
