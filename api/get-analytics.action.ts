 * Action to get detailed analytics for a specific Buffer post
@RegisterClass(BaseAction, 'BufferGetAnalyticsAction')
export class BufferGetAnalyticsAction extends BufferBaseAction {
     * Get analytics for a Buffer post
            // Get analytics
            const analyticsData = await this.getAnalytics(updateId);
            // Normalize analytics
            const normalizedAnalytics = this.normalizeAnalytics(analyticsData);
            // Create detailed analytics object
            const detailedAnalytics = {
                metrics: normalizedAnalytics,
                platformSpecific: analyticsData,
                    totalEngagements: normalizedAnalytics.engagements,
                    engagementRate: analyticsData.reach > 0 ? 
                        (normalizedAnalytics.engagements / analyticsData.reach) * 100 : 0,
                    primaryMetric: this.determinePrimaryMetric(analyticsData),
                    performanceLevel: this.calculatePerformanceLevel(normalizedAnalytics)
            const analyticsParam = outputParams.find(p => p.Name === 'Analytics');
            if (analyticsParam) analyticsParam.Value = detailedAnalytics;
                Message: `Successfully retrieved analytics for Buffer post ${updateId}`,
                Message: `Failed to get analytics: ${errorMessage}`,
     * Determine the primary metric based on the platform
    private determinePrimaryMetric(analytics: any): { name: string; value: number } {
        // Twitter/X focuses on retweets
        if (analytics.retweets !== undefined) {
            return { name: 'Retweets', value: analytics.retweets || 0 };
        // Facebook focuses on shares
        if (analytics.shares !== undefined) {
            return { name: 'Shares', value: analytics.shares || 0 };
        // LinkedIn focuses on reach
        if (analytics.reach !== undefined && analytics.reach > 0) {
            return { name: 'Reach', value: analytics.reach };
        // Default to clicks
        return { name: 'Clicks', value: analytics.clicks || 0 };
     * Calculate performance level based on engagement metrics
    private calculatePerformanceLevel(analytics: any): 'low' | 'medium' | 'high' | 'viral' {
        const engagementRate = analytics.reach > 0 ? 
            (analytics.engagements / analytics.reach) * 100 : 0;
        if (engagementRate >= 10) return 'viral';
        if (engagementRate >= 5) return 'high';
        if (engagementRate >= 2) return 'medium';
                Name: 'Analytics',
        return 'Retrieves detailed analytics and interaction metrics for a specific Buffer post';
import { HootSuiteBaseAction, HootSuiteAnalytics } from '../hootsuite-base.action';
import { SocialAnalytics } from '../../../base/base-social.action';
 * Action to retrieve analytics data from HootSuite
@RegisterClass(BaseAction, 'HootSuiteGetAnalyticsAction')
export class HootSuiteGetAnalyticsAction extends HootSuiteBaseAction {
     * Get analytics data from HootSuite
            const metricsType = this.getParamValue(Params, 'MetricsType') || 'all';
            const aggregateByProfile = this.getParamValue(Params, 'AggregateByProfile') || false;
            // Determine what analytics to fetch
            let analyticsData: any;
            if (postId) {
                // Get analytics for specific post
                analyticsData = await this.getPostAnalytics(postId, startDate, endDate);
            } else if (profileId) {
                // Get analytics for specific profile
                analyticsData = await this.getProfileAnalytics(profileId, startDate, endDate, metricsType);
                // Get analytics for all profiles
                analyticsData = await this.getAllProfilesAnalytics(startDate, endDate, metricsType, aggregateByProfile);
            const normalizedAnalytics = this.processAnalyticsData(analyticsData, metricsType);
                    start: startDate || 'Not specified',
                    end: endDate || 'Not specified'
                metricsType: metricsType,
                totalMetrics: this.calculateTotalMetrics(normalizedAnalytics),
                topPerformingPosts: this.getTopPerformingPosts(normalizedAnalytics),
                engagementRate: this.calculateEngagementRate(normalizedAnalytics)
            if (analyticsParam) analyticsParam.Value = normalizedAnalytics;
                Message: 'Successfully retrieved analytics data',
     * Get analytics for a specific post
    private async getPostAnalytics(postId: string, startDate?: string, endDate?: string): Promise<HootSuiteAnalytics> {
        const params: any = {};
        if (startDate) params.startTime = this.formatHootSuiteDate(startDate);
        if (endDate) params.endTime = this.formatHootSuiteDate(endDate);
        const response = await this.axiosInstance.get(`/analytics/posts/${postId}`, { params });
     * Get analytics for a specific profile
    private async getProfileAnalytics(
        startDate?: string, 
        endDate?: string,
        metricsType?: string
            socialProfileIds: profileId
        if (metricsType && metricsType !== 'all') params.metrics = this.getMetricsList(metricsType);
        const response = await this.axiosInstance.get('/analytics/profiles', { params });
     * Get analytics for all profiles
    private async getAllProfilesAnalytics(
        metricsType?: string,
        aggregateByProfile?: boolean
        // First get all profiles
            return { data: [] };
            socialProfileIds: profiles.map(p => p.id).join(',')
        if (aggregateByProfile) params.groupBy = 'socialProfile';
     * Get list of metrics based on type
    private getMetricsList(metricsType: string): string {
        const metricsMap: Record<string, string[]> = {
            'engagement': ['likes', 'comments', 'shares', 'engagements'],
            'reach': ['impressions', 'reach'],
            'clicks': ['clicks', 'linkClicks'],
            'all': ['likes', 'comments', 'shares', 'clicks', 'impressions', 'engagements', 'reach']
        return (metricsMap[metricsType] || metricsMap['all']).join(',');
     * Process and normalize analytics data
    private processAnalyticsData(data: any, metricsType: string): SocialAnalytics[] {
        if (!data || !data.data) return [];
        const results: SocialAnalytics[] = [];
        if (Array.isArray(data.data)) {
            data.data.forEach((item: any) => {
                results.push(this.normalizeAnalytics(item.metrics));
        } else if (data.metrics) {
            results.push(this.normalizeAnalytics(data.metrics));
     * Calculate total metrics across all data
    private calculateTotalMetrics(analytics: SocialAnalytics[]): SocialAnalytics {
        return analytics.reduce((total, current) => ({
            impressions: total.impressions + current.impressions,
            engagements: total.engagements + current.engagements,
            clicks: total.clicks + current.clicks,
            shares: total.shares + current.shares,
            comments: total.comments + current.comments,
            likes: total.likes + current.likes,
            reach: total.reach + current.reach,
            saves: (total.saves || 0) + (current.saves || 0),
            videoViews: (total.videoViews || 0) + (current.videoViews || 0),
            platformMetrics: {}
        }), {
            impressions: 0,
            engagements: 0,
            clicks: 0,
            shares: 0,
            comments: 0,
            likes: 0,
            reach: 0,
            saves: 0,
            videoViews: 0,
     * Get top performing posts based on engagement
    private getTopPerformingPosts(analytics: SocialAnalytics[]): SocialAnalytics[] {
        return analytics
            .sort((a, b) => b.engagements - a.engagements)
     * Calculate average engagement rate
    private calculateEngagementRate(analytics: SocialAnalytics[]): number {
        const total = this.calculateTotalMetrics(analytics);
        if (total.impressions === 0) return 0;
        return (total.engagements / total.impressions) * 100;
                Name: 'MetricsType',
                Name: 'AggregateByProfile',
        return 'Retrieves analytics data from HootSuite for posts, profiles, or overall account performance';
import { TwitterBaseAction, Tweet, TwitterMetrics } from '../twitter-base.action';
 * Action to get analytics for tweets or user account from Twitter/X
@RegisterClass(BaseAction, 'TwitterGetAnalyticsAction')
export class TwitterGetAnalyticsAction extends TwitterBaseAction {
     * Get analytics from Twitter
            const analyticsType = this.getParamValue(Params, 'AnalyticsType') || 'account'; // 'account' or 'tweets'
            const tweetIds = this.getParamValue(Params, 'TweetIDs');
            const granularity = this.getParamValue(Params, 'Granularity') || 'day'; // 'hour', 'day', 'total'
            if (analyticsType === 'tweets') {
                // Get analytics for specific tweets
                if (!tweetIds || !Array.isArray(tweetIds) || tweetIds.length === 0) {
                    throw new Error('TweetIDs array is required for tweet analytics');
                return await this.getTweetAnalytics(Params, tweetIds);
                // Get account-level analytics
                return await this.getAccountAnalytics(Params, startDate, endDate, granularity);
     * Get analytics for specific tweets
    private async getTweetAnalytics(params: ActionParam[], tweetIds: string[]): Promise<ActionResultSimple> {
            LogStatus(`Getting analytics for ${tweetIds.length} tweets...`);
            // Twitter API v2 requires organic metrics scope for detailed analytics
            // We'll get public metrics for each tweet
            const tweetAnalytics: any[] = [];
            // Process in batches of 100 (API limit)
            const batchSize = 100;
            for (let i = 0; i < tweetIds.length; i += batchSize) {
                const batch = tweetIds.slice(i, i + batchSize);
                const ids = batch.join(',');
                const response = await this.axiosInstance.get('/tweets', {
                        'ids': ids,
                        'tweet.fields': 'id,text,created_at,public_metrics,organic_metrics,promoted_metrics',
                    for (const tweet of response.data.data) {
                        const metrics: TwitterMetrics = {
                            impression_count: 0,
                            engagement_count: 0,
                            retweet_count: 0,
                            reply_count: 0,
                            like_count: 0,
                            quote_count: 0,
                            bookmark_count: 0,
                            url_link_clicks: 0,
                            user_profile_clicks: 0
                        // Combine public and organic metrics if available
                        if (tweet.public_metrics) {
                            Object.assign(metrics, tweet.public_metrics);
                        if (tweet.organic_metrics) {
                            Object.assign(metrics, tweet.organic_metrics);
                        // Calculate engagement count
                        metrics.engagement_count = 
                            metrics.retweet_count + 
                            metrics.reply_count + 
                            metrics.like_count + 
                            metrics.quote_count +
                            metrics.url_link_clicks +
                            metrics.user_profile_clicks;
                        const normalizedAnalytics = this.normalizeAnalytics(metrics);
                        tweetAnalytics.push({
                            tweetId: tweet.id,
                            text: tweet.text.substring(0, 100) + (tweet.text.length > 100 ? '...' : ''),
                            createdAt: tweet.created_at,
                            engagementRate: metrics.impression_count > 0 
                                ? ((metrics.engagement_count / metrics.impression_count) * 100).toFixed(2) + '%'
            const aggregateMetrics = this.calculateAggregateMetrics(tweetAnalytics);
            const outputParams = [...params];
            if (analyticsParam) analyticsParam.Value = tweetAnalytics;
            const aggregateParam = outputParams.find(p => p.Name === 'AggregateMetrics');
            if (aggregateParam) aggregateParam.Value = aggregateMetrics;
                Message: `Successfully retrieved analytics for ${tweetAnalytics.length} tweets`,
     * Get account-level analytics
    private async getAccountAnalytics(
        params: ActionParam[], 
        granularity?: string
            // Get current user
            LogStatus(`Getting account analytics for @${currentUser.username}...`);
            // For account analytics, we'll analyze recent tweets performance
            const queryParams: Record<string, any> = {
                'tweet.fields': 'id,text,created_at,public_metrics,organic_metrics',
                queryParams['start_time'] = this.formatTwitterDate(startDate);
                queryParams['end_time'] = this.formatTwitterDate(endDate);
            // Get user's tweets
            const tweets = await this.getPaginatedTweets(
                `/users/${currentUser.id}/tweets`, 
                200 // Get up to 200 tweets for analysis
            // Calculate time-based analytics
            const timeBasedAnalytics = this.calculateTimeBasedAnalytics(tweets, granularity || 'day');
            // Calculate overall metrics
            const overallMetrics = {
                totalTweets: tweets.length,
                totalImpressions: 0,
                totalRetweets: 0,
                totalQuotes: 0,
                averageEngagementRate: 0,
                topPerformingTweets: [] as any[]
            tweets.forEach(tweet => {
                    overallMetrics.totalImpressions += tweet.public_metrics.impression_count || 0;
                    overallMetrics.totalLikes += tweet.public_metrics.like_count || 0;
                    overallMetrics.totalRetweets += tweet.public_metrics.retweet_count || 0;
                    overallMetrics.totalReplies += tweet.public_metrics.reply_count || 0;
                    overallMetrics.totalQuotes += tweet.public_metrics.quote_count || 0;
                    const engagement = 
                        (tweet.public_metrics.retweet_count || 0) +
                        (tweet.public_metrics.quote_count || 0);
                    overallMetrics.totalEngagements += engagement;
            // Calculate average engagement rate
            if (overallMetrics.totalImpressions > 0) {
                overallMetrics.averageEngagementRate = 
                    parseFloat(((overallMetrics.totalEngagements / overallMetrics.totalImpressions) * 100).toFixed(2));
            // Get top performing tweets
            overallMetrics.topPerformingTweets = tweets
                .filter(t => t.public_metrics)
                    const aEngagement = this.calculateTweetEngagement(a.public_metrics!);
                    const bEngagement = this.calculateTweetEngagement(b.public_metrics!);
                    return bEngagement - aEngagement;
                .map(tweet => ({
                    metrics: tweet.public_metrics,
                    engagement: this.calculateTweetEngagement(tweet.public_metrics!)
            const overallParam = outputParams.find(p => p.Name === 'OverallMetrics');
            if (overallParam) overallParam.Value = overallMetrics;
            const timeBasedParam = outputParams.find(p => p.Name === 'TimeBasedAnalytics');
            if (timeBasedParam) timeBasedParam.Value = timeBasedAnalytics;
                Message: `Successfully retrieved account analytics for ${tweets.length} tweets`,
     * Calculate aggregate metrics from tweet analytics
    private calculateAggregateMetrics(tweetAnalytics: any[]): any {
        const aggregate = {
            bestPerformingTweet: null as any,
            worstPerformingTweet: null as any
        let bestEngagement = -1;
        let worstEngagement = Infinity;
        tweetAnalytics.forEach(analytics => {
            const metrics = analytics.metrics;
            aggregate.totalImpressions += metrics.impressions;
            aggregate.totalEngagements += metrics.engagements;
            aggregate.totalLikes += metrics.likes;
            aggregate.totalRetweets += metrics.shares;
            aggregate.totalReplies += metrics.comments;
            if (metrics.engagements > bestEngagement) {
                bestEngagement = metrics.engagements;
                aggregate.bestPerformingTweet = analytics;
            if (metrics.engagements < worstEngagement) {
                worstEngagement = metrics.engagements;
                aggregate.worstPerformingTweet = analytics;
        if (aggregate.totalImpressions > 0) {
            aggregate.averageEngagementRate = 
                parseFloat(((aggregate.totalEngagements / aggregate.totalImpressions) * 100).toFixed(2));
        return aggregate;
     * Calculate time-based analytics
    private calculateTimeBasedAnalytics(tweets: Tweet[], granularity: string): any[] {
        const buckets: Map<string, any> = new Map();
            const date = new Date(tweet.created_at);
            let bucketKey: string;
            switch (granularity) {
                case 'hour':
                    bucketKey = `${date.toISOString().slice(0, 13)}:00:00Z`;
                    bucketKey = date.toISOString().slice(0, 10);
                case 'total':
                    bucketKey = 'total';
            if (!buckets.has(bucketKey)) {
                buckets.set(bucketKey, {
                    period: bucketKey,
                    tweets: 0,
                    retweets: 0,
                    replies: 0
            const bucket = buckets.get(bucketKey)!;
            bucket.tweets++;
                bucket.impressions += tweet.public_metrics.impression_count || 0;
                bucket.likes += tweet.public_metrics.like_count || 0;
                bucket.retweets += tweet.public_metrics.retweet_count || 0;
                bucket.replies += tweet.public_metrics.reply_count || 0;
                bucket.engagements += this.calculateTweetEngagement(tweet.public_metrics);
        return Array.from(buckets.values()).sort((a, b) => 
            a.period.localeCompare(b.period)
     * Calculate tweet engagement from public metrics
    private calculateTweetEngagement(metrics: any): number {
        return (metrics.like_count || 0) +
               (metrics.retweet_count || 0) +
               (metrics.reply_count || 0) +
               (metrics.quote_count || 0);
                Name: 'AnalyticsType',
                Value: 'account' // 'account' or 'tweets'
                Name: 'Granularity',
                Value: 'day' // 'hour', 'day', 'total'
                Name: 'AggregateMetrics',
                Name: 'OverallMetrics',
                Name: 'TimeBasedAnalytics',
        return 'Gets analytics data from Twitter/X for specific tweets or account-level metrics with time-based analysis';
