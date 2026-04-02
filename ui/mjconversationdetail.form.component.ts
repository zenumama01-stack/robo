import { MJConversationDetailEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Conversation Details') // Tell MemberJunction about this class
    selector: 'gen-mjconversationdetail-form',
    templateUrl: './mjconversationdetail.form.component.html'
export class MJConversationDetailFormComponent extends BaseFormComponent {
    public record!: MJConversationDetailEntity;
            { sectionKey: 'messageCore', sectionName: 'Message Core', isExpanded: true },
            { sectionKey: 'userFeedbackInsights', sectionName: 'User Feedback & Insights', isExpanded: true },
            { sectionKey: 'relatedEntities', sectionName: 'Related Entities', isExpanded: false },
            { sectionKey: 'interactiveElements', sectionName: 'Interactive Elements', isExpanded: false },
            { sectionKey: 'mJConversationDetailArtifacts', sectionName: 'MJ: Conversation Detail Artifacts', isExpanded: false },
            { sectionKey: 'mJConversationDetailRatings', sectionName: 'MJ: Conversation Detail Ratings', isExpanded: false },
            { sectionKey: 'mJTasks', sectionName: 'MJ: Tasks', isExpanded: false }
