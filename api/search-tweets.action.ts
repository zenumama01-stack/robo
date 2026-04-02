import { TwitterBaseAction, Tweet, TwitterSearchParams } from '../twitter-base.action';
 * Action to search for tweets on Twitter/X with advanced operators and historical data
@RegisterClass(BaseAction, 'TwitterSearchTweetsAction')
export class TwitterSearchTweetsAction extends TwitterBaseAction {
     * Search for tweets on Twitter
            const fromUser = this.getParamValue(Params, 'FromUser');
            const toUser = this.getParamValue(Params, 'ToUser');
            const mentionUser = this.getParamValue(Params, 'MentionUser');
            const language = this.getParamValue(Params, 'Language');
            const hasMedia = this.getParamValue(Params, 'HasMedia');
            const hasLinks = this.getParamValue(Params, 'HasLinks');
            const isRetweet = this.getParamValue(Params, 'IsRetweet');
            const isReply = this.getParamValue(Params, 'IsReply');
            const isQuote = this.getParamValue(Params, 'IsQuote');
            const isVerified = this.getParamValue(Params, 'IsVerified');
            const minLikes = this.getParamValue(Params, 'MinLikes');
            const minRetweets = this.getParamValue(Params, 'MinRetweets');
            const minReplies = this.getParamValue(Params, 'MinReplies');
            const place = this.getParamValue(Params, 'Place');
            const sortOrder = this.getParamValue(Params, 'SortOrder') || 'recency';
            // Build search query with advanced operators
            const searchQuery = this.buildAdvancedSearchQuery({
                fromUser,
                toUser,
                mentionUser,
                language,
                hasMedia,
                hasLinks,
                isRetweet,
                isReply,
                isQuote,
                isVerified,
                minLikes,
                minRetweets,
                minReplies,
                place
            // Validate query
            if (!searchQuery.trim()) {
                throw new Error('At least one search parameter must be provided');
            // Twitter search query length limit
            if (searchQuery.length > 512) {
                throw new Error(`Search query exceeds Twitter's 512 character limit (current: ${searchQuery.length} characters)`);
            const searchParams: TwitterSearchParams = {
                query: searchQuery,
                max_results: Math.min(maxResults, 100), // API limit per request
                sort_order: sortOrder as 'recency' | 'relevancy'
                searchParams.start_time = this.formatTwitterDate(startDate);
                searchParams.end_time = this.formatTwitterDate(endDate);
            LogStatus(`Searching tweets with query: ${searchQuery.substring(0, 100)}${searchQuery.length > 100 ? '...' : ''}`);
            const tweets = await this.searchTweetsInternal(searchParams, maxResults);
            const analysis = this.analyzeSearchResults(tweets, normalizedPosts);
            const analysisParam = outputParams.find(p => p.Name === 'Analysis');
            if (analysisParam) analysisParam.Value = analysis;
            const actualQueryParam = outputParams.find(p => p.Name === 'ActualQuery');
            if (actualQueryParam) actualQueryParam.Value = searchQuery;
                Message: `Successfully found ${normalizedPosts.length} tweets matching search criteria`,
                Message: `Failed to search tweets: ${errorMessage}`,
     * Build advanced search query with Twitter operators
    private buildAdvancedSearchQuery(params: any): string {
        // Basic query
        if (params.hashtags && Array.isArray(params.hashtags) && params.hashtags.length > 0) {
                .map((tag: string) => tag.startsWith('#') ? tag : `#${tag}`)
        // User filters
        if (params.fromUser) {
            parts.push(`from:${params.fromUser}`);
        if (params.toUser) {
            parts.push(`to:${params.toUser}`);
        if (params.mentionUser) {
            parts.push(`@${params.mentionUser}`);
        if (params.language) {
            parts.push(`lang:${params.language}`);
        // Media and link filters
        if (params.hasMedia === true) {
            parts.push('has:media');
        } else if (params.hasMedia === false) {
            parts.push('-has:media');
        if (params.hasLinks === true) {
            parts.push('has:links');
        } else if (params.hasLinks === false) {
            parts.push('-has:links');
        // Tweet type filters
        if (params.isRetweet === true) {
            parts.push('is:retweet');
        } else if (params.isRetweet === false) {
            parts.push('-is:retweet');
        if (params.isReply === true) {
            parts.push('is:reply');
        } else if (params.isReply === false) {
            parts.push('-is:reply');
        if (params.isQuote === true) {
            parts.push('is:quote');
        } else if (params.isQuote === false) {
            parts.push('-is:quote');
        // Verified filter
        if (params.isVerified === true) {
            parts.push('is:verified');
        } else if (params.isVerified === false) {
            parts.push('-is:verified');
        // Engagement filters (Note: These require Academic Research access)
        if (params.minLikes && params.minLikes > 0) {
            parts.push(`min_faves:${params.minLikes}`);
        if (params.minRetweets && params.minRetweets > 0) {
            parts.push(`min_retweets:${params.minRetweets}`);
        if (params.minReplies && params.minReplies > 0) {
            parts.push(`min_replies:${params.minReplies}`);
        // Place filter
        if (params.place) {
            parts.push(`place:"${params.place}"`);
     * Internal method to search tweets with pagination
    private async searchTweetsInternal(searchParams: TwitterSearchParams, maxResults: number): Promise<Tweet[]> {
        let nextToken: string | undefined;
            'query': searchParams.query,
            'tweet.fields': 'id,text,created_at,author_id,conversation_id,public_metrics,attachments,entities,referenced_tweets,lang,possibly_sensitive',
            'max_results': searchParams.max_results,
            'sort_order': searchParams.sort_order
        // Add time filters
        if (searchParams.start_time) {
            queryParams['start_time'] = searchParams.start_time;
        if (searchParams.end_time) {
            queryParams['end_time'] = searchParams.end_time;
        while (tweets.length < maxResults) {
            if (nextToken) {
                queryParams['next_token'] = nextToken;
                const response = await this.axiosInstance.get('/tweets/search/recent', {
                    params: queryParams
                if (tweets.length >= maxResults) {
                nextToken = response.data.meta?.next_token;
                if (!nextToken) {
                // If we get a 400 error, it might be due to unsupported operators
                if ((error as any).response?.status === 400) {
                    const errorDetail = (error as any).response?.data?.detail || '';
                    if (errorDetail.includes('min_faves') || errorDetail.includes('min_retweets') || errorDetail.includes('min_replies')) {
                        LogStatus('Note: Engagement filters (min_likes, min_retweets, min_replies) require Academic Research access');
                        // Retry without engagement filters
                        const cleanedQuery = searchParams.query
                            .replace(/min_faves:\d+\s*/g, '')
                            .replace(/min_retweets:\d+\s*/g, '')
                            .replace(/min_replies:\d+\s*/g, '')
                        if (cleanedQuery !== searchParams.query) {
                            queryParams['query'] = cleanedQuery;
     * Implement searchPosts for base class
            query: this.buildSearchQuery(params),
            max_results: params.limit || 100
            searchParams.start_time = this.formatTwitterDate(params.startDate);
            searchParams.end_time = this.formatTwitterDate(params.endDate);
        const tweets = await this.searchTweetsInternal(searchParams, params.limit || 100);
        return tweets.map(tweet => this.normalizePost(tweet));
    private analyzeSearchResults(tweets: Tweet[], normalizedPosts: SocialPost[]): any {
            totalResults: tweets.length,
                earliest: null as string | null,
                latest: null as string | null
            languages: {} as Record<string, number>,
            tweetTypes: {
                original: 0,
            topHashtags: [] as Array<{ tag: string; count: number }>,
            topMentions: [] as Array<{ username: string; count: number }>,
                averageEngagement: 0
            topEngagedTweets: [] as any[]
        // Track hashtags and mentions
        const hashtagCounts = new Map<string, number>();
        const mentionCounts = new Map<string, number>();
        tweets.forEach((tweet, index) => {
            const createdAt = tweet.created_at;
            if (!analysis.dateRange.earliest || createdAt < analysis.dateRange.earliest) {
                analysis.dateRange.earliest = createdAt;
            if (!analysis.dateRange.latest || createdAt > analysis.dateRange.latest) {
                analysis.dateRange.latest = createdAt;
            // Language
            const lang = (tweet as any).lang || 'unknown';
            analysis.languages[lang] = (analysis.languages[lang] || 0) + 1;
            // Tweet types
                const types = tweet.referenced_tweets.map(ref => ref.type);
                if (types.includes('replied_to')) analysis.tweetTypes.replies++;
                else if (types.includes('retweeted')) analysis.tweetTypes.retweets++;
                else if (types.includes('quoted')) analysis.tweetTypes.quotes++;
                analysis.tweetTypes.original++;
            if (tweet.entities?.hashtags) {
                tweet.entities.hashtags.forEach(hashtag => {
                    const tag = hashtag.tag.toLowerCase();
                    hashtagCounts.set(tag, (hashtagCounts.get(tag) || 0) + 1);
            // Mentions
            if (tweet.entities?.mentions) {
                tweet.entities.mentions.forEach(mention => {
                    const username = mention.username.toLowerCase();
                    mentionCounts.set(username, (mentionCounts.get(username) || 0) + 1);
            // Engagement stats
                analysis.engagementStats.totalLikes += tweet.public_metrics.like_count || 0;
                analysis.engagementStats.totalRetweets += tweet.public_metrics.retweet_count || 0;
                analysis.engagementStats.totalReplies += tweet.public_metrics.reply_count || 0;
                analysis.engagementStats.totalQuotes += tweet.public_metrics.quote_count || 0;
        if (tweets.length > 0) {
                analysis.engagementStats.totalLikes +
                analysis.engagementStats.totalRetweets +
                analysis.engagementStats.totalReplies +
                analysis.engagementStats.totalQuotes;
            analysis.engagementStats.averageEngagement = Math.round(totalEngagement / tweets.length);
        analysis.topHashtags = Array.from(hashtagCounts.entries())
        // Top mentions
        analysis.topMentions = Array.from(mentionCounts.entries())
        // Top engaged tweets
        analysis.topEngagedTweets = normalizedPosts
                metrics: post.analytics
            if (error.message.includes('character limit')) return 'QUERY_TOO_LONG';
            if (error.message.includes('Academic Research')) return 'INSUFFICIENT_ACCESS';
                Name: 'FromUser',
                Name: 'ToUser',
                Name: 'MentionUser',
                Name: 'HasMedia',
                Name: 'HasLinks',
                Name: 'IsRetweet',
                Name: 'IsReply',
                Name: 'IsQuote',
                Name: 'IsVerified',
                Name: 'MinLikes',
                Name: 'MinRetweets',
                Name: 'MinReplies',
                Name: 'Place',
                Value: 'recency' // 'recency' or 'relevancy'
                Name: 'Analysis',
                Name: 'ActualQuery',
        return 'Searches for tweets on Twitter/X using advanced operators and filters, with comprehensive analysis of results including historical data';
