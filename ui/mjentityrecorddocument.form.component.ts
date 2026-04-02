import { MJEntityRecordDocumentEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Entity Record Documents') // Tell MemberJunction about this class
    selector: 'gen-mjentityrecorddocument-form',
    templateUrl: './mjentityrecorddocument.form.component.html'
export class MJEntityRecordDocumentFormComponent extends BaseFormComponent {
    public record!: MJEntityRecordDocumentEntity;
            { sectionKey: 'documentDefinitionOutput', sectionName: 'Document Definition & Output', isExpanded: true },
            { sectionKey: 'vectorEmbedding', sectionName: 'Vector Embedding', isExpanded: false },
            { sectionKey: 'timestampsAudit', sectionName: 'Timestamps & Audit', isExpanded: false },
