 * Action to get analytics for a YouTube channel
@RegisterClass(BaseAction, 'YouTubeGetChannelAnalyticsAction')
export class YouTubeGetChannelAnalyticsAction extends YouTubeBaseAction {
     * Get analytics for a YouTube channel
            const channelId = this.getParamValue(Params, 'ChannelID') || this.getCustomAttribute(1);
            const dateRange = this.getParamValue(Params, 'DateRange') || 'last30Days';
            const includeVideos = this.getParamValue(Params, 'IncludeVideos') ?? true;
            const videoLimit = this.getParamValue(Params, 'VideoLimit') || 50;
            // If no channel ID provided, get the authenticated user's channel
            let actualChannelId = channelId;
            if (!actualChannelId) {
                const channelResponse = await this.makeYouTubeRequest<any>(
                    '/channels',
                        part: 'id,snippet,statistics,contentDetails,brandingSettings',
                        mine: true
                if (!channelResponse.items || channelResponse.items.length === 0) {
                    throw new Error('No channel found for authenticated user');
                actualChannelId = channelResponse.items[0].id;
                // Get channel details for specified channel
                        part: 'snippet,statistics,contentDetails,brandingSettings',
                        id: actualChannelId
                    throw new Error(`Channel not found: ${actualChannelId}`);
            // Get channel details
            const channelData = await this.getChannelDetails(actualChannelId, ContextUser);
            const channel = channelData.items[0];
            const { start, end } = this.calculateDateRange(dateRange, startDate, endDate);
            // Get videos for the date range if requested
            let videos: any[] = [];
            let videoAnalytics: any = {};
            if (includeVideos) {
                videos = await this.getChannelVideosForDateRange(actualChannelId, start, end, videoLimit, ContextUser);
                if (videos.length > 0) {
                    videoAnalytics = await this.aggregateVideoAnalytics(videos);
            // Prepare channel analytics
            const channelAnalytics = {
                channelId: channel.id,
                channelTitle: channel.snippet.title,
                channelDescription: channel.snippet.description,
                channelUrl: `https://www.youtube.com/channel/${channel.id}`,
                customUrl: channel.snippet.customUrl,
                country: channel.snippet.country,
                publishedAt: channel.snippet.publishedAt,
                thumbnails: channel.snippet.thumbnails,
                // Overall statistics
                    viewCount: parseInt(channel.statistics.viewCount || '0'),
                    subscriberCount: parseInt(channel.statistics.subscriberCount || '0'),
                    videoCount: parseInt(channel.statistics.videoCount || '0'),
                    // Note: hiddenSubscriberCount indicates if subscriber count is public
                    subscribersHidden: channel.statistics.hiddenSubscriberCount
                // Date range specific analytics
                dateRangeAnalytics: {
                        start: start.toISOString(),
                        end: end.toISOString(),
                        days: Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24))
                    videosPublished: videos.length,
                    totalViews: videoAnalytics.totalViews || 0,
                    totalLikes: videoAnalytics.totalLikes || 0,
                    totalComments: videoAnalytics.totalComments || 0,
                    averageViewsPerVideo: videoAnalytics.averageViews || 0,
                    topPerformingVideos: videoAnalytics.topVideos || []
                // Growth metrics (if we had historical data)
                growth: {
                    note: 'Growth metrics require YouTube Analytics API access',
                    subscriberGrowth: 'N/A',
                    viewGrowth: 'N/A'
                // Content details
                contentDetails: {
                    relatedPlaylists: channel.contentDetails.relatedPlaylists,
                    uploadPlaylistId: channel.contentDetails.relatedPlaylists.uploads
                // Branding
                branding: {
                    keywords: channel.brandingSettings?.channel?.keywords,
                    unsubscribedTrailer: channel.brandingSettings?.channel?.unsubscribedTrailer,
                    featuredChannelsUrls: channel.brandingSettings?.channel?.featuredChannelsUrls
                quotaCost: this.getQuotaCost('channels.list') + 
                          (videos.length > 0 ? this.getQuotaCost('search.list') + this.getQuotaCost('videos.list') : 0)
            // Prepare summary
                channelHealth: this.calculateChannelHealth(channel, videoAnalytics),
                recommendations: this.generateRecommendations(channel, videoAnalytics),
                performanceIndicators: {
                    engagementRate: this.calculateEngagementRate(channel.statistics, videoAnalytics),
                    averageViewsPerSubscriber: this.calculateViewsPerSubscriber(channel.statistics),
                    uploadFrequency: this.calculateUploadFrequency(videos, start, end)
            if (analyticsParam) analyticsParam.Value = channelAnalytics;
            if (videosParam) videosParam.Value = videos;
                Message: `Retrieved analytics for channel: ${channel.snippet.title}`,
                Message: `Failed to get channel analytics: ${errorMessage}`,
     * Get channel details
    private async getChannelDetails(channelId: string, contextUser?: any): Promise<any> {
        return await this.makeYouTubeRequest(
                id: channelId
     * Get videos for a date range
    private async getChannelVideosForDateRange(
        channelId: string, 
        endDate: Date, 
        contextUser?: any
    ): Promise<any[]> {
        const searchResponse = await this.makeYouTubeRequest<any>(
                part: 'id',
                channelId: channelId,
                publishedAfter: startDate.toISOString(),
                publishedBefore: endDate.toISOString(),
                maxResults: Math.min(limit, 50),
                order: 'date'
        if (!searchResponse.items || searchResponse.items.length === 0) {
        const videoIds = searchResponse.items.map((item: any) => item.id.videoId).join(',');
                part: 'snippet,statistics,contentDetails',
        return videosResponse.items || [];
     * Aggregate video analytics
    private aggregateVideoAnalytics(videos: any[]): any {
        let totalViews = 0;
        let totalDislikes = 0;
        for (const video of videos) {
            const stats = video.statistics || {};
            totalViews += parseInt(stats.viewCount || '0');
            totalLikes += parseInt(stats.likeCount || '0');
            totalComments += parseInt(stats.commentCount || '0');
            totalDislikes += parseInt(stats.dislikeCount || '0');
        const topVideos = [...videos]
            .sort((a, b) => parseInt(b.statistics?.viewCount || '0') - parseInt(a.statistics?.viewCount || '0'))
                title: v.snippet.title,
                views: parseInt(v.statistics?.viewCount || '0'),
                likes: parseInt(v.statistics?.likeCount || '0'),
                comments: parseInt(v.statistics?.commentCount || '0'),
                publishedAt: v.snippet.publishedAt,
                url: `https://www.youtube.com/watch?v=${v.id}`
            totalLikes,
            totalComments,
            totalDislikes,
            averageViews: videos.length > 0 ? Math.round(totalViews / videos.length) : 0,
            averageLikes: videos.length > 0 ? Math.round(totalLikes / videos.length) : 0,
            topVideos
     * Calculate date range
    private calculateDateRange(
        dateRange: string, 
        endDate?: string
    ): { start: Date; end: Date } {
        const end = endDate ? new Date(endDate) : new Date();
        let start: Date;
            start = new Date(startDate);
            switch (dateRange) {
                case 'last7Days':
                    start = new Date(end.getTime() - 7 * 24 * 60 * 60 * 1000);
                case 'last30Days':
                    start = new Date(end.getTime() - 30 * 24 * 60 * 60 * 1000);
                case 'last90Days':
                    start = new Date(end.getTime() - 90 * 24 * 60 * 60 * 1000);
                    start = new Date(end.getTime() - 365 * 24 * 60 * 60 * 1000);
     * Calculate channel health score
    private calculateChannelHealth(channel: any, videoAnalytics: any): any {
        const stats = channel.statistics;
        const subscribers = parseInt(stats.subscriberCount || '0');
        const views = parseInt(stats.viewCount || '0');
        const videos = parseInt(stats.videoCount || '0');
        // Simple health score based on engagement
        let factors: string[] = [];
        // Subscriber to view ratio
        if (subscribers > 0 && views > subscribers * 100) {
            score += 25;
            factors.push('Good view-to-subscriber ratio');
        // Video count
        if (videos > 50) {
            factors.push('Active content creation');
        // Recent engagement
        if (videoAnalytics.averageViews > subscribers * 0.1) {
            factors.push('Strong recent engagement');
        // Upload consistency
        if (videoAnalytics.topVideos?.length >= 3) {
            factors.push('Consistent uploads');
            score: Math.min(score, 100),
            rating: score >= 75 ? 'Excellent' : score >= 50 ? 'Good' : score >= 25 ? 'Fair' : 'Needs Improvement',
            factors
     * Generate recommendations
    private generateRecommendations(channel: any, videoAnalytics: any): string[] {
        if (videoAnalytics.averageViews < subscribers * 0.05) {
            recommendations.push('Consider improving video titles and thumbnails to increase click-through rate');
        if (videoAnalytics.topVideos?.length < 3) {
            recommendations.push('Increase upload frequency to maintain audience engagement');
        if (!channel.brandingSettings?.channel?.keywords) {
            recommendations.push('Add channel keywords to improve discoverability');
        if (!channel.snippet.customUrl) {
            recommendations.push('Claim a custom URL for easier channel sharing');
    private calculateEngagementRate(stats: any, videoAnalytics: any): number {
        const views = videoAnalytics.totalViews || parseInt(stats.viewCount || '0');
        const engagements = (videoAnalytics.totalLikes || 0) + (videoAnalytics.totalComments || 0);
        return views > 0 ? Math.round((engagements / views) * 10000) / 100 : 0;
     * Calculate views per subscriber
    private calculateViewsPerSubscriber(stats: any): number {
        return subscribers > 0 ? Math.round((views / subscribers) * 100) / 100 : 0;
     * Calculate upload frequency
    private calculateUploadFrequency(videos: any[], start: Date, end: Date): string {
        const days = Math.ceil((end.getTime() - start.getTime()) / (1000 * 60 * 60 * 24));
        const videosPerDay = videos.length / days;
        if (videosPerDay >= 1) return `${Math.round(videosPerDay * 10) / 10} videos/day`;
        if (videosPerDay >= 1/7) return `${Math.round(videosPerDay * 7 * 10) / 10} videos/week`;
        return `${Math.round(videosPerDay * 30 * 10) / 10} videos/month`;
                Name: 'ChannelID',
                Value: 'last30Days'
                Name: 'IncludeVideos',
                Name: 'VideoLimit',
        return 'Gets comprehensive analytics for a YouTube channel including statistics, growth metrics, and performance indicators';
