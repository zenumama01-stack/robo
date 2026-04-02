 * Retrieves posts from an Instagram Business account.
 * Returns feed posts, carousels, reels, and IGTV videos with basic metrics.
@RegisterClass(BaseAction, 'Instagram - Get Business Posts')
export class InstagramGetBusinessPostsAction extends InstagramBaseAction {
            const limit = this.getParamValue(params.Params, 'Limit') || 25;
            const mediaType = this.getParamValue(params.Params, 'MediaType');
            const includeMetrics = this.getParamValue(params.Params, 'IncludeMetrics') !== false;
            const afterCursor = this.getParamValue(params.Params, 'AfterCursor');
            let fields = 'id,caption,media_type,media_url,thumbnail_url,permalink,timestamp';
            if (includeMetrics) {
                fields += ',like_count,comments_count,impressions,reach,saved,video_views';
                limit: Math.min(limit, 100) // Instagram max is 100 per request
            if (afterCursor) {
                queryParams.after = afterCursor;
            // Fetch posts
                    cursors: {
                    previous?: string;
            // Filter by media type if specified
            let posts = response.data || [];
            if (mediaType) {
                posts = posts.filter(post => post.media_type === mediaType);
            // Transform to common format
            const normalizedPosts: SocialPost[] = posts.map(post => this.normalizePost(post));
            // Get additional insights if requested and available
            if (includeMetrics && posts.length > 0) {
                await this.enrichPostsWithInsights(normalizedPosts);
                    posts: normalizedPosts,
                    paging: {
                        hasNext: !!response.paging?.next,
                        hasPrevious: !!response.paging?.previous,
                        afterCursor: response.paging?.cursors?.after,
                        beforeCursor: response.paging?.cursors?.before
                        mediaTypes: this.summarizeMediaTypes(posts),
                        dateRange: this.getDateRange(normalizedPosts)
                Message: `Retrieved ${normalizedPosts.length} Instagram posts`,
            LogError('Failed to retrieve Instagram business posts', error);
                    Message: 'Insufficient permissions to access Instagram business posts',
                Message: `Failed to retrieve Instagram posts: ${error.message}`,
     * Enrich posts with additional insights data
    private async enrichPostsWithInsights(posts: SocialPost[]): Promise<void> {
        // Instagram allows batch insights requests
        const postIds = posts.map(p => p.id);
        const batchSize = 50; // Instagram's batch limit
        for (let i = 0; i < postIds.length; i += batchSize) {
            const batch = postIds.slice(i, i + batchSize);
                // Get insights for this batch
                const metrics = ['impressions', 'reach', 'engagement', 'saved', 'video_views'];
                const insightsPromises = batch.map(postId => 
                    this.getInsights(postId, metrics, 'lifetime')
                const insightsResults = await Promise.allSettled(insightsPromises);
                // Map insights back to posts
                insightsResults.forEach((result, index) => {
                    if (result.status === 'fulfilled' && result.value) {
                        const postIndex = i + index;
                        const post = posts[postIndex];
                        result.value.forEach((metric: any) => {
                            const value = metric.values?.[0]?.value || 0;
                                    post.analytics!.impressions = value;
                                    post.analytics!.reach = value;
                                    post.analytics!.engagements = value;
                                case 'saved':
                                    post.analytics!.saves = value;
                                case 'video_views':
                                    post.analytics!.videoViews = value;
                // Log but don't fail the whole operation
                LogError(`Failed to get insights for batch starting at ${i}`, error);
     * Summarize media types in the response
    private summarizeMediaTypes(posts: any[]): Record<string, number> {
        const summary: Record<string, number> = {
            CAROUSEL_ALBUM: 0
            if (summary[post.media_type] !== undefined) {
                summary[post.media_type]++;
     * Get date range of posts
    private getDateRange(posts: SocialPost[]): { earliest: Date | null; latest: Date | null } {
            return { earliest: null, latest: null };
                Value: 25
                Name: 'AfterCursor',
        return 'Retrieves posts from an Instagram Business account with optional metrics and filtering.';
