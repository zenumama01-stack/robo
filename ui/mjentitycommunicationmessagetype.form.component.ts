import { MJEntityCommunicationMessageTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Communication Message Types') // Tell MemberJunction about this class
    selector: 'gen-mjentitycommunicationmessagetype-form',
    templateUrl: './mjentitycommunicationmessagetype.form.component.html'
export class MJEntityCommunicationMessageTypeFormComponent extends BaseFormComponent {
    public record!: MJEntityCommunicationMessageTypeEntity;
            { sectionKey: 'mappingKeys', sectionName: 'Mapping Keys', isExpanded: true },
            { sectionKey: 'messageAttributes', sectionName: 'Message Attributes', isExpanded: true },
            { sectionKey: 'entityCommunicationFields', sectionName: 'Entity Communication Fields', isExpanded: false }
