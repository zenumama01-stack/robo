 * Retrieves detailed insights and analytics for a specific Facebook post.
 * Provides metrics like reach, impressions, engagement, and more.
@RegisterClass(BaseAction, 'FacebookGetPostInsightsAction')
export class FacebookGetPostInsightsAction extends FacebookBaseAction {
        return 'Retrieves detailed analytics and insights for a specific Facebook post including reach, impressions, and engagement metrics';
                Value: 'lifetime',
            const period = this.getParamValue(Params, 'Period') as string || 'lifetime';
            LogStatus(`Retrieving insights for Facebook post ${postId}...`);
            // Extract page ID from post ID (format: pageId_postId)
                : this.getDefaultPostMetrics(includeVideoMetrics);
            // Get post insights
                `${this.apiBaseUrl}/${postId}/insights`,
                        period: period
            // Get additional engagement data
            const engagementResponse = await axios.get(
                        fields: 'reactions.summary(true).limit(0),comments.summary(true).limit(0),shares,likes.summary(true).limit(0)'
            const engagementData = engagementResponse.data;
            // Get demographic insights if requested
            let demographics = null;
                demographics = await this.getPostDemographics(postId, pageToken);
            // Get post details for context
            const postDetails = await this.getPost(postId);
            // Process and organize insights
            const processedInsights = this.processInsights(insights);
            // Build comprehensive analytics object
                postDetails: {
                    message: postDetails.message,
                    createdTime: postDetails.created_time,
                    type: postDetails.attachments?.data?.[0]?.type || 'status',
                    permalinkUrl: postDetails.permalink_url
                metrics: processedInsights,
                    reactions: {
                        total: engagementData.reactions?.summary?.total_count || 0,
                        likes: engagementData.likes?.summary?.total_count || 0
                    comments: engagementData.comments?.summary?.total_count || 0,
                    shares: engagementData.shares?.count || 0
                demographics,
                retrievedAt: new Date().toISOString()
            // Calculate engagement rate if we have reach data
            const reach = processedInsights.post_impressions_unique || processedInsights.post_reach;
            if (reach && reach > 0) {
                const totalEngagements = analytics.engagement.reactions.total + 
                                       analytics.engagement.comments + 
                                       analytics.engagement.shares;
                (analytics as any).engagementRate = (totalEngagements / reach) * 100;
            LogStatus(`Successfully retrieved insights for post ${postId}`);
                Message: 'Post insights retrieved successfully',
            LogError(`Failed to get Facebook post insights: ${error instanceof Error ? error.message : 'Unknown error'}`);
            // Check if it's a permissions error
            if (error instanceof Error && error.message.includes('permissions')) {
                Message: 'Insufficient permissions to access post insights. Ensure the page token has insights permissions.',
     * Get default post metrics based on post type
    private getDefaultPostMetrics(includeVideo: boolean): string[] {
            // Reach and impressions
            'post_impressions',
            'post_impressions_unique',
            'post_impressions_paid',
            'post_impressions_paid_unique',
            'post_impressions_fan',
            'post_impressions_fan_unique',
            'post_impressions_organic',
            'post_impressions_organic_unique',
            'post_engaged_users',
            'post_engaged_fan',
            'post_clicks',
            'post_clicks_unique',
            // Reactions
            'post_reactions_by_type_total',
            // Negative feedback
            'post_negative_feedback',
            'post_negative_feedback_unique',
            // Activity
            'post_activity',
            'post_activity_by_action_type'
                'post_video_views',
                'post_video_views_unique',
                'post_video_views_10s',
                'post_video_views_10s_unique',
                'post_video_avg_time_watched',
                'post_video_complete_views_30s',
                'post_video_complete_views_30s_unique',
                'post_video_retention_graph',
                'post_video_view_time',
                'post_video_view_time_by_age_bucket_and_gender',
                'post_video_view_time_by_region_id'
     * Get demographic insights for a post
    private async getPostDemographics(postId: string, pageToken: string): Promise<any> {
            const demographicMetrics = [
                'post_impressions_by_age_gender_unique',
                'post_engaged_users_by_age_gender',
                'post_clicks_by_age_gender_unique'
                        metric: demographicMetrics.join(',')
            const insights = response.data.data || [];
            const demographics: Record<string, any> = {};
                if (insight.values?.[0]?.value) {
                    demographics[insight.name] = insight.values[0].value;
            return demographics;
            LogError(`Failed to get demographic insights: ${error}`);
     * Process raw insights into a more usable format
    private processInsights(insights: FacebookInsight[]): Record<string, any> {
        const processed: Record<string, any> = {};
                // Handle different value types
                if (typeof value === 'object' && !Array.isArray(value)) {
                    // For metrics like reactions_by_type
                    processed[insight.name] = value;
                // Add metadata
                processed[`${insight.name}_meta`] = {
                    title: insight.title,
                    description: insight.description,
                    period: insight.period
        return processed;
 * Retrieves detailed insights and analytics for a specific Instagram post.
 * Available metrics vary by post type (feed, reels, stories).
@RegisterClass(BaseAction, 'Instagram - Get Post Insights')
export class InstagramGetPostInsightsAction extends InstagramBaseAction {
            const metricTypes = this.getParamValue(params.Params, 'MetricTypes') as string[];
            const period = this.getParamValue(params.Params, 'Period') || 'lifetime';
            // First, get the post details to determine its type
            const postDetails = await this.getPostDetails(postId);
            if (!postDetails) {
                    Message: 'Post not found or access denied',
            // Determine available metrics based on post type
            const availableMetrics = this.getAvailableMetrics(postDetails.media_type);
            const requestedMetrics = metricTypes && metricTypes.length > 0 
                ? metricTypes.filter(m => availableMetrics.includes(m))
                : availableMetrics;
            if (requestedMetrics.length === 0) {
                    Message: 'No valid metrics specified for this post type',
                    ResultCode: 'INVALID_METRICS'
            // Get insights
            const insights = await this.getInsights(postId, requestedMetrics, period as any);
            // Parse and structure the insights data
            const parsedInsights = this.parseInsightsData(insights);
            const engagementRate = this.calculateEngagementRate(parsedInsights, postDetails);
                    postType: postDetails.media_type,
                    permalink: postDetails.permalink,
                    publishedAt: postDetails.timestamp,
                    metrics: parsedInsights,
                        engagementRate,
                        totalEngagements: this.calculateTotalEngagements(parsedInsights),
                        performanceScore: this.calculatePerformanceScore(parsedInsights)
                Message: 'Successfully retrieved post insights',
            LogError('Failed to retrieve Instagram post insights', error);
                Message: `Failed to retrieve post insights: ${error.message}`,
     * Get post details including media type
    private async getPostDetails(postId: string): Promise<any> {
                    fields: 'id,media_type,permalink,timestamp,caption,like_count,comments_count',
     * Get available metrics based on post type
    private getAvailableMetrics(mediaType: string): string[] {
        const baseMetrics = ['impressions', 'reach', 'engagement'];
        switch (mediaType) {
            case 'IMAGE':
            case 'CAROUSEL_ALBUM':
                return [...baseMetrics, 'saved', 'shares'];
            case 'VIDEO':
            case 'REELS':
                return [...baseMetrics, 'saved', 'shares', 'video_views', 'avg_watch_time', 'completion_rate'];
            case 'STORY':
                return ['impressions', 'reach', 'exits', 'replies', 'taps_forward', 'taps_back'];
     * Parse insights data into a structured format
    private parseInsightsData(insights: any[]): Record<string, any> {
                // For lifetime metrics, there's usually only one value
                const primaryValue = values[0];
                    value: primaryValue.value || 0,
                // For time series data (like daily metrics)
     * Calculate engagement rate
    private calculateEngagementRate(insights: Record<string, any>, postDetails: any): number {
        const reach = insights.reach?.value || postDetails.reach || 0;
        const engagement = insights.engagement?.value || 0;
        if (reach === 0) return 0;
        return Number(((engagement / reach) * 100).toFixed(2));
     * Calculate total engagements
    private calculateTotalEngagements(insights: Record<string, any>): number {
        const saves = insights.saved?.value || 0;
        const shares = insights.shares?.value || 0;
        return engagement + saves + shares;
     * Calculate a performance score (0-100)
    private calculatePerformanceScore(insights: Record<string, any>): number {
        // Simple scoring algorithm based on key metrics
        let score = 0;
        let factors = 0;
        // Engagement rate factor
            const engagementRate = (engagement / reach) * 100;
            score += Math.min(engagementRate * 10, 30); // Max 30 points
            factors++;
        // Reach factor (compared to impressions)
        const impressions = insights.impressions?.value || 0;
        if (impressions > 0) {
            const reachRate = (reach / impressions) * 100;
            score += Math.min(reachRate, 20); // Max 20 points
        // Saves factor
            const saveRate = (saves / reach) * 100;
            score += Math.min(saveRate * 20, 25); // Max 25 points
        // Video completion rate (for videos)
        const completionRate = insights.completion_rate?.value || 0;
        if (completionRate > 0) {
            score += Math.min(completionRate, 25); // Max 25 points
        // Normalize score
        if (factors === 0) return 0;
        return Math.round(Math.min(score, 100));
                Value: 'lifetime'
        return 'Retrieves detailed analytics and insights for a specific Instagram post including impressions, reach, engagement, and more.';
