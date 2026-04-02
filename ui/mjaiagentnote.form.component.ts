@RegisterClass(BaseFormComponent, 'MJ: AI Agent Notes') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentnote-form',
    templateUrl: './mjaiagentnote.form.component.html'
export class MJAIAgentNoteFormComponent extends BaseFormComponent {
    public record!: MJAIAgentNoteEntity;
            { sectionKey: 'scopeReferences', sectionName: 'Scope & References', isExpanded: true },
            { sectionKey: 'noteDetails', sectionName: 'Note Details', isExpanded: true },
            { sectionKey: 'embeddingAIData', sectionName: 'Embedding & AI Data', isExpanded: false },
