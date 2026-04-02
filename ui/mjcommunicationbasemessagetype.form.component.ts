import { MJCommunicationBaseMessageTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Communication Base Message Types') // Tell MemberJunction about this class
    selector: 'gen-mjcommunicationbasemessagetype-form',
    templateUrl: './mjcommunicationbasemessagetype.form.component.html'
export class MJCommunicationBaseMessageTypeFormComponent extends BaseFormComponent {
    public record!: MJCommunicationBaseMessageTypeEntity;
            { sectionKey: 'messageTypeDetails', sectionName: 'Message Type Details', isExpanded: true },
            { sectionKey: 'supportedFeatures', sectionName: 'Supported Features', isExpanded: true },
            { sectionKey: 'communicationProviderMessageTypes', sectionName: 'Communication Provider Message Types', isExpanded: false },
            { sectionKey: 'entityCommunicationMessageTypes', sectionName: 'Entity Communication Message Types', isExpanded: false }
