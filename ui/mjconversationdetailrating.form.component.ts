import { MJConversationDetailRatingEntity } from '@memberjunction/core-entities';
@RegisterClass(BaseFormComponent, 'MJ: Conversation Detail Ratings') // Tell MemberJunction about this class
    selector: 'gen-mjconversationdetailrating-form',
    templateUrl: './mjconversationdetailrating.form.component.html'
export class MJConversationDetailRatingFormComponent extends BaseFormComponent {
    public record!: MJConversationDetailRatingEntity;
            { sectionKey: 'ratingInformation', sectionName: 'Rating Information', isExpanded: true },
