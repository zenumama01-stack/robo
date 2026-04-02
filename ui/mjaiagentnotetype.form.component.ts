import { MJAIAgentNoteTypeEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: AI Agent Note Types') // Tell MemberJunction about this class
    selector: 'gen-mjaiagentnotetype-form',
    templateUrl: './mjaiagentnotetype.form.component.html'
export class MJAIAgentNoteTypeFormComponent extends BaseFormComponent {
    public record!: MJAIAgentNoteTypeEntity;
            { sectionKey: 'identifier', sectionName: 'Identifier', isExpanded: true },
            { sectionKey: 'noteTypeDefinition', sectionName: 'Note Type Definition', isExpanded: true },
            { sectionKey: 'aIAgentNotes', sectionName: 'AIAgent Notes', isExpanded: false }
