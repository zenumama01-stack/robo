import { RatingJSON } from '../../models/conversation-complete-query.model';
 * Component for displaying and managing multi-user ratings on conversation messages.
 * Shows aggregate ratings and allows users to provide their own rating.
    selector: 'mj-conversation-message-rating',
        <div class="rating-container">
          @if (totalRatings > 0) {
            <div class="aggregate-rating" [title]="getRatingsTooltip()">
              <span class="thumbs-up" [class.has-votes]="thumbsUpCount > 0">
                👍 {{ thumbsUpCount }}
              <span class="thumbs-down" [class.has-votes]="thumbsDownCount > 0">
                👎 {{ thumbsDownCount }}
              <span class="total-count">({{ totalRatings }} {{ totalRatings === 1 ? 'rating' : 'ratings' }})</span>
          <div class="user-rating" [class.has-rated]="currentUserRating != null">
              class="rating-button thumbs-up-btn"
              [class.active]="currentUserRating != null && currentUserRating >= 8"
              (click)="RateThumbsUp()"
              title="This was helpful"
              type="button">
              👍
              class="rating-button thumbs-down-btn"
              [class.active]="currentUserRating != null && currentUserRating <= 3"
              (click)="RateThumbsDown()"
              title="This was not helpful"
              👎
        .rating-container {
        .aggregate-rating {
        .thumbs-up, .thumbs-down {
        .thumbs-up.has-votes, .thumbs-down.has-votes {
        .user-rating {
            border: 1px solid #9CA3AF;
        .rating-button:hover {
            border-color: #6B7280;
        .rating-button.active {
        .thumbs-up-btn {
            color: #16A34A;
        .thumbs-up-btn:hover {
            background: #F0FDF4;
        .thumbs-up-btn.active {
            border-color: #16A34A;
            background: #DCFCE7;
        .thumbs-down-btn {
        .thumbs-down-btn:hover {
        .thumbs-down-btn.active {
export class ConversationMessageRatingComponent implements OnInit {
    @Input() conversationDetailId!: string;
    @Input() ratingsData?: RatingJSON[]; // Pre-loaded ratings from parent (RatingsJSON from query)
    thumbsUpCount = 0;
    thumbsDownCount = 0;
    totalRatings = 0;
    currentUserRating: number | null = null;
    allRatings: RatingJSON[] = [];
    private get currentUserId(): string {
        return this.currentUser?.ID || '';
        if (this.ratingsData) {
            // Use pre-loaded ratings (no database query needed)
            this.ProcessRatings(this.ratingsData);
            // Fallback to loading ratings if not provided
            await this.LoadRatings();
     * Process ratings data (from query or API)
    private ProcessRatings(ratings: RatingJSON[] | MJConversationDetailRatingEntity[]): void {
        this.allRatings = ratings as RatingJSON[];
        this.thumbsUpCount = ratings.filter(r => r.Rating ? r.Rating >= 8 : false).length;
        this.thumbsDownCount = ratings.filter(r => r.Rating ? r.Rating <= 3 : false).length;
        this.totalRatings = ratings.length;
        const currentUserRating = ratings.find(r => r.UserID === this.currentUserId);
        this.currentUserRating = currentUserRating?.Rating ?? null;
     * Get tooltip showing who rated this message
    getRatingsTooltip(): string {
        if (this.allRatings.length === 0) return '';
        const thumbsUpUsers = this.allRatings
            .filter(r => r.Rating ? r.Rating >= 8 : false)
            .map(r => (r as RatingJSON).UserName || 'Unknown')
        const thumbsDownUsers = this.allRatings
            .filter(r => r.Rating ? r.Rating <= 3 : false)
        if (thumbsUpUsers) parts.push(`👍 ${thumbsUpUsers}`);
        if (thumbsDownUsers) parts.push(`👎 ${thumbsDownUsers}`);
     * Load all ratings for this message (fallback if not pre-loaded)
    async LoadRatings(): Promise<void> {
            const result = await rv.RunView<MJConversationDetailRatingEntity>({
                EntityName: 'MJ: Conversation Detail Ratings',
                ExtraFilter: `ConversationDetailID='${this.conversationDetailId}'`,
            this.ProcessRatings(result.Results);
            console.error('Failed to load ratings:', error);
     * Rate message as thumbs up (10/10)
    async RateThumbsUp(): Promise<void> {
        await this.SaveRating(10);
     * Rate message as thumbs down (1/10)
    async RateThumbsDown(): Promise<void> {
        await this.SaveRating(1);
     * Save or update user's rating for this message
    private async SaveRating(rating: number): Promise<void> {
            let ratingEntity: MJConversationDetailRatingEntity;
            // Try to load existing rating
            const existing = await rv.RunView<MJConversationDetailRatingEntity>({
                ExtraFilter: `ConversationDetailID='${this.conversationDetailId}' AND UserID='${this.currentUserId}'`,
                ratingEntity = existing.Results[0];
                // If clicking same rating, remove it (toggle off)
                if (ratingEntity.Rating === rating) {
                    await ratingEntity.Delete();
                ratingEntity.Rating = rating;
                ratingEntity = await md.GetEntityObject<MJConversationDetailRatingEntity>('MJ: Conversation Detail Ratings');
                ratingEntity.ConversationDetailID = this.conversationDetailId;
                ratingEntity.UserID = this.currentUserId;
            await ratingEntity.Save();
            console.error('Failed to save rating:', error);
