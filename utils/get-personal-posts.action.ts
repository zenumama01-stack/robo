 * Action to get posts from a LinkedIn personal profile
@RegisterClass(BaseAction, 'LinkedInGetPersonalPostsAction')
export class LinkedInGetPersonalPostsAction extends LinkedInBaseAction {
     * Get posts from the authenticated user's LinkedIn profile
            // Get current user URN
            LogStatus(`Fetching posts for user: ${userUrn}`);
            const shares = await this.getShares(userUrn, count, startIndex);
                // Personal posts have limited analytics access
                        // Note: Personal share analytics are limited compared to organization analytics
                        const analytics = await this.getPersonalPostAnalytics(share.id);
                        if (analytics) {
            LogStatus(`Retrieved ${posts.length} personal posts`);
                Message: `Successfully retrieved ${posts.length} personal posts`,
                Message: `Failed to get personal posts: ${errorMessage}`,
     * Get analytics for a personal post
     * Note: LinkedIn provides limited analytics for personal posts
    private async getPersonalPostAnalytics(shareId: string): Promise<any> {
            // LinkedIn v2 API has limited personal analytics
            // We can get basic engagement data from the share itself
            const response = await this.axiosInstance.get(`/ugcPosts/${shareId}`);
                // Extract available metrics from the post data
                        impressionCount: 0, // Not available for personal posts
                        clickCount: 0, // Not available for personal posts
                        engagement: 0, // Not available for personal posts
                        likeCount: response.data.likesSummary?.totalLikes || 0,
                        commentCount: response.data.commentsSummary?.totalComments || 0,
                        shareCount: 0, // Not available in this endpoint
                        uniqueImpressionsCount: 0 // Not available for personal posts
            LogError(`Failed to get personal post analytics: ${error instanceof Error ? error.message : 'Unknown error'}`);
        return 'Retrieves posts from the authenticated user\'s LinkedIn personal profile with limited analytics data';
