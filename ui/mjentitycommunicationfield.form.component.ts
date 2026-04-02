import { MJEntityCommunicationFieldEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Communication Fields') // Tell MemberJunction about this class
    selector: 'gen-mjentitycommunicationfield-form',
    templateUrl: './mjentitycommunicationfield.form.component.html'
export class MJEntityCommunicationFieldFormComponent extends BaseFormComponent {
    public record!: MJEntityCommunicationFieldEntity;
            { sectionKey: 'identification', sectionName: 'Identification', isExpanded: true },
            { sectionKey: 'mappingConfiguration', sectionName: 'Mapping Configuration', isExpanded: true },
