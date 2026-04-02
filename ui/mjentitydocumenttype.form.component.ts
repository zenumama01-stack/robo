import { MJEntityDocumentTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Document Types') // Tell MemberJunction about this class
    selector: 'gen-mjentitydocumenttype-form',
    templateUrl: './mjentitydocumenttype.form.component.html'
export class MJEntityDocumentTypeFormComponent extends BaseFormComponent {
    public record!: MJEntityDocumentTypeEntity;
            { sectionKey: 'recordIdentifier', sectionName: 'Record Identifier', isExpanded: true },
            { sectionKey: 'documentTypeInformation', sectionName: 'Document Type Information', isExpanded: true },
            { sectionKey: 'entityDocuments', sectionName: 'Entity Documents', isExpanded: false }
