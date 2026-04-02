import { LinkedInBaseAction, LinkedInAnalytics } from '../linkedin-base.action';
 * Action to get analytics for a LinkedIn post
@RegisterClass(BaseAction, 'LinkedInGetPostAnalyticsAction')
export class LinkedInGetPostAnalyticsAction extends LinkedInBaseAction {
     * Get analytics for a specific LinkedIn post
            const authorType = this.getParamValue(Params, 'AuthorType') || 'organization'; // 'personal' or 'organization'
            let analytics: LinkedInAnalytics | null = null;
                // Get organization analytics
                analytics = await this.getOrganizationPostAnalytics(postId, organizationUrn, timeRange);
                // Get personal post analytics (limited)
                analytics = await this.getPersonalPostAnalytics(postId);
            if (!analytics) {
                throw new Error('No analytics data available for this post');
            const normalizedAnalytics = this.normalizeAnalytics(analytics);
            const rawAnalyticsParam = outputParams.find(p => p.Name === 'RawAnalytics');
            if (rawAnalyticsParam) rawAnalyticsParam.Value = analytics;
                Message: `Successfully retrieved analytics for post ${postId}`,
                Message: `Failed to get post analytics: ${errorMessage}`,
     * Get analytics for an organization post
    private async getOrganizationPostAnalytics(shareId: string, organizationUrn: string, timeRange?: any): Promise<LinkedInAnalytics> {
            // Add time range if specified
            if (timeRange) {
                if (timeRange.start) {
            const response = await this.axiosInstance.get('/organizationalEntityShareStatistics', { params });
                const stats = response.data.elements[0];
                    totalShareStatistics: stats.totalShareStatistics || {},
                    timeRange: timeRange ? {
                        start: new Date(timeRange.start).getTime(),
                        end: new Date(timeRange.end || Date.now()).getTime()
            throw new Error('No analytics data found');
            LogError(`Failed to get organization post analytics: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get analytics for a personal post (limited data available)
    private async getPersonalPostAnalytics(shareId: string): Promise<LinkedInAnalytics> {
            // For personal posts, we need to fetch the post itself to get basic engagement metrics
                        engagement: response.data.likesSummary?.totalLikes || 0 + response.data.commentsSummary?.totalComments || 0,
                        shareCount: 0, // Not directly available
            throw new Error('Post not found');
                Name: 'RawAnalytics',
        return 'Retrieves detailed analytics for a LinkedIn post (organization posts have more detailed analytics than personal posts)';
