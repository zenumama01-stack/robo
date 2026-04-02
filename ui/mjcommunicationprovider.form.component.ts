import { MJCommunicationProviderEntity } from '@memberjunction/core-entities';
import { JoinGridComponent } from "@memberjunction/ng-join-grid"
@RegisterClass(BaseFormComponent, 'MJ: Communication Providers') // Tell MemberJunction about this class
    selector: 'gen-mjcommunicationprovider-form',
    templateUrl: './mjcommunicationprovider.form.component.html'
export class MJCommunicationProviderFormComponent extends BaseFormComponent {
    public record!: MJCommunicationProviderEntity;
            { sectionKey: 'providerDetails', sectionName: 'Provider Details', isExpanded: true },
            { sectionKey: 'advancedCapabilities', sectionName: 'Advanced Capabilities', isExpanded: false },
            { sectionKey: 'communicationLogs', sectionName: 'Communication Logs', isExpanded: false },
            { sectionKey: 'messageTypes', sectionName: 'Message Types', isExpanded: false }
