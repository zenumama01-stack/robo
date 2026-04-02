 * Action to search for videos on TikTok
 * Note: TikTok API only allows searching within authenticated user's own videos
@RegisterClass(BaseAction, 'SearchVideosAction')
export class SearchVideosAction extends TikTokBaseAction {
     * Search for videos on TikTok
            const minViews = this.getParamValue(Params, 'MinViews');
            const minEngagement = this.getParamValue(Params, 'MinEngagement');
            const sortBy = this.getParamValue(Params, 'SortBy') || 'date';
            const limit = this.getParamValue(Params, 'Limit') || 50;
            // Build search params
            // Note about TikTok limitations
            const apiLimitation = {
                notice: 'TikTok API only allows searching within your own videos',
                recommendation: 'For broader search capabilities, use TikTok\'s web interface or mobile app',
                alternativeApproach: 'This search filters your own video library based on the provided criteria'
            // Get all user videos to search through
            const allVideos = await this.getUserVideos();
            // Apply search filters
            let filteredVideos = this.filterVideos(allVideos, {
                minViews,
            filteredVideos = this.sortVideos(filteredVideos, sortBy, sortOrder);
            // Apply pagination
            const paginatedVideos = filteredVideos.slice(offset, offset + limit);
            // Convert to social posts
            const socialPosts: SocialPost[] = paginatedVideos.map(video => this.normalizePost(video));
            const searchAnalytics = this.analyzeSearchResults(filteredVideos, paginatedVideos);
            // Create time-based insights for historical analysis
            const historicalInsights = this.generateHistoricalInsights(filteredVideos, startDate, endDate);
            // Generate content insights
            const contentInsights = this.generateContentInsights(filteredVideos, query, hashtags);
                totalResults: filteredVideos.length,
                returnedResults: paginatedVideos.length,
                    dateRange: startDate && endDate ? {
                        end: endDate,
                        daysSpanned: Math.ceil((new Date(endDate).getTime() - new Date(startDate).getTime()) / (1000 * 60 * 60 * 24))
                analytics: searchAnalytics,
                historicalInsights,
                contentInsights,
                apiLimitation,
                    hasMore: offset + limit < filteredVideos.length,
                    totalPages: Math.ceil(filteredVideos.length / limit)
            if (rawDataParam) rawDataParam.Value = paginatedVideos;
                Message: `Found ${filteredVideos.length} videos matching search criteria (showing ${paginatedVideos.length})`,
                Message: `Failed to search TikTok videos: ${errorMessage}`,
     * Filter videos based on search criteria
    private filterVideos(videos: TikTokVideo[], filters: any): TikTokVideo[] {
        return videos.filter(video => {
            // Query filter
            if (filters.query) {
                const query = filters.query.toLowerCase();
                const matchesQuery = video.title.toLowerCase().includes(query) ||
                                   video.description.toLowerCase().includes(query);
                if (!matchesQuery) return false;
            // Hashtag filter
            if (filters.hashtags && filters.hashtags.length > 0) {
                const hasMatchingHashtag = filters.hashtags.some((tag: string) => {
                    const cleanTag = tag.replace('#', '').toLowerCase();
                    return videoHashtags.includes(cleanTag);
                if (!hasMatchingHashtag) return false;
            if (filters.startDate) {
                const startTime = new Date(filters.startDate).getTime() / 1000;
                if (video.create_time < startTime) return false;
            if (filters.endDate) {
                const endTime = new Date(filters.endDate).getTime() / 1000;
                if (video.create_time > endTime) return false;
            // View count filter
            if (filters.minViews && video.view_count < filters.minViews) {
            // Engagement filter
            if (filters.minEngagement) {
                const engagement = video.like_count + video.comment_count + video.share_count;
                if (engagement < filters.minEngagement) return false;
     * Sort videos based on criteria
    private sortVideos(videos: TikTokVideo[], sortBy: string, order: string): TikTokVideo[] {
        const sorted = [...videos];
                case 'views':
                    aValue = a.view_count;
                    bValue = b.view_count;
                case 'likes':
                    aValue = a.like_count;
                    bValue = b.like_count;
                case 'comments':
                    aValue = a.comment_count;
                    bValue = b.comment_count;
                case 'shares':
                    aValue = a.share_count;
                    bValue = b.share_count;
                    aValue = a.like_count + a.comment_count + a.share_count;
                    bValue = b.like_count + b.comment_count + b.share_count;
                    aValue = a.create_time;
                    bValue = b.create_time;
            return order === 'desc' ? bValue - aValue : aValue - bValue;
    private analyzeSearchResults(allResults: TikTokVideo[], displayedResults: TikTokVideo[]): any {
        if (allResults.length === 0) {
                averageViews: 0,
                averageEngagement: 0,
                topPerformer: null
        const totalViews = allResults.reduce((sum, v) => sum + v.view_count, 0);
        const totalEngagement = allResults.reduce((sum, v) => 
            totalViews,
            totalEngagement,
            averageViews: Math.round(totalViews / allResults.length),
            averageEngagement: Math.round(totalEngagement / allResults.length),
            engagementRate: totalViews > 0 ? ((totalEngagement / totalViews) * 100).toFixed(2) + '%' : '0%',
            topPerformer: allResults.reduce((best, current) => 
                current.view_count > best.view_count ? current : best
            performanceDistribution: this.calculatePerformanceDistribution(allResults)
     * Generate historical insights
    private generateHistoricalInsights(videos: TikTokVideo[], startDate?: string, endDate?: string): any {
        if (videos.length === 0) return null;
        // Group videos by time period
        const timeGroups = this.groupVideosByTimePeriod(videos);
        // Calculate trends
        const viewTrend = this.calculateTrend(timeGroups, 'views');
        const engagementTrend = this.calculateTrend(timeGroups, 'engagement');
        // Find best performing periods
        const bestPeriods = Object.entries(timeGroups)
            .map(([period, vids]) => ({
                totalViews: vids.reduce((sum, v) => sum + v.view_count, 0),
                videoCount: vids.length
            .sort((a, b) => b.totalViews - a.totalViews)
            .slice(0, 3);
            timeRange: {
                start: startDate || new Date(Math.min(...videos.map(v => v.create_time)) * 1000),
                end: endDate || new Date(Math.max(...videos.map(v => v.create_time)) * 1000)
            totalVideosInPeriod: videos.length,
                views: viewTrend,
                engagement: engagementTrend
            bestPerformingPeriods: bestPeriods,
            postingFrequency: this.calculatePostingFrequency(videos),
            seasonalPatterns: this.identifySeasonalPatterns(timeGroups)
     * Generate content insights
    private generateContentInsights(videos: TikTokVideo[], query?: string, hashtags?: string[]): any {
        // Analyze content themes
        const commonHashtags = this.findCommonHashtags(videos);
        const contentLength = this.analyzeContentLength(videos);
            searchRelevance: {
                query: query || 'all content',
                matchingVideos: videos.length,
                averageRelevanceScore: this.calculateRelevanceScore(videos, query, hashtags)
            contentPatterns: {
                commonHashtags: commonHashtags.slice(0, 10),
                averageDescriptionLength: contentLength.average,
                optimalLength: contentLength.optimal,
                titlePatterns: this.analyzeTitlePatterns(videos)
            performanceCorrelations: {
                hashtagsToViews: this.correlateHashtagsToPerformance(videos),
                lengthToEngagement: contentLength.correlation,
                timeOfDayImpact: this.analyzeTimeOfDayImpact(videos)
     * Group videos by time period
    private groupVideosByTimePeriod(videos: TikTokVideo[]): Record<string, TikTokVideo[]> {
        const groups: Record<string, TikTokVideo[]> = {};
            const date = new Date(video.create_time * 1000);
            const monthKey = `${date.getFullYear()}-${String(date.getMonth() + 1).padStart(2, '0')}`;
            if (!groups[monthKey]) {
                groups[monthKey] = [];
            groups[monthKey].push(video);
     * Calculate performance trend
    private calculateTrend(timeGroups: Record<string, TikTokVideo[]>, metric: string): string {
        const periods = Object.keys(timeGroups).sort();
        if (periods.length < 2) return 'insufficient_data';
        const recentPeriod = periods[periods.length - 1];
        const previousPeriod = periods[periods.length - 2];
        let recentValue: number;
        let previousValue: number;
        if (metric === 'views') {
            recentValue = timeGroups[recentPeriod].reduce((sum, v) => sum + v.view_count, 0);
            previousValue = timeGroups[previousPeriod].reduce((sum, v) => sum + v.view_count, 0);
            recentValue = timeGroups[recentPeriod].reduce((sum, v) => 
            previousValue = timeGroups[previousPeriod].reduce((sum, v) => 
        const changePercent = previousValue > 0 
            ? ((recentValue - previousValue) / previousValue) * 100
            : 100;
        if (changePercent > 20) return 'increasing';
        if (changePercent < -20) return 'decreasing';
     * Calculate posting frequency
    private calculatePostingFrequency(videos: TikTokVideo[]): any {
        if (videos.length < 2) return { average: 0, pattern: 'insufficient_data' };
        const timestamps = videos.map(v => v.create_time).sort();
        const daysBetweenPosts: number[] = [];
        for (let i = 1; i < timestamps.length; i++) {
            const daysDiff = (timestamps[i] - timestamps[i-1]) / (60 * 60 * 24);
            daysBetweenPosts.push(daysDiff);
        const avgDaysBetween = daysBetweenPosts.reduce((sum, days) => sum + days, 0) / daysBetweenPosts.length;
        let pattern: string;
        if (avgDaysBetween < 1) pattern = 'multiple_daily';
        else if (avgDaysBetween <= 1.5) pattern = 'daily';
        else if (avgDaysBetween <= 3.5) pattern = 'every_few_days';
        else if (avgDaysBetween <= 7.5) pattern = 'weekly';
        else pattern = 'irregular';
            averageDaysBetween: avgDaysBetween.toFixed(1),
            postsPerWeek: (7 / avgDaysBetween).toFixed(1)
     * Find common hashtags
    private findCommonHashtags(videos: TikTokVideo[]): any[] {
        const hashtagCount = new Map<string, number>();
                hashtagCount.set(tag, (hashtagCount.get(tag) || 0) + 1);
        return Array.from(hashtagCount.entries())
            .map(([tag, count]) => ({ hashtag: `#${tag}`, count, percentage: (count / videos.length * 100).toFixed(1) }))
            .sort((a, b) => b.count - a.count);
     * Analyze content length
    private analyzeContentLength(videos: TikTokVideo[]): any {
        const lengths = videos.map(v => (v.description || '').length);
        const avgLength = lengths.reduce((sum, len) => sum + len, 0) / lengths.length;
        // Find optimal length (videos with above-average performance)
        const avgViews = videos.reduce((sum, v) => sum + v.view_count, 0) / videos.length;
        const highPerformers = videos.filter(v => v.view_count > avgViews);
        const optimalLength = highPerformers.length > 0
            ? highPerformers.reduce((sum, v) => sum + (v.description || '').length, 0) / highPerformers.length
            : avgLength;
            average: Math.round(avgLength),
            optimal: Math.round(optimalLength),
            correlation: optimalLength > avgLength ? 'longer_performs_better' : 'shorter_performs_better'
     * Calculate relevance score
    private calculateRelevanceScore(videos: TikTokVideo[], query?: string, hashtags?: string[]): number {
        if (!query && (!hashtags || hashtags.length === 0)) return 100;
        let totalScore = 0;
                if (video.title.toLowerCase().includes(queryLower)) score += 50;
                if (video.description.toLowerCase().includes(queryLower)) score += 30;
                const matchingHashtags = hashtags.filter(tag => 
                    videoHashtags.includes(tag.replace('#', '').toLowerCase())
                score += (matchingHashtags.length / hashtags.length) * 20;
            totalScore += score;
        return videos.length > 0 ? Math.round(totalScore / videos.length) : 0;
     * Analyze title patterns
    private analyzeTitlePatterns(videos: TikTokVideo[]): any {
        const patterns = {
            questions: videos.filter(v => v.title.includes('?')).length,
            exclamations: videos.filter(v => v.title.includes('!')).length,
            emojis: videos.filter(v => /[\u{1F600}-\u{1F64F}]/u.test(v.title)).length,
            numbers: videos.filter(v => /\d/.test(v.title)).length,
            allCaps: videos.filter(v => v.title === v.title.toUpperCase() && v.title.length > 3).length
            ...patterns,
            mostCommonPattern: Object.entries(patterns)
                .sort(([, a], [, b]) => b - a)[0][0]
     * Correlate hashtags to performance
    private correlateHashtagsToPerformance(videos: TikTokVideo[]): any[] {
        const hashtagPerformance = new Map<string, { views: number; count: number }>();
                const current = hashtagPerformance.get(tag) || { views: 0, count: 0 };
                current.views += video.view_count;
                current.count += 1;
                hashtagPerformance.set(tag, current);
        return Array.from(hashtagPerformance.entries())
            .map(([tag, data]) => ({
                averageViews: Math.round(data.views / data.count),
                usageCount: data.count
            .sort((a, b) => b.averageViews - a.averageViews)
     * Analyze time of day impact
    private analyzeTimeOfDayImpact(videos: TikTokVideo[]): any {
        const hourBuckets: Record<number, { views: number; count: number }> = {};
            const hour = new Date(video.create_time * 1000).getHours();
            if (!hourBuckets[hour]) {
                hourBuckets[hour] = { views: 0, count: 0 };
            hourBuckets[hour].views += video.view_count;
            hourBuckets[hour].count += 1;
        const hourlyPerformance = Object.entries(hourBuckets)
            .map(([hour, data]) => ({
                postCount: data.count
            .sort((a, b) => b.averageViews - a.averageViews);
            bestHours: hourlyPerformance.slice(0, 3).map(h => h.hour),
            worstHours: hourlyPerformance.slice(-3).map(h => h.hour),
            recommendation: this.generateTimeRecommendation(hourlyPerformance)
     * Generate time posting recommendation
    private generateTimeRecommendation(hourlyPerformance: any[]): string {
        if (hourlyPerformance.length === 0) return 'Insufficient data for recommendations';
        const bestHour = hourlyPerformance[0].hour;
        const timeOfDay = bestHour < 12 ? 'morning' : bestHour < 17 ? 'afternoon' : 'evening';
        return `Best posting time appears to be ${bestHour}:00 (${timeOfDay}) based on historical performance`;
     * Calculate performance distribution
    private calculatePerformanceDistribution(videos: TikTokVideo[]): any {
        const viewBuckets = {
            '0-1k': 0,
            '1k-10k': 0,
            '10k-100k': 0,
            '100k-1M': 0,
            '1M+': 0
            const views = video.view_count;
            if (views < 1000) viewBuckets['0-1k']++;
            else if (views < 10000) viewBuckets['1k-10k']++;
            else if (views < 100000) viewBuckets['10k-100k']++;
            else if (views < 1000000) viewBuckets['100k-1M']++;
            else viewBuckets['1M+']++;
        return viewBuckets;
     * Identify seasonal patterns
    private identifySeasonalPatterns(timeGroups: Record<string, TikTokVideo[]>): string[] {
        const patterns: string[] = [];
        // Simple seasonal analysis
        const monthlyAvgs: Record<number, number[]> = {};
        Object.entries(timeGroups).forEach(([period, videos]) => {
            const month = parseInt(period.split('-')[1]);
            if (!monthlyAvgs[month]) monthlyAvgs[month] = [];
            monthlyAvgs[month].push(avgViews);
        // Find high-performing months
        const monthPerformance = Object.entries(monthlyAvgs)
            .map(([month, avgs]) => ({
                month: parseInt(month),
                avgPerformance: avgs.reduce((sum, avg) => sum + avg, 0) / avgs.length
            .sort((a, b) => b.avgPerformance - a.avgPerformance);
        if (monthPerformance.length > 0) {
            const bestMonth = monthPerformance[0].month;
            patterns.push(`${monthNames[bestMonth - 1]} shows strongest performance historically`);
        return patterns;
                Name: 'MinViews',
                Value: 'date'
        return 'Searches historical TikTok videos with advanced filtering, date ranges, and performance analytics (searches within your own videos only due to API limitations)';
 * Action to search for YouTube videos including historical content
@RegisterClass(BaseAction, 'YouTubeSearchVideosAction')
export class YouTubeSearchVideosAction extends YouTubeBaseAction {
     * Search for YouTube videos
            const videoDuration = this.getParamValue(Params, 'VideoDuration');
            const videoType = this.getParamValue(Params, 'VideoType');
            const orderBy = this.getParamValue(Params, 'OrderBy') || 'relevance';
            const locationRadius = this.getParamValue(Params, 'LocationRadius');
            const safeSearch = this.getParamValue(Params, 'SafeSearch') || 'moderate';
            const videoCaption = this.getParamValue(Params, 'VideoCaption');
            const videoDefinition = this.getParamValue(Params, 'VideoDefinition');
            const videoDimension = this.getParamValue(Params, 'VideoDimension');
            const videoLicense = this.getParamValue(Params, 'VideoLicense');
            const includeHistorical = this.getParamValue(Params, 'IncludeHistorical') ?? true;
                limit: maxResults
            // If searching within own channel, use channel ID from integration
            const actualChannelId = channelId || (includeHistorical ? this.getCustomAttribute(1) : null);
            let allVideos: SocialPost[] = [];
            let nextPageToken = pageToken;
            let totalResults = 0;
            let pagesSearched = 0;
            const maxPages = includeHistorical ? 10 : 1; // Search up to 10 pages for historical data
                const searchResponse = await this.searchYouTubeVideos({
                    ...searchParams,
                    videoDuration,
                    videoType,
                    location,
                    locationRadius,
                    safeSearch,
                    videoCaption,
                    videoDefinition,
                    videoDimension,
                    videoLicense,
                    pageToken: nextPageToken
                }, ContextUser);
                if (searchResponse.videos && searchResponse.videos.length > 0) {
                    allVideos.push(...searchResponse.videos);
                    totalResults = searchResponse.totalResults || allVideos.length;
                nextPageToken = searchResponse.nextPageToken;
                pagesSearched++;
                // Continue searching if we haven't reached the limit and there are more pages
                includeHistorical && 
                nextPageToken && 
                allVideos.length < maxResults && 
                pagesSearched < maxPages
            // Trim to max results
            if (allVideos.length > maxResults) {
                allVideos = allVideos.slice(0, maxResults);
            // Group videos by various criteria for analytics
            const analytics = this.analyzeSearchResults(allVideos);
                videosReturned: allVideos.length,
                pagesSearched: pagesSearched,
                        start: startDate || 'Any',
                        end: endDate || 'Any'
                        duration: videoDuration || 'Any',
                        type: videoType || 'Any',
                        definition: videoDefinition || 'Any',
                        caption: videoCaption || 'Any'
                analytics: analytics,
                nextPageToken: nextPageToken,
                hasMore: !!nextPageToken,
                quotaCost: this.getQuotaCost('search.list') * pagesSearched + 
                          this.getQuotaCost('videos.list') * Math.ceil(allVideos.length / 50)
            if (videosParam) videosParam.Value = allVideos;
            if (nextPageTokenParam) nextPageTokenParam.Value = nextPageToken;
                Message: `Found ${allVideos.length} videos${totalResults > allVideos.length ? ` out of ${totalResults} total results` : ''}`,
                Message: `Failed to search videos: ${errorMessage}`,
     * Search YouTube videos with detailed parameters
    private async searchYouTubeVideos(params: any, contextUser?: any): Promise<any> {
            maxResults: Math.min(params.limit || 50, 50),
            order: params.orderBy || 'relevance'
        // Add channel filter
        if (params.channelId) {
            searchParams.channelId = params.channelId;
        // Add date filters for historical search
        // Add video duration filter
        if (params.videoDuration) {
            searchParams.videoDuration = params.videoDuration; // short, medium, long
        if (params.videoType) {
            searchParams.videoType = params.videoType; // movie, episode
        // Add location-based search
        if (params.location && params.locationRadius) {
            searchParams.location = params.location; // lat,lng
            searchParams.locationRadius = params.locationRadius; // e.g., "10km"
        // Add language filter
            searchParams.relevanceLanguage = params.language;
        // Add safe search
        if (params.safeSearch) {
            searchParams.safeSearch = params.safeSearch; // none, moderate, strict
        // Add caption filter
        if (params.videoCaption) {
            searchParams.videoCaption = params.videoCaption; // closedCaption, none
        // Add video quality filters
        if (params.videoDefinition) {
            searchParams.videoDefinition = params.videoDefinition; // high, standard
        if (params.videoDimension) {
            searchParams.videoDimension = params.videoDimension; // 2d, 3d
        if (params.videoLicense) {
            searchParams.videoLicense = params.videoLicense; // creativeCommon, youtube
        const videos: SocialPost[] = [];
        if (searchResponse.items && searchResponse.items.length > 0) {
            if (videosResponse.items) {
                for (const video of videosResponse.items) {
                    videos.push(this.normalizePost(video));
            videos: videos,
            totalResults: searchResponse.pageInfo?.totalResults,
            prevPageToken: searchResponse.prevPageToken
     * Analyze search results for insights
    private analyzeSearchResults(videos: SocialPost[]): any {
        if (videos.length === 0) {
                videoCount: 0,
                performance: null,
                categories: {},
                channels: {}
        // Sort by date to find range
        const sortedByDate = [...videos].sort((a, b) => 
            a.publishedAt.getTime() - b.publishedAt.getTime()
        const categories: Record<string, number> = {};
        const channels: Record<string, { count: number; name: string }> = {};
            const platformData = video.platformSpecificData;
            const analytics = video.analytics;
                totalViews += analytics.videoViews || 0;
                totalLikes += analytics.likes || 0;
                totalComments += analytics.comments || 0;
            const categoryId = platformData.categoryId;
            categories[categoryId] = (categories[categoryId] || 0) + 1;
            // Count by channel
            const channelId = video.profileId;
            if (!channels[channelId]) {
                channels[channelId] = {
                    name: platformData.channelTitle || channelId
            channels[channelId].count++;
            videoCount: videos.length,
                oldest: sortedByDate[0].publishedAt,
                newest: sortedByDate[sortedByDate.length - 1].publishedAt,
                spanDays: Math.ceil(
                    (sortedByDate[sortedByDate.length - 1].publishedAt.getTime() - 
                     sortedByDate[0].publishedAt.getTime()) / (1000 * 60 * 60 * 24)
                totalViews: totalViews,
                totalLikes: totalLikes,
                totalComments: totalComments,
                averageViews: Math.round(totalViews / videos.length),
                averageLikes: Math.round(totalLikes / videos.length),
                averageComments: Math.round(totalComments / videos.length),
                topVideos: [...videos]
                    .sort((a, b) => (b.analytics?.videoViews || 0) - (a.analytics?.videoViews || 0))
                        title: v.platformSpecificData.title,
                        views: v.analytics?.videoViews || 0,
                        url: v.mediaUrls[0]
            categories: categories,
            channels: channels,
            durationBreakdown: this.analyzeDurations(videos),
            privacyBreakdown: this.analyzePrivacy(videos)
     * Analyze video durations
    private analyzeDurations(videos: SocialPost[]): any {
        const durations = { short: 0, medium: 0, long: 0 };
            const duration = video.platformSpecificData.duration;
            if (duration) {
                const seconds = this.parseDuration(duration);
                if (seconds < 240) durations.short++; // < 4 minutes
                else if (seconds < 1200) durations.medium++; // 4-20 minutes
                else durations.long++; // > 20 minutes
        return durations;
     * Analyze privacy status
    private analyzePrivacy(videos: SocialPost[]): any {
        const privacy = { public: 0, unlisted: 0, private: 0 };
            const status = video.platformSpecificData.privacyStatus || 'public';
            privacy[status]++;
        return privacy;
                Value: 'relevance'
                Name: 'LocationRadius',
                Name: 'SafeSearch',
                Value: 'moderate'
                Name: 'VideoCaption',
                Name: 'VideoDefinition',
                Name: 'VideoDimension',
                Name: 'VideoLicense',
                Name: 'IncludeHistorical',
        return 'Searches YouTube videos with comprehensive filtering options including historical content retrieval with date ranges';
