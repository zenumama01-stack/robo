 * Action to retrieve scheduled posts from HootSuite
@RegisterClass(BaseAction, 'HootSuiteGetScheduledPostsAction')
export class HootSuiteGetScheduledPostsAction extends HootSuiteBaseAction {
     * Get scheduled posts from HootSuite
            const includeAnalytics = this.getParamValue(Params, 'IncludeAnalytics') || false;
                state: 'SCHEDULED',
                limit: Math.min(limit, 100),
                maxResults: limit
            if (profileId) {
                queryParams.socialProfileIds = profileId;
                queryParams.scheduledAfter = this.formatHootSuiteDate(startDate);
                queryParams.scheduledBefore = this.formatHootSuiteDate(endDate);
            // Get scheduled posts
            const posts = await this.makePaginatedRequest<HootSuitePost>('/messages', queryParams);
            // Convert to common format
            const normalizedPosts: SocialPost[] = await Promise.all(
                posts.map(async (post) => {
                    const normalized = this.normalizePost(post);
                    // Optionally include analytics
                    if (includeAnalytics && post.state === 'PUBLISHED') {
                            const analytics = await this.getPostAnalytics(post.id);
                            normalized.analytics = this.normalizeAnalytics(analytics);
                            LogStatus(`Failed to get analytics for post ${post.id}: ${error instanceof Error ? error.message : 'Unknown error'}`);
                byProfile: this.groupByProfile(posts),
                byDate: this.groupByDate(normalizedPosts),
            const postsParam = outputParams.find(p => p.Name === 'ScheduledPosts');
            if (postsParam) postsParam.Value = normalizedPosts;
                Message: `Retrieved ${normalizedPosts.length} scheduled posts`,
                Message: `Failed to get scheduled posts: ${errorMessage}`,
    private async getPostAnalytics(postId: string): Promise<any> {
            const response = await this.axiosInstance.get(`/analytics/posts/${postId}`);
            // Analytics might not be available for all posts
     * Group posts by profile
    private groupByProfile(posts: HootSuitePost[]): Record<string, number> {
        const groups: Record<string, number> = {};
            post.socialProfileIds.forEach(profileId => {
                groups[profileId] = (groups[profileId] || 0) + 1;
     * Group posts by scheduled date
    private groupByDate(posts: SocialPost[]): Record<string, number> {
                const dateKey = post.scheduledFor.toISOString().split('T')[0];
                groups[dateKey] = (groups[dateKey] || 0) + 1;
                Name: 'ScheduledPosts',
        return 'Retrieves scheduled posts from HootSuite with optional date filtering and analytics';
