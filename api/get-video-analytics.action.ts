 * Action to get analytics for specific TikTok videos
@RegisterClass(BaseAction, 'GetVideoAnalyticsAction')
export class GetVideoAnalyticsAction extends TikTokBaseAction {
     * Get analytics for TikTok videos
            const videoIds = this.getParamValue(Params, 'VideoIDs');
            const dateRange = this.getParamValue(Params, 'DateRange');
            const metrics = this.getParamValue(Params, 'Metrics') || ['views', 'likes', 'comments', 'shares'];
            if (!videoIds || !Array.isArray(videoIds) || videoIds.length === 0) {
                throw new Error('VideoIDs array is required');
            // Get analytics for each video
            const analyticsResults: any[] = [];
            for (const videoId of videoIds) {
                    // TikTok API endpoint for video analytics
                        `/v2/video/data/`,
                            fields: metrics.join(',')
                    const videoData = response.data;
                    const analytics: SocialAnalytics = this.normalizeAnalytics(videoData);
                    analyticsResults.push({
                        analytics,
                        title: videoData.title,
                        publishedAt: new Date(videoData.create_time * 1000),
                        url: videoData.share_url,
                        performanceScore: this.calculatePerformanceScore(analytics),
                        rawData: videoData
            // Calculate aggregate metrics
            const aggregateMetrics = this.calculateAggregateMetrics(analyticsResults);
                totalVideosAnalyzed: analyticsResults.length,
                failedVideos: errors.length,
                dateRange: dateRange || 'all-time',
                aggregateMetrics,
                topPerformingVideo: analyticsResults.length > 0 
                    ? analyticsResults.reduce((best, current) => 
                        current.performanceScore > best.performanceScore ? current : best
                    ) : null,
                errors: errors.length > 0 ? errors : undefined
            if (analyticsParam) analyticsParam.Value = analyticsResults;
            const message = errors.length > 0
                ? `Retrieved analytics for ${analyticsResults.length} videos with ${errors.length} failures`
                : `Successfully retrieved analytics for ${analyticsResults.length} videos`;
                ResultCode: errors.length > 0 ? 'PARTIAL_SUCCESS' : 'SUCCESS',
                Message: `Failed to get TikTok video analytics: ${errorMessage}`,
     * Calculate performance score for a video
    private calculatePerformanceScore(analytics: SocialAnalytics): number {
        // Simple scoring algorithm - can be customized
        const likeWeight = 10;
        const commentWeight = 20;
        const shareWeight = 30;
            (analytics.videoViews || 0) * viewWeight +
            analytics.likes * likeWeight +
            analytics.comments * commentWeight +
            analytics.shares * shareWeight
     * Calculate aggregate metrics across all videos
    private calculateAggregateMetrics(analyticsResults: any[]): any {
        if (analyticsResults.length === 0) {
        const totals = analyticsResults.reduce((acc, result) => ({
            views: acc.views + (result.analytics.videoViews || 0),
            likes: acc.likes + result.analytics.likes,
            comments: acc.comments + result.analytics.comments,
            shares: acc.shares + result.analytics.shares,
            engagements: acc.engagements + result.analytics.engagements
            views: 0,
            engagements: 0
        const count = analyticsResults.length;
            total: totals,
            average: {
                views: Math.round(totals.views / count),
                likes: Math.round(totals.likes / count),
                comments: Math.round(totals.comments / count),
                shares: Math.round(totals.shares / count),
                engagements: Math.round(totals.engagements / count)
            engagementRate: totals.views > 0 
                ? ((totals.engagements / totals.views) * 100).toFixed(2) + '%'
                Name: 'VideoIDs',
                Name: 'Metrics',
                Value: ['views', 'likes', 'comments', 'shares']
        return 'Retrieves detailed analytics for specific TikTok videos including views, likes, comments, and shares';
 * Action to get analytics for YouTube videos
@RegisterClass(BaseAction, 'YouTubeGetVideoAnalyticsAction')
export class YouTubeGetVideoAnalyticsAction extends YouTubeBaseAction {
     * Get analytics for YouTube videos
            const metricsRequested = this.getParamValue(Params, 'Metrics') || ['views', 'likes', 'comments', 'shares'];
            const dimensions = this.getParamValue(Params, 'Dimensions');
            if (!videoIds || (Array.isArray(videoIds) && videoIds.length === 0)) {
                throw new Error('VideoIDs parameter is required');
            // Convert single video ID to array
            const videoIdArray = Array.isArray(videoIds) ? videoIds : [videoIds];
            // YouTube Data API provides basic statistics
            // For advanced analytics, YouTube Analytics API would be needed
            const videoDetails = await this.getVideoStatistics(videoIdArray, ContextUser);
            // Process analytics for each video
            const analyticsData = videoDetails.map((video: any) => {
                const snippet = video.snippet || {};
                    videoId: video.id,
                    title: snippet.title,
                    publishedAt: snippet.publishedAt,
                        views: parseInt(stats.viewCount || '0'),
                        likes: parseInt(stats.likeCount || '0'),
                        dislikes: parseInt(stats.dislikeCount || '0'),
                        comments: parseInt(stats.commentCount || '0'),
                        favorites: parseInt(stats.favoriteCount || '0'),
                        // Engagement rate calculation
                        engagementRate: this.calculateEngagementRate(stats),
                        // Like/dislike ratio
                        likeRatio: this.calculateLikeRatio(stats)
                    duration: video.contentDetails?.duration,
                    privacyStatus: video.status?.privacyStatus,
                    categoryId: snippet.categoryId,
                    tags: snippet.tags || [],
                    // Thumbnail for reference
                    thumbnail: snippet.thumbnails?.high?.url || snippet.thumbnails?.default?.url
            const aggregateMetrics = this.calculateAggregateMetrics(analyticsData);
            // If YouTube Analytics API was available, we could get:
            // - Watch time
            // - Average view duration
            // - Impressions
            // - Click-through rate
            // - Revenue data
            // - Geographic data
            // - Traffic sources
            // - Device types
            // - Audience retention
                totalVideos: analyticsData.length,
                    start: startDate || 'N/A',
                    end: endDate || 'N/A'
                aggregateMetrics: aggregateMetrics,
                topPerformers: this.getTopPerformers(analyticsData),
                quotaCost: this.getQuotaCost('videos.list') * Math.ceil(videoIdArray.length / 50)
            if (analyticsParam) analyticsParam.Value = analyticsData;
                Message: `Retrieved analytics for ${analyticsData.length} videos`,
                Message: `Failed to get video analytics: ${errorMessage}`,
     * Get video statistics from YouTube Data API
    private async getVideoStatistics(videoIds: string[], contextUser?: any): Promise<any[]> {
        // YouTube allows up to 50 video IDs per request
        const chunks = this.chunkArray(videoIds, 50);
        for (const chunk of chunks) {
                    part: 'snippet,statistics,contentDetails,status',
                    id: chunk.join(',')
            if (response.items) {
                results.push(...response.items);
    private calculateEngagementRate(stats: any): number {
        if (views === 0) return 0;
        const engagements = parseInt(stats.likeCount || '0') + 
                           parseInt(stats.dislikeCount || '0') + 
                           parseInt(stats.commentCount || '0');
        return Math.round((engagements / views) * 10000) / 100; // Percentage with 2 decimals
     * Calculate like ratio
    private calculateLikeRatio(stats: any): number {
        const likes = parseInt(stats.likeCount || '0');
        const dislikes = parseInt(stats.dislikeCount || '0');
        const total = likes + dislikes;
        return Math.round((likes / total) * 10000) / 100; // Percentage with 2 decimals
    private calculateAggregateMetrics(analyticsData: any[]): any {
        const totals = analyticsData.reduce((acc, video) => {
            acc.views += video.metrics.views;
            acc.likes += video.metrics.likes;
            acc.dislikes += video.metrics.dislikes;
            acc.comments += video.metrics.comments;
            acc.favorites += video.metrics.favorites;
        }, { views: 0, likes: 0, dislikes: 0, comments: 0, favorites: 0 });
        const avgEngagement = analyticsData.reduce((sum, v) => sum + v.metrics.engagementRate, 0) / analyticsData.length;
        const avgLikeRatio = analyticsData.reduce((sum, v) => sum + v.metrics.likeRatio, 0) / analyticsData.length;
                views: Math.round(totals.views / analyticsData.length),
                likes: Math.round(totals.likes / analyticsData.length),
                dislikes: Math.round(totals.dislikes / analyticsData.length),
                comments: Math.round(totals.comments / analyticsData.length),
                engagementRate: Math.round(avgEngagement * 100) / 100,
                likeRatio: Math.round(avgLikeRatio * 100) / 100
     * Get top performing videos
    private getTopPerformers(analyticsData: any[]): any {
            byViews: [...analyticsData].sort((a, b) => b.metrics.views - a.metrics.views).slice(0, 5),
            byEngagement: [...analyticsData].sort((a, b) => b.metrics.engagementRate - a.metrics.engagementRate).slice(0, 5),
            byLikes: [...analyticsData].sort((a, b) => b.metrics.likes - a.metrics.likes).slice(0, 5)
     * Chunk array into smaller arrays
    private chunkArray<T>(array: T[], size: number): T[][] {
        const chunks: T[][] = [];
        for (let i = 0; i < array.length; i += size) {
            chunks.push(array.slice(i, i + size));
        return chunks;
                Name: 'Dimensions',
        return 'Gets analytics data for YouTube videos including views, engagement, and performance metrics';
