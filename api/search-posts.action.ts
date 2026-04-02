import { SocialPost } from '../../../base/base-social.action';
 * Action to search historical posts in Buffer across profiles and date ranges
@RegisterClass(BaseAction, 'BufferSearchPostsAction')
export class BufferSearchPostsAction extends BufferBaseAction {
     * Search for posts in Buffer
            const query = this.getParamValue(Params, 'Query');
            const hashtags = this.getParamValue(Params, 'Hashtags');
            const offset = this.getParamValue(Params, 'Offset') || 0;
            const includeAnalytics = this.getParamValue(Params, 'IncludeAnalytics') !== false;
            // Search posts
            const searchParams = {
                hashtags,
                startDate: startDate ? new Date(startDate) : undefined,
                endDate: endDate ? new Date(endDate) : undefined,
                profileIds
            const posts = await this.searchPosts(searchParams);
            // Optionally fetch analytics for each post
            if (includeAnalytics) {
                for (const post of posts) {
                        const analytics = await this.getAnalytics(post.id);
                        post.analytics = this.normalizeAnalytics(analytics);
                        // Log but don't fail if analytics fetch fails
                        console.warn(`Failed to fetch analytics for post ${post.id}:`, error);
            // Create search statistics
            const statistics = this.generateSearchStatistics(posts, searchParams);
                totalResults: posts.length,
                    query: query || null,
                    hashtags: hashtags || [],
                        start: startDate || null,
                        end: endDate || null
                    profileIds: profileIds || [],
                    offset
                statistics,
                topPerformingPost: this.findTopPerformingPost(posts),
                    earliest: posts.length > 0 ? posts[posts.length - 1].publishedAt : null,
                    latest: posts.length > 0 ? posts[0].publishedAt : null
            if (postsParam) postsParam.Value = posts;
                Message: `Found ${posts.length} posts matching search criteria`,
                Message: `Failed to search posts: ${errorMessage}`,
     * Generate statistics from search results
    private generateSearchStatistics(posts: SocialPost[], searchParams: any): any {
            postsByProfile: {} as Record<string, number>,
            postsByMonth: {} as Record<string, number>,
            postsByDayOfWeek: {} as Record<string, number>,
            postsByHour: {} as Record<string, number>,
            averageEngagements: 0,
            totalEngagements: 0,
            postsWithMedia: 0,
            hashtagFrequency: {} as Record<string, number>
        let totalEngagements = 0;
        let postsWithEngagementData = 0;
            // Count by profile
            stats.postsByProfile[post.profileId] = (stats.postsByProfile[post.profileId] || 0) + 1;
            // Count by month
            const month = post.publishedAt.toISOString().substring(0, 7); // YYYY-MM
            stats.postsByMonth[month] = (stats.postsByMonth[month] || 0) + 1;
            // Count by day of week
            const dayOfWeek = post.publishedAt.toLocaleDateString('en-US', { weekday: 'long' });
            stats.postsByDayOfWeek[dayOfWeek] = (stats.postsByDayOfWeek[dayOfWeek] || 0) + 1;
            // Count by hour
            const hour = post.publishedAt.getHours();
            stats.postsByHour[hour] = (stats.postsByHour[hour] || 0) + 1;
            // Count posts with media
            if (post.mediaUrls && post.mediaUrls.length > 0) {
                stats.postsWithMedia++;
            // Calculate engagements
                totalEngagements += post.analytics.engagements;
                postsWithEngagementData++;
            // Extract and count hashtags
            const hashtags = this.extractHashtags(post.content);
            for (const hashtag of hashtags) {
                stats.hashtagFrequency[hashtag] = (stats.hashtagFrequency[hashtag] || 0) + 1;
        stats.totalEngagements = totalEngagements;
        stats.averageEngagements = postsWithEngagementData > 0 ? 
            totalEngagements / postsWithEngagementData : 0;
    private findTopPerformingPost(posts: SocialPost[]): SocialPost | null {
                Name: 'Hashtags',
                Name: 'IncludeAnalytics',
        return 'Searches historical posts in Buffer across profiles with support for date ranges, hashtags, and content queries';
import { FacebookBaseAction, FacebookPost } from '../facebook-base.action';
import { SocialPost, SearchParams, SocialMediaErrorCode } from '../../../base/base-social.action';
 * Searches for historical posts on Facebook pages.
 * Provides powerful search capabilities including date ranges, keywords, and content types.
@RegisterClass(BaseAction, 'FacebookSearchPostsAction')
export class FacebookSearchPostsAction extends FacebookBaseAction {
        return 'Searches for historical posts on Facebook pages with filters for date ranges, keywords, hashtags, and content types';
                Name: 'PageIDs',
                Name: 'PostTypes',
                Name: 'MinEngagements',
                Name: 'IncludeMetrics',
                Value: 'created_time',
                Value: 'DESC',
            const pageIds = this.getParamValue(Params, 'PageIDs') as string[];
            const query = this.getParamValue(Params, 'Query') as string;
            const hashtags = this.getParamValue(Params, 'Hashtags') as string[];
            const postTypes = this.getParamValue(Params, 'PostTypes') as string[];
            const minEngagements = this.getParamValue(Params, 'MinEngagements') as number;
            const includeMetrics = this.getParamValue(Params, 'IncludeMetrics') !== false;
            const sortBy = this.getParamValue(Params, 'SortBy') as string || 'created_time';
            const sortOrder = this.getParamValue(Params, 'SortOrder') as string || 'DESC';
            // Build search parameters
            const searchParams: SearchParams = {
                limit
            LogStatus('Starting Facebook post search...');
            // Get pages to search
            let pagesToSearch = pageIds;
            if (!pagesToSearch || pagesToSearch.length === 0) {
                // Get all accessible pages
                const userPages = await this.getUserPages();
                pagesToSearch = userPages.map(p => p.id);
                LogStatus(`Searching across ${pagesToSearch.length} accessible pages`);
            // Search posts from each page
            const allPosts: FacebookPost[] = [];
            for (const pageId of pagesToSearch) {
                    const posts = await this.searchPagePosts(
                        searchParams,
                        postTypes,
                        includeMetrics
                    allPosts.push(...posts);
                    LogStatus(`Found ${posts.length} posts from page ${pageId}`);
                    LogError(`Failed to search page ${pageId}: ${error}`);
                    // Continue with other pages
            // Filter by minimum engagements if specified
            let filteredPosts = allPosts;
            if (minEngagements && minEngagements > 0) {
                filteredPosts = allPosts.filter(post => {
                    const engagements = this.calculateEngagements(post);
                    return engagements >= minEngagements;
                LogStatus(`Filtered to ${filteredPosts.length} posts with at least ${minEngagements} engagements`);
            // Sort posts
            const sortedPosts = this.sortPosts(filteredPosts, sortBy, sortOrder);
            // Limit results
            const limitedPosts = sortedPosts.slice(0, limit);
            const normalizedPosts = limitedPosts.map(post => this.normalizePost(post));
            // Calculate search summary
            const summary = this.calculateSearchSummary(normalizedPosts, searchParams);
            LogStatus(`Search completed. Found ${normalizedPosts.length} matching posts`);
                Message: `Found ${normalizedPosts.length} posts matching search criteria`,
            LogError(`Failed to search Facebook posts: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Search posts from a specific page
    private async searchPagePosts(
        searchParams: SearchParams,
        postTypes: string[] | null,
        includeMetrics: boolean
    ): Promise<FacebookPost[]> {
        // Build fields parameter
        const fields = includeMetrics 
            ? this.getPostFields() 
            : 'id,message,created_time,updated_time,from,story,permalink_url,attachments';
        // Build API parameters
        const apiParams: any = {
            limit: 100 // Get max per request, we'll filter later
        // Add date range
        if (searchParams.startDate) {
            apiParams.since = Math.floor(searchParams.startDate.getTime() / 1000);
        if (searchParams.endDate) {
            apiParams.until = Math.floor(searchParams.endDate.getTime() / 1000);
        // Get all posts in date range
        const allPosts = await this.getPaginatedResults<FacebookPost>(
            apiParams,
            searchParams.limit ? searchParams.limit * 2 : undefined // Get extra to account for filtering
        // Filter posts based on search criteria
        // Filter by content/query
        if (searchParams.query) {
            const queryLower = searchParams.query.toLowerCase();
            filteredPosts = filteredPosts.filter(post => {
                const message = (post.message || '').toLowerCase();
                const story = (post.story || '').toLowerCase();
                return message.includes(queryLower) || story.includes(queryLower);
        // Filter by hashtags
        if (searchParams.hashtags && searchParams.hashtags.length > 0) {
            const hashtagsToFind = searchParams.hashtags.map(tag => 
                tag.startsWith('#') ? tag.toLowerCase() : `#${tag}`.toLowerCase()
                const content = (post.message || '').toLowerCase();
                return hashtagsToFind.some(hashtag => content.includes(hashtag));
        // Filter by post types
        if (postTypes && postTypes.length > 0) {
                const postType = this.getPostType(post);
                return postTypes.includes(postType);
        return filteredPosts;
     * Implement the abstract searchPosts method
        // This is called by the base class, but we implement our own search logic
        // in RunAction, so we'll just throw an error here
        throw new Error('Use FacebookSearchPostsAction.RunAction instead');
     * Get post type from Facebook post
    private getPostType(post: FacebookPost): string {
            return post.attachments.data[0].type.toLowerCase();
        return 'status';
     * Calculate total engagements for a post
    private calculateEngagements(post: FacebookPost): number {
        const reactions = post.reactions?.summary?.total_count || 0;
        const comments = post.comments?.summary?.total_count || 0;
        const shares = post.shares?.count || 0;
        return reactions + comments + shares;
     * Sort posts by specified criteria
    private sortPosts(posts: FacebookPost[], sortBy: string, sortOrder: string): FacebookPost[] {
        const sorted = [...posts].sort((a, b) => {
            let aValue: number;
            let bValue: number;
                case 'engagement':
                    aValue = this.calculateEngagements(a);
                    bValue = this.calculateEngagements(b);
                case 'reach':
                    aValue = this.getPostReach(a);
                    bValue = this.getPostReach(b);
                case 'created_time':
                    aValue = new Date(a.created_time).getTime();
                    bValue = new Date(b.created_time).getTime();
            return sortOrder === 'DESC' ? bValue - aValue : aValue - bValue;
     * Get post reach from insights
    private getPostReach(post: FacebookPost): number {
        if (post.insights?.data) {
            const reachInsight = post.insights.data.find(i => 
                i.name === 'post_impressions_unique' || i.name === 'post_reach'
            return reachInsight?.values?.[0]?.value || 0;
     * Calculate search summary statistics
    private calculateSearchSummary(posts: SocialPost[], searchParams: SearchParams): any {
        if (posts.length === 0) {
                totalPosts: 0,
                dateRange: null,
                topHashtags: [],
                postTypes: {},
                engagementStats: null
        // Date range
        const dates = posts.map(p => p.publishedAt.getTime());
        const dateRange = {
            earliest: new Date(Math.min(...dates)),
            latest: new Date(Math.max(...dates))
        // Extract hashtags
        const hashtagCounts: Record<string, number> = {};
        posts.forEach(post => {
            const hashtags = post.content.match(/#\w+/g) || [];
            hashtags.forEach(tag => {
                hashtagCounts[tag] = (hashtagCounts[tag] || 0) + 1;
        // Top hashtags
        const topHashtags = Object.entries(hashtagCounts)
            .sort(([, a], [, b]) => b - a)
            .map(([tag, count]) => ({ tag, count }));
        // Post types
        const postTypes: Record<string, number> = {};
            const type = post.platformSpecificData.postType || 'status';
            postTypes[type] = (postTypes[type] || 0) + 1;
        // Engagement statistics
        const engagements = posts.map(p => p.analytics?.engagements || 0);
        const totalEngagements = engagements.reduce((sum, e) => sum + e, 0);
        const avgEngagements = totalEngagements / posts.length;
        const maxEngagements = Math.max(...engagements);
            totalPosts: posts.length,
            dateRange,
            topHashtags,
            engagementStats: {
                total: totalEngagements,
                average: Math.round(avgEngagements),
                max: maxEngagements,
                distribution: this.getEngagementDistribution(engagements)
     * Get engagement distribution
    private getEngagementDistribution(engagements: number[]): Record<string, number> {
        const distribution = {
            '0-10': 0,
            '11-50': 0,
            '51-100': 0,
            '101-500': 0,
            '501-1000': 0,
            '1000+': 0
        engagements.forEach(e => {
            if (e <= 10) distribution['0-10']++;
            else if (e <= 50) distribution['11-50']++;
            else if (e <= 100) distribution['51-100']++;
            else if (e <= 500) distribution['101-500']++;
            else if (e <= 1000) distribution['501-1000']++;
            else distribution['1000+']++;
        return distribution;
import { SearchParams, SocialPost } from '../../../base/base-social.action';
 * Action to search historical posts in HootSuite
@RegisterClass(BaseAction, 'HootSuiteSearchPostsAction')
export class HootSuiteSearchPostsAction extends HootSuiteBaseAction {
     * Search for posts in HootSuite
            const postState = this.getParamValue(Params, 'PostState') || 'PUBLISHED';
            const sortBy = this.getParamValue(Params, 'SortBy') || 'publishedTime';
                query: query,
                hashtags: hashtags,
                limit: limit
            // Perform the search
            LogStatus('Searching for posts...');
            // Filter by profile if specified
            let filteredPosts = posts;
                filteredPosts = posts.filter(p => 
                    p.profileId.includes(profileId) || 
                    (p.platformSpecificData?.socialProfileIds?.includes(profileId))
            // Sort results
            filteredPosts = this.sortPosts(filteredPosts, sortBy, sortOrder);
            if (limit && filteredPosts.length > limit) {
                filteredPosts = filteredPosts.slice(0, limit);
                LogStatus('Fetching analytics for posts...');
                filteredPosts = await this.enrichPostsWithAnalytics(filteredPosts);
            // Analyze content patterns
            const contentAnalysis = this.analyzePostContent(filteredPosts);
                totalResults: filteredPosts.length,
                    end: endDate || 'Not specified',
                    actualStart: filteredPosts.length > 0 
                        ? filteredPosts[filteredPosts.length - 1].publishedAt.toISOString() 
                    actualEnd: filteredPosts.length > 0 
                        ? filteredPosts[0].publishedAt.toISOString() 
                byProfile: this.groupByProfile(filteredPosts),
                byMonth: this.groupByMonth(filteredPosts),
                contentAnalysis: contentAnalysis,
                topHashtags: this.extractTopHashtags(filteredPosts),
                performanceStats: includeAnalytics ? this.calculatePerformanceStats(filteredPosts) : null
            if (postsParam) postsParam.Value = filteredPosts;
                Message: `Found ${filteredPosts.length} posts matching search criteria`,
     * Search for posts implementation
            limit: Math.min(params.limit || 100, 100),
            maxResults: params.limit
        // Add state filter - default to published for historical search
        queryParams.state = 'PUBLISHED';
        // Add date filters
        if (params.startDate) {
            queryParams.publishedAfter = this.formatHootSuiteDate(params.startDate);
        if (params.endDate) {
            queryParams.publishedBefore = this.formatHootSuiteDate(params.endDate);
        // Add text search
            queryParams.text = params.query;
        // Add hashtag search
            // HootSuite API may require hashtags in the text query
            const hashtagQuery = params.hashtags.map(tag => 
                tag.startsWith('#') ? tag : `#${tag}`
            ).join(' ');
            queryParams.text = params.query 
                ? `${params.query} ${hashtagQuery}`
                : hashtagQuery;
        // Make paginated request
        const hootSuitePosts = await this.makePaginatedRequest<HootSuitePost>('/messages', queryParams);
        return hootSuitePosts.map(post => this.normalizePost(post));
     * Enrich posts with analytics data
    private async enrichPostsWithAnalytics(posts: SocialPost[]): Promise<SocialPost[]> {
        const enrichedPosts: SocialPost[] = [];
                const response = await this.axiosInstance.get(`/analytics/posts/${post.id}`);
                const analytics = response.data;
                enrichedPosts.push({
                    ...post,
                    analytics: this.normalizeAnalytics(analytics.metrics)
                // If analytics fail, include post without analytics
                LogStatus(`Could not get analytics for post ${post.id}`);
                enrichedPosts.push(post);
        return enrichedPosts;
    private sortPosts(posts: SocialPost[], sortBy: string, sortOrder: string): SocialPost[] {
        return posts.sort((a, b) => {
            let compareValue = 0;
                case 'publishedTime':
                    compareValue = a.publishedAt.getTime() - b.publishedAt.getTime();
                case 'engagements':
                    compareValue = (a.analytics?.engagements || 0) - (b.analytics?.engagements || 0);
                case 'impressions':
                    compareValue = (a.analytics?.impressions || 0) - (b.analytics?.impressions || 0);
            return sortOrder === 'DESC' ? -compareValue : compareValue;
     * Analyze post content patterns
    private analyzePostContent(posts: SocialPost[]): any {
        const totalPosts = posts.length;
        if (totalPosts === 0) return null;
        const contentLengths = posts.map(p => p.content.length);
        const avgLength = contentLengths.reduce((a, b) => a + b, 0) / totalPosts;
        const withMedia = posts.filter(p => p.mediaUrls && p.mediaUrls.length > 0).length;
        const withHashtags = posts.filter(p => p.content.includes('#')).length;
        const withLinks = posts.filter(p => 
            p.content.includes('http://') || p.content.includes('https://')
            averageLength: Math.round(avgLength),
            minLength: Math.min(...contentLengths),
            maxLength: Math.max(...contentLengths),
            withMedia: withMedia,
            withMediaPercentage: (withMedia / totalPosts) * 100,
            withHashtags: withHashtags,
            withHashtagsPercentage: (withHashtags / totalPosts) * 100,
            withLinks: withLinks,
            withLinksPercentage: (withLinks / totalPosts) * 100
     * Extract top hashtags from posts
    private extractTopHashtags(posts: SocialPost[]): Array<{ hashtag: string; count: number }> {
                const normalized = tag.toLowerCase();
                hashtagCounts[normalized] = (hashtagCounts[normalized] || 0) + 1;
        return Object.entries(hashtagCounts)
            .map(([hashtag, count]) => ({ hashtag, count }))
     * Calculate performance statistics
    private calculatePerformanceStats(posts: SocialPost[]): any {
        const postsWithAnalytics = posts.filter(p => p.analytics);
        if (postsWithAnalytics.length === 0) return null;
        const totalEngagements = postsWithAnalytics.reduce((sum, p) => 
            sum + (p.analytics?.engagements || 0), 0
        const totalImpressions = postsWithAnalytics.reduce((sum, p) => 
            sum + (p.analytics?.impressions || 0), 0
        const avgEngagements = totalEngagements / postsWithAnalytics.length;
        const avgImpressions = totalImpressions / postsWithAnalytics.length;
        const engagementRate = totalImpressions > 0 
            ? (totalEngagements / totalImpressions) * 100 
            postsAnalyzed: postsWithAnalytics.length,
            averageEngagements: Math.round(avgEngagements),
            averageImpressions: Math.round(avgImpressions),
            totalEngagements: totalEngagements,
            totalImpressions: totalImpressions,
            engagementRate: engagementRate.toFixed(2)
    private groupByProfile(posts: SocialPost[]): Record<string, number> {
            const profiles = post.platformSpecificData?.socialProfileIds || [post.profileId];
            profiles.forEach((profileId: string) => {
     * Group posts by month
    private groupByMonth(posts: SocialPost[]): Record<string, number> {
            const monthKey = post.publishedAt.toISOString().substring(0, 7);
            groups[monthKey] = (groups[monthKey] || 0) + 1;
                Name: 'PostState',
        return 'Searches historical posts in HootSuite with support for text queries, hashtags, date ranges, and content analysis';
import { SocialPost, SearchParams } from '../../../base/base-social.action';
 * Searches for historical Instagram posts from the business account.
 * Instagram API only allows searching within your own business account's posts.
 * Supports filtering by date range, hashtags, and content.
@RegisterClass(BaseAction, 'Instagram - Search Posts')
export class InstagramSearchPostsAction extends InstagramBaseAction {
            const minEngagement = this.getParamValue(params.Params, 'MinEngagement') || 0;
            const includeArchived = this.getParamValue(params.Params, 'IncludeArchived') || false;
                filteredPosts = posts.filter(post => 
                    post.platformSpecificData.mediaType === mediaType
            // Filter by minimum engagement
            if (minEngagement > 0) {
                    const totalEngagement = 
                        (post.analytics?.likes || 0) + 
                        (post.analytics?.comments || 0);
                    return totalEngagement >= minEngagement;
            // Include archived posts if requested
            if (includeArchived) {
                const archivedPosts = await this.getArchivedPosts(searchParams);
                filteredPosts = [...filteredPosts, ...archivedPosts];
            // Sort posts by relevance
            const sortedPosts = this.sortByRelevance(filteredPosts, query, hashtags);
            // Analyze search results
            const analysis = this.analyzeSearchResults(sortedPosts, searchParams);
                    posts: sortedPosts.slice(0, limit),
                    totalFound: sortedPosts.length,
                            start: startDate,
                            end: endDate
                        mediaType,
                        minEngagement
                    suggestions: this.generateSearchSuggestions(sortedPosts, searchParams)
                Message: `Found ${sortedPosts.length} matching posts`,
            LogError('Failed to search Instagram posts', error);
                Message: `Failed to search posts: ${error.message}`,
     * Search posts implementation (Instagram only allows searching own posts)
        // Instagram doesn't have a public search API, so we fetch all posts
        // and filter them client-side
        const allPosts = await this.fetchAllAccountPosts(params.startDate, params.endDate);
        let filtered = allPosts;
        // Filter by query in caption
            const queryLower = params.query.toLowerCase();
            filtered = filtered.filter(post => 
                post.content.toLowerCase().includes(queryLower)
            const searchHashtags = params.hashtags.map(tag => 
            filtered = filtered.filter(post => {
                return searchHashtags.some(searchTag => 
                    postHashtags.includes(searchTag)
        return filtered.slice(0, params.limit || 100);
     * Fetch all posts from the account within date range
    private async fetchAllAccountPosts(startDate?: Date, endDate?: Date): Promise<SocialPost[]> {
        let afterCursor: string | undefined;
        while (hasNext) {
                fields: 'id,caption,media_type,media_url,permalink,timestamp,like_count,comments_count',
                queryParams.since = Math.floor(startDate.getTime() / 1000);
                queryParams.until = Math.floor(endDate.getTime() / 1000);
                    cursors: { after: string };
                const normalizedPosts = response.data.map(post => this.normalizePost(post));
                posts.push(...normalizedPosts);
            if (response.paging?.next && response.paging?.cursors?.after) {
                afterCursor = response.paging.cursors.after;
            // Stop if we've reached the date range limit
            if (posts.length > 0 && startDate) {
                const oldestPost = posts[posts.length - 1];
                if (oldestPost.publishedAt < startDate) {
        return posts;
     * Get archived posts (if any)
    private async getArchivedPosts(params: SearchParams): Promise<SocialPost[]> {
            // Instagram API doesn't have a specific endpoint for archived posts
            // This would require additional implementation or different API access
            LogError('Failed to get archived posts', error);
     * Extract hashtags from content
    private extractHashtags(content: string): string[] {
        const hashtagRegex = /#[\w\u0590-\u05ff]+/g;
        const matches = content.match(hashtagRegex) || [];
        return matches.map(tag => tag.toLowerCase());
     * Sort posts by relevance
    private sortByRelevance(
        posts: SocialPost[], 
        query?: string, 
        hashtags?: string[]
    ): SocialPost[] {
            let scoreA = 0;
            let scoreB = 0;
            // Score based on query match position
            if (query) {
                const queryLower = query.toLowerCase();
                const posA = a.content.toLowerCase().indexOf(queryLower);
                const posB = b.content.toLowerCase().indexOf(queryLower);
                if (posA === 0) scoreA += 10; // Starts with query
                else if (posA > 0) scoreA += 5; // Contains query
                if (posB === 0) scoreB += 10;
                else if (posB > 0) scoreB += 5;
            // Score based on hashtag matches
                const hashtagsA = this.extractHashtags(a.content);
                const hashtagsB = this.extractHashtags(b.content);
                    const searchTag = tag.startsWith('#') ? tag.toLowerCase() : `#${tag}`.toLowerCase();
                    if (hashtagsA.includes(searchTag)) scoreA += 3;
                    if (hashtagsB.includes(searchTag)) scoreB += 3;
            // Score based on engagement
            const engagementA = (a.analytics?.likes || 0) + (a.analytics?.comments || 0);
            const engagementB = (b.analytics?.likes || 0) + (b.analytics?.comments || 0);
            scoreA += Math.log10(engagementA + 1);
            scoreB += Math.log10(engagementB + 1);
            // Sort by score (descending) then by date (descending)
            if (scoreA !== scoreB) {
            return b.publishedAt.getTime() - a.publishedAt.getTime();
     * Analyze search results
    private analyzeSearchResults(posts: SocialPost[], params: SearchParams): any {
                earliest: null as Date | null,
                latest: null as Date | null
            mediaTypes: {
                totalLikes: 0,
                totalComments: 0,
                avgLikesPerPost: 0,
                avgCommentsPerPost: 0,
                topPost: null as any
            hashtagFrequency: {} as Record<string, number>,
            postingPatterns: {
                byDayOfWeek: {} as Record<string, number>,
                byHour: {} as Record<number, number>
        if (posts.length === 0) return analysis;
        // Find date range
        analysis.dateRange.earliest = new Date(Math.min(...dates));
        analysis.dateRange.latest = new Date(Math.max(...dates));
        // Analyze posts
        let topEngagement = 0;
            // Media types
            const mediaType = post.platformSpecificData.mediaType;
            if (analysis.mediaTypes[mediaType] !== undefined) {
                analysis.mediaTypes[mediaType]++;
            const likes = post.analytics?.likes || 0;
            const comments = post.analytics?.comments || 0;
            const totalEngagement = likes + comments;
            analysis.engagement.totalLikes += likes;
            analysis.engagement.totalComments += comments;
            if (totalEngagement > topEngagement) {
                topEngagement = totalEngagement;
                analysis.engagement.topPost = {
                    content: post.content.substring(0, 100) + '...',
                    engagement: totalEngagement,
                    publishedAt: post.publishedAt
            // Hashtags
                analysis.hashtagFrequency[tag] = (analysis.hashtagFrequency[tag] || 0) + 1;
            // Posting patterns
            analysis.postingPatterns.byDayOfWeek[dayOfWeek] = 
                (analysis.postingPatterns.byDayOfWeek[dayOfWeek] || 0) + 1;
            analysis.postingPatterns.byHour[hour] = 
                (analysis.postingPatterns.byHour[hour] || 0) + 1;
        analysis.engagement.avgLikesPerPost = 
            Math.round(analysis.engagement.totalLikes / posts.length);
        analysis.engagement.avgCommentsPerPost = 
            Math.round(analysis.engagement.totalComments / posts.length);
     * Generate search suggestions based on results
    private generateSearchSuggestions(posts: SocialPost[], params: SearchParams): any {
        const suggestions = {
            relatedHashtags: [] as string[],
            optimalPostingTimes: [] as any[],
            contentThemes: [] as string[],
            performanceInsights: [] as string[]
            suggestions.performanceInsights.push('No posts found matching your criteria. Try broadening your search.');
        // Related hashtags (most frequently used)
                if (!params.hashtags?.includes(tag)) {
        suggestions.relatedHashtags = Object.entries(hashtagCounts)
            .map(([tag]) => tag);
        // Optimal posting times (based on engagement)
        const timeEngagement: Record<string, { total: number; count: number }> = {};
            const day = post.publishedAt.toLocaleDateString('en-US', { weekday: 'long' });
            const key = `${day} ${hour}:00`;
            if (!timeEngagement[key]) {
                timeEngagement[key] = { total: 0, count: 0 };
            const engagement = (post.analytics?.likes || 0) + (post.analytics?.comments || 0);
            timeEngagement[key].total += engagement;
            timeEngagement[key].count++;
        suggestions.optimalPostingTimes = Object.entries(timeEngagement)
            .map(([time, data]) => ({
                time,
                avgEngagement: Math.round(data.total / data.count)
            .sort((a, b) => b.avgEngagement - a.avgEngagement)
        // Performance insights
        const avgEngagement = posts.reduce((sum, post) => 
            sum + (post.analytics?.likes || 0) + (post.analytics?.comments || 0), 0
        ) / posts.length;
        suggestions.performanceInsights.push(
            `Average engagement per post: ${Math.round(avgEngagement)}`,
            `Most successful media type: ${this.getMostSuccessfulMediaType(posts)}`,
            `Posts with questions get ${this.getQuestionEngagementBoost(posts)}% more engagement`
     * Get most successful media type
    private getMostSuccessfulMediaType(posts: SocialPost[]): string {
        const typeEngagement: Record<string, { total: number; count: number }> = {};
            const type = post.platformSpecificData.mediaType;
            if (!typeEngagement[type]) {
                typeEngagement[type] = { total: 0, count: 0 };
            typeEngagement[type].total += engagement;
            typeEngagement[type].count++;
        let bestType = 'IMAGE';
        let bestAvg = 0;
        Object.entries(typeEngagement).forEach(([type, data]) => {
            const avg = data.total / data.count;
            if (avg > bestAvg) {
                bestAvg = avg;
                bestType = type;
        return bestType;
     * Calculate engagement boost for posts with questions
    private getQuestionEngagementBoost(posts: SocialPost[]): number {
        const withQuestions = posts.filter(p => p.content.includes('?'));
        const withoutQuestions = posts.filter(p => !p.content.includes('?'));
        if (withQuestions.length === 0 || withoutQuestions.length === 0) {
        const avgWithQuestions = withQuestions.reduce((sum, post) => 
        ) / withQuestions.length;
        const avgWithoutQuestions = withoutQuestions.reduce((sum, post) => 
        ) / withoutQuestions.length;
        if (avgWithoutQuestions === 0) return 0;
        return Math.round(((avgWithQuestions - avgWithoutQuestions) / avgWithoutQuestions) * 100);
                Name: 'MinEngagement',
        return 'Searches historical Instagram posts from your business account with filters for date range, hashtags, content, and engagement metrics.';
import { LinkedInBaseAction, LinkedInShare } from '../linkedin-base.action';
 * Action to search for historical posts on LinkedIn
@RegisterClass(BaseAction, 'LinkedInSearchPostsAction')
export class LinkedInSearchPostsAction extends LinkedInBaseAction {
     * Search for posts on LinkedIn with various filters
            const authorType = this.getParamValue(Params, 'AuthorType') || 'all'; // 'personal', 'organization', 'all'
                limit: limit,
                offset: offset
            const posts = await this.searchPostsImplementation(
                authorType,
                organizationId,
                includeAnalytics
            LogStatus(`Found ${posts.length} posts matching search criteria`);
                Message: `Successfully found ${posts.length} posts`,
     * Implementation of post search
    private async searchPostsImplementation(
        params: SearchParams,
        authorType: string,
        organizationId?: string,
        includeAnalytics: boolean = false
    ): Promise<SocialPost[]> {
        const allPosts: SocialPost[] = [];
        const authorsToSearch: string[] = [];
        // Determine which authors to search
        if (authorType === 'personal' || authorType === 'all') {
            authorsToSearch.push(userUrn);
        if (authorType === 'organization' || authorType === 'all') {
            if (organizationId) {
                authorsToSearch.push(`urn:li:organization:${organizationId}`);
                // Get all admin organizations
                authorsToSearch.push(...orgs.map(org => org.urn));
        // Search posts for each author
        for (const authorUrn of authorsToSearch) {
            LogStatus(`Searching posts for author: ${authorUrn}`);
            // Get all posts for the author (LinkedIn doesn't support direct content search)
            const shares = await this.getAllSharesForAuthor(authorUrn, params.limit || 100);
                if (this.matchesSearchCriteria(share, params)) {
                    if (includeAnalytics && authorUrn.includes('organization')) {
                            const analytics = await this.getPostAnalyticsForSearch(share.id, authorUrn);
                    allPosts.push(post);
        // Sort by date (newest first)
        allPosts.sort((a, b) => b.publishedAt.getTime() - a.publishedAt.getTime());
        // Apply offset and limit
        const startIndex = params.offset || 0;
        const endIndex = startIndex + (params.limit || 100);
        return allPosts.slice(startIndex, endIndex);
     * Get all shares for an author with pagination
    private async getAllSharesForAuthor(authorUrn: string, maxResults: number): Promise<LinkedInShare[]> {
        const allShares: LinkedInShare[] = [];
        let start = 0;
        const count = 50; // LinkedIn's typical page size
        while (allShares.length < maxResults) {
                const shares = await this.getShares(authorUrn, count, start);
                if (shares.length === 0) {
                    break; // No more results
                allShares.push(...shares);
                start += count;
                // Check if we've hit the API limit
                if (shares.length < count) {
                    break; // Last page
                LogError(`Error fetching shares at offset ${start}: ${error instanceof Error ? error.message : 'Unknown error'}`);
        return allShares.slice(0, maxResults);
     * Check if a share matches the search criteria
    private matchesSearchCriteria(share: LinkedInShare, params: SearchParams): boolean {
        const content = share.specificContent?.['com.linkedin.ugc.ShareContent']?.shareCommentary?.text || '';
        const publishedDate = new Date(share.firstPublishedAt || share.created.time);
        if (params.startDate && publishedDate < params.startDate) {
        if (params.endDate && publishedDate > params.endDate) {
        // Check query (case-insensitive)
            if (!content.toLowerCase().includes(query)) {
            const contentLower = content.toLowerCase();
            const hasAllHashtags = params.hashtags.every(hashtag => {
                const normalizedHashtag = hashtag.toLowerCase().startsWith('#') 
                    ? hashtag.toLowerCase() 
                    : `#${hashtag.toLowerCase()}`;
                return contentLower.includes(normalizedHashtag);
            if (!hasAllHashtags) {
     * Get analytics for a post during search
    private async getPostAnalyticsForSearch(shareId: string, organizationUrn: string): Promise<any> {
            // Silently fail for individual analytics requests during search
     * Override base searchPosts method
        return this.searchPostsImplementation(params, 'all');
                Value: null // Array of hashtags
                Value: 'all' // 'personal', 'organization', 'all'
        return 'Searches for historical LinkedIn posts with support for content search, hashtags, date ranges, and author filtering. Retrieves posts from personal profiles and organization pages.';
