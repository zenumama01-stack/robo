import { FacebookBaseAction, FacebookInsight } from '../facebook-base.action';
 * Retrieves comprehensive analytics and insights for a Facebook page.
 * Provides metrics like page views, likes, engagement, demographics, and more.
@RegisterClass(BaseAction, 'FacebookGetPageInsightsAction')
export class FacebookGetPageInsightsAction extends FacebookBaseAction {
        return 'Retrieves comprehensive analytics for a Facebook page including views, engagement, demographics, and performance metrics';
                Name: 'MetricTypes',
                Name: 'Period',
                Value: 'day',
                Name: 'IncludeDemographics',
                Name: 'IncludeVideoMetrics',
                Name: 'CompareWithPrevious',
            const metricTypes = this.getParamValue(Params, 'MetricTypes') as string[];
            const period = this.getParamValue(Params, 'Period') as string || 'day';
            const startDate = this.getParamValue(Params, 'StartDate') as string;
            const endDate = this.getParamValue(Params, 'EndDate') as string;
            const includeDemographics = this.getParamValue(Params, 'IncludeDemographics') !== false;
            const includeVideoMetrics = this.getParamValue(Params, 'IncludeVideoMetrics') !== false;
            const compareWithPrevious = this.getParamValue(Params, 'CompareWithPrevious') as boolean;
            LogStatus(`Retrieving insights for Facebook page ${pageId}...`);
            // Build metrics list
            const metrics = metricTypes && metricTypes.length > 0 
                ? metricTypes 
                : this.getDefaultPageMetrics(includeDemographics, includeVideoMetrics);
            // Build date range parameters
            const dateParams: any = {};
                dateParams.since = new Date(startDate).toISOString();
                dateParams.until = new Date(endDate).toISOString();
            } else if (period !== 'lifetime') {
                // Default to last 30 days if no date range specified
                start.setDate(start.getDate() - 30);
                dateParams.since = start.toISOString();
                dateParams.until = end.toISOString();
            // Get page insights
            const insightsResponse = await axios.get(
                `${this.apiBaseUrl}/${pageId}/insights`,
                        metric: metrics.join(','),
                        period: period,
                        ...dateParams
            const insights: FacebookInsight[] = insightsResponse.data.data || [];
            // Get page info for context
            const pageInfoResponse = await axios.get(
                `${this.apiBaseUrl}/${pageId}`,
                        fields: 'id,name,category,fan_count,followers_count,about,cover,picture'
            const pageInfo = pageInfoResponse.data;
            // Process insights into categories
            const processedInsights = this.categorizeInsights(insights);
            // Get comparison data if requested
            let comparison = null;
            if (compareWithPrevious && startDate && endDate) {
                comparison = await this.getComparisonData(
                    pageId,
                    metrics,
                    period,
                    new Date(startDate),
                    new Date(endDate)
            const summary = this.calculateSummaryMetrics(processedInsights, pageInfo);
            LogStatus(`Successfully retrieved insights for page ${pageId}`);
                Message: 'Page insights retrieved successfully',
            LogError(`Failed to get Facebook page insights: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get default page metrics based on options
    private getDefaultPageMetrics(includeDemographics: boolean, includeVideo: boolean): string[] {
            // Page Views
            'page_views_total',
            'page_views_logged_in_unique',
            'page_views_by_site_logged_in_unique',
            // Page Likes
            'page_fan_adds',
            'page_fan_removes',
            'page_fan_adds_unique',
            'page_fans',
            // Engagement
            'page_engaged_users',
            'page_post_engagements',
            'page_consumptions',
            'page_consumptions_unique',
            'page_places_checkin_total',
            // Impressions
            'page_impressions',
            'page_impressions_unique',
            'page_impressions_paid',
            'page_impressions_paid_unique',
            'page_impressions_organic',
            'page_impressions_organic_unique',
            // Posts
            'page_posts_impressions',
            'page_posts_impressions_unique',
            'page_actions_post_reactions_total',
            'page_actions_post_reactions_like_total',
            'page_actions_post_reactions_love_total',
            'page_actions_post_reactions_wow_total',
            'page_actions_post_reactions_haha_total',
            'page_actions_post_reactions_sorry_total',
            'page_actions_post_reactions_anger_total',
            // CTA Clicks
            'page_total_actions',
            'page_cta_clicks_logged_in_total',
            // Messages
            'page_messages_total_messaging_connections',
            'page_messages_new_conversations',
            // Negative Feedback
            'page_negative_feedback',
            'page_negative_feedback_unique'
        if (includeDemographics) {
            metrics.push(
                'page_fans_gender_age',
                'page_fans_country',
                'page_fans_city',
                'page_fans_locale',
                'page_impressions_by_age_gender_unique',
                'page_engaged_users_by_age_gender'
        if (includeVideo) {
                'page_video_views',
                'page_video_views_paid',
                'page_video_views_organic',
                'page_video_views_autoplayed',
                'page_video_views_click_to_play',
                'page_video_views_unique',
                'page_video_repeat_views',
                'page_video_complete_views_30s',
                'page_video_complete_views_30s_paid',
                'page_video_complete_views_30s_organic',
                'page_video_views_10s',
                'page_video_views_10s_paid',
                'page_video_views_10s_organic'
     * Categorize insights by type for better organization
    private categorizeInsights(insights: FacebookInsight[]): Record<string, any> {
        const categories: Record<string, any> = {
            pageViews: {},
            engagement: {},
            likes: {},
            impressions: {},
            posts: {},
            reactions: {},
            demographics: {},
            video: {},
            messages: {},
            negativeFeedback: {}
            const name = insight.name;
            const value = insight.values?.[0]?.value;
            if (name.includes('page_views')) {
                categories.pageViews[name] = value;
            } else if (name.includes('engaged') || name.includes('engagement')) {
                categories.engagement[name] = value;
            } else if (name.includes('fan') || name === 'page_fans') {
                categories.likes[name] = value;
            } else if (name.includes('impressions')) {
                categories.impressions[name] = value;
            } else if (name.includes('posts')) {
                categories.posts[name] = value;
            } else if (name.includes('reactions')) {
                categories.reactions[name] = value;
            } else if (name.includes('gender') || name.includes('country') || name.includes('city') || name.includes('locale')) {
                categories.demographics[name] = value;
            } else if (name.includes('video')) {
                categories.video[name] = value;
            } else if (name.includes('messages')) {
                categories.messages[name] = value;
            } else if (name.includes('negative')) {
                categories.negativeFeedback[name] = value;
        return categories;
     * Calculate summary metrics
    private calculateSummaryMetrics(insights: Record<string, any>, pageInfo: any): any {
        const summary: any = {
            totalFans: pageInfo.fan_count || 0,
            totalFollowers: pageInfo.followers_count || 0,
            totalPageViews: insights.pageViews.page_views_total || 0,
            totalEngagements: insights.engagement.page_engaged_users || 0,
            totalImpressions: insights.impressions.page_impressions || 0,
            netNewFans: (insights.likes.page_fan_adds || 0) - (insights.likes.page_fan_removes || 0)
        // Calculate engagement rate if we have the data
        if (summary.totalImpressions > 0) {
            summary.engagementRate = (summary.totalEngagements / summary.totalImpressions) * 100;
        // Add reaction breakdown
        const reactions = insights.reactions;
        if (reactions) {
            summary.reactionBreakdown = {
                total: reactions.page_actions_post_reactions_total || 0,
                like: reactions.page_actions_post_reactions_like_total || 0,
                love: reactions.page_actions_post_reactions_love_total || 0,
                wow: reactions.page_actions_post_reactions_wow_total || 0,
                haha: reactions.page_actions_post_reactions_haha_total || 0,
                sorry: reactions.page_actions_post_reactions_sorry_total || 0,
                anger: reactions.page_actions_post_reactions_anger_total || 0
     * Get comparison data for previous period
    private async getComparisonData(
        pageId: string,
        metrics: string[],
        period: string,
        startDate: Date,
        endDate: Date
            const duration = endDate.getTime() - startDate.getTime();
            const previousStart = new Date(startDate.getTime() - duration);
            const previousEnd = new Date(startDate.getTime());
                        since: previousStart.toISOString(),
                        until: previousEnd.toISOString()
            const previousInsights = response.data.data || [];
            const processedPrevious = this.categorizeInsights(previousInsights);
                    start: previousStart.toISOString(),
                    end: previousEnd.toISOString()
                insights: processedPrevious
            LogError(`Failed to get comparison data: ${error}`);
