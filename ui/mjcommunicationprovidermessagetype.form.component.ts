import { MJCommunicationProviderMessageTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Communication Provider Message Types') // Tell MemberJunction about this class
    selector: 'gen-mjcommunicationprovidermessagetype-form',
    templateUrl: './mjcommunicationprovidermessagetype.form.component.html'
export class MJCommunicationProviderMessageTypeFormComponent extends BaseFormComponent {
    public record!: MJCommunicationProviderMessageTypeEntity;
            { sectionKey: 'providerMapping', sectionName: 'Provider Mapping', isExpanded: true },
            { sectionKey: 'messageTypeDefinition', sectionName: 'Message Type Definition', isExpanded: true },
            { sectionKey: 'communicationLogs', sectionName: 'Communication Logs', isExpanded: false }
