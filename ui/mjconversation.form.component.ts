import { MJConversationEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Conversations') // Tell MemberJunction about this class
    selector: 'gen-mjconversation-form',
    templateUrl: './mjconversation.form.component.html'
export class MJConversationFormComponent extends BaseFormComponent {
    public record!: MJConversationEntity;
            { sectionKey: 'conversationCore', sectionName: 'Conversation Core', isExpanded: true },
            { sectionKey: 'participantsReferences', sectionName: 'Participants & References', isExpanded: true },
            { sectionKey: 'contextualScope', sectionName: 'Contextual Scope', isExpanded: false },
            { sectionKey: 'testRunDetails', sectionName: 'Test Run Details', isExpanded: false },
            { sectionKey: 'reports', sectionName: 'Reports', isExpanded: false },
