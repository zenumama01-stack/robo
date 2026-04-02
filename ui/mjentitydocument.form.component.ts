import { MJEntityDocumentEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Documents') // Tell MemberJunction about this class
    selector: 'gen-mjentitydocument-form',
    templateUrl: './mjentitydocument.form.component.html'
export class MJEntityDocumentFormComponent extends BaseFormComponent {
    public record!: MJEntityDocumentEntity;
            { sectionKey: 'documentDetails', sectionName: 'Document Details', isExpanded: true },
            { sectionKey: 'relationships', sectionName: 'Relationships', isExpanded: true },
            { sectionKey: 'matchingConfiguration', sectionName: 'Matching Configuration', isExpanded: false },
            { sectionKey: 'entityDocumentRuns', sectionName: 'Entity Document Runs', isExpanded: false },
            { sectionKey: 'entityDocumentSettings', sectionName: 'Entity Document Settings', isExpanded: false },
            { sectionKey: 'entityRecordDocuments', sectionName: 'Entity Record Documents', isExpanded: false }
