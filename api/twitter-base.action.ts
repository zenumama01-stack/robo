 * Base class for all Twitter/X actions.
 * Handles Twitter-specific authentication, API interactions, and rate limiting.
 * Uses Twitter API v2 with OAuth 2.0.
@RegisterClass(BaseAction, 'TwitterBaseAction')
export abstract class TwitterBaseAction extends BaseSocialMediaAction {
        return 'Twitter';
        return 'https://api.twitter.com/2';
     * Upload endpoint for media
    protected get uploadApiUrl(): string {
        return 'https://upload.twitter.com/1.1';
                        LogStatus(`Twitter Rate Limit - Remaining: ${rateLimitInfo.remaining}/${rateLimitInfo.limit}, Reset: ${rateLimitInfo.reset}`);
                        const resetTime = error.response.headers['x-rate-limit-reset'];
                        const waitTime = resetTime 
                            ? Math.max(0, parseInt(resetTime) - Math.floor(Date.now() / 1000))
                            : 60;
            throw new Error('No refresh token available for Twitter');
            const clientId = this.getCustomAttribute(2) || ''; // Client ID stored in CustomAttribute2
            const clientSecret = this.getCustomAttribute(3) || ''; // Client Secret stored in CustomAttribute3
            const basicAuth = Buffer.from(`${clientId}:${clientSecret}`).toString('base64');
            const response = await axios.post('https://api.twitter.com/2/oauth2/token', 
                    client_id: clientId
                        'Authorization': `Basic ${basicAuth}`
            LogStatus('Twitter access token refreshed successfully');
            LogError(`Failed to refresh Twitter access token: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get the authenticated user's info
    protected async getCurrentUser(): Promise<TwitterUser> {
            const response = await this.axiosInstance.get('/users/me', {
                    'user.fields': 'id,name,username,profile_image_url,description,created_at,verified'
            LogError(`Failed to get current user: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Upload media to Twitter
            // Step 1: Initialize upload
            const initResponse = await axios.post(
                `${this.uploadApiUrl}/media/upload.json`,
                    command: 'INIT',
                    total_bytes: fileData.length.toString(),
                    media_type: file.mimeType,
                    media_category: this.getMediaCategory(file.mimeType)
            const mediaId = initResponse.data.media_id_string;
            // Step 2: Upload chunks (for large files, Twitter requires chunking)
            const chunkSize = 5 * 1024 * 1024; // 5MB chunks
            let segmentIndex = 0;
            for (let offset = 0; offset < fileData.length; offset += chunkSize) {
                const chunk = fileData.slice(offset, Math.min(offset + chunkSize, fileData.length));
                formData.append('command', 'APPEND');
                formData.append('media_id', mediaId);
                formData.append('segment_index', segmentIndex.toString());
                formData.append('media', chunk, {
                segmentIndex++;
            // Step 3: Finalize upload
                    command: 'FINALIZE',
                    media_id: mediaId
            // Step 4: Check processing status (for videos)
            if (file.mimeType.startsWith('video/')) {
            LogError(`Failed to upload media to Twitter: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Wait for media processing to complete (for videos)
    private async waitForMediaProcessing(mediaId: string, maxWaitTime: number = 60000): Promise<void> {
                        command: 'STATUS',
                        'Authorization': `Bearer ${this.getAccessToken()}`
            const { processing_info } = response.data;
            if (!processing_info) {
                // Processing complete
            if (processing_info.state === 'succeeded') {
            if (processing_info.state === 'failed') {
                throw new Error(`Media processing failed: ${processing_info.error?.message || 'Unknown error'}`);
            // Wait before checking again
            const checkAfterSecs = processing_info.check_after_secs || 1;
            await new Promise(resolve => setTimeout(resolve, checkAfterSecs * 1000));
        throw new Error('Media processing timeout');
     * Get media category based on MIME type
    private getMediaCategory(mimeType: string): string {
        if (mimeType.startsWith('image/gif')) {
            return 'tweet_gif';
        } else if (mimeType.startsWith('image/')) {
            return 'tweet_image';
        } else if (mimeType.startsWith('video/')) {
            return 'tweet_video';
     * Validate media file meets Twitter requirements
            'image/webp',
            'video/mp4'
        // Twitter media size limits
        if (file.mimeType === 'image/gif') {
            maxSize = 15 * 1024 * 1024; // 15MB for GIFs
        } else if (file.mimeType.startsWith('image/')) {
            maxSize = 5 * 1024 * 1024; // 5MB for images
            maxSize = 512 * 1024 * 1024; // 512MB for videos
            maxSize = 5 * 1024 * 1024; // Default 5MB
     * Create a tweet
    protected async createTweet(tweetData: CreateTweetData): Promise<Tweet> {
            const response = await this.axiosInstance.post('/tweets', tweetData);
            this.handleTwitterError(error as AxiosError);
     * Delete a tweet
    protected async deleteTweet(tweetId: string): Promise<void> {
            await this.axiosInstance.delete(`/tweets/${tweetId}`);
     * Get tweets with specified parameters
    protected async getTweets(endpoint: string, params: Record<string, any> = {}): Promise<Tweet[]> {
            const defaultParams = {
                'tweet.fields': 'id,text,created_at,author_id,conversation_id,public_metrics,attachments,entities,referenced_tweets',
                'user.fields': 'id,name,username,profile_image_url',
                'media.fields': 'url,preview_image_url,type,width,height',
                'expansions': 'author_id,attachments.media_keys,referenced_tweets.id',
                'max_results': 100
            const response = await this.axiosInstance.get(endpoint, {
                params: { ...defaultParams, ...params }
            LogError(`Failed to get tweets: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get paginated tweets
    protected async getPaginatedTweets(endpoint: string, params: Record<string, any> = {}, maxResults?: number): Promise<Tweet[]> {
        const tweets: Tweet[] = [];
        let paginationToken: string | undefined;
        const limit = params.max_results || 100;
                    max_results: limit,
                    ...(paginationToken && { pagination_token: paginationToken })
                tweets.push(...response.data.data);
            if (maxResults && tweets.length >= maxResults) {
                return tweets.slice(0, maxResults);
            paginationToken = response.data.meta?.next_token;
            if (!paginationToken) {
        return tweets;
     * Convert Twitter tweet to common format
    protected normalizePost(tweet: Tweet): SocialPost {
            id: tweet.id,
            platform: 'Twitter',
            profileId: tweet.author_id || '',
            content: tweet.text,
            mediaUrls: tweet.attachments?.media_keys || [],
            publishedAt: new Date(tweet.created_at),
            analytics: tweet.public_metrics ? {
                impressions: tweet.public_metrics.impression_count || 0,
                engagements: (tweet.public_metrics.retweet_count || 0) + 
                            (tweet.public_metrics.reply_count || 0) + 
                            (tweet.public_metrics.like_count || 0) +
                            (tweet.public_metrics.quote_count || 0),
                clicks: 0, // Not available in public metrics
                shares: tweet.public_metrics.retweet_count || 0,
                comments: tweet.public_metrics.reply_count || 0,
                likes: tweet.public_metrics.like_count || 0,
                reach: tweet.public_metrics.impression_count || 0,
                platformMetrics: tweet.public_metrics
                conversationId: tweet.conversation_id,
                referencedTweets: tweet.referenced_tweets,
                entities: tweet.entities
     * Normalize Twitter analytics to common format
    protected normalizeAnalytics(twitterMetrics: TwitterMetrics): SocialAnalytics {
            impressions: twitterMetrics.impression_count || 0,
            engagements: twitterMetrics.engagement_count || 0,
            clicks: twitterMetrics.url_link_clicks || 0,
            shares: twitterMetrics.retweet_count || 0,
            comments: twitterMetrics.reply_count || 0,
            likes: twitterMetrics.like_count || 0,
            reach: twitterMetrics.impression_count || 0,
            videoViews: twitterMetrics.video_view_count,
            platformMetrics: twitterMetrics
     * Search for tweets - implemented in search action
        // This is implemented in the search-tweets.action.ts
        throw new Error('Search posts is implemented in TwitterSearchTweetsAction');
     * Handle Twitter-specific errors
    protected handleTwitterError(error: AxiosError): never {
                    throw new Error(`Bad Request: ${errorData.detail || errorData.message || 'Invalid request parameters'}`);
                    throw new Error('Forbidden: Insufficient permissions. Ensure the app has required Twitter scopes.');
                    throw new Error('Internal Server Error: Twitter service error');
                    throw new Error('Service Unavailable: Twitter service temporarily unavailable');
                    throw new Error(`Twitter API Error (${status}): ${errorData.detail || errorData.message || 'Unknown error'}`);
            throw new Error('Network Error: No response from Twitter');
     * Parse Twitter-specific rate limit headers
        const remaining = headers['x-rate-limit-remaining'];
        const reset = headers['x-rate-limit-reset'];
        const limit = headers['x-rate-limit-limit'];
                reset: new Date(parseInt(reset) * 1000), // Unix timestamp to Date
     * Build search query with operators
    protected buildSearchQuery(params: SearchParams): string {
            parts.push(params.query);
            const hashtagQuery = params.hashtags
                .map(tag => tag.startsWith('#') ? tag : `#${tag}`)
                .join(' OR ');
            parts.push(`(${hashtagQuery})`);
        return parts.join(' ');
     * Format date for Twitter API (RFC 3339)
    protected formatTwitterDate(date: Date | string): string {
 * Twitter-specific interfaces
export interface TwitterUser {
    profile_image_url?: string;
    verified?: boolean;
export interface CreateTweetData {
        media_ids: string[];
    poll?: {
        options: string[];
        duration_minutes: number;
    reply?: {
        in_reply_to_tweet_id: string;
    quote_tweet_id?: string;
export interface Tweet {
    author_id?: string;
    conversation_id?: string;
    public_metrics?: {
        retweet_count: number;
        quote_count: number;
        bookmark_count: number;
        impression_count: number;
        media_keys?: string[];
        poll_ids?: string[];
    entities?: {
        hashtags?: Array<{ start: number; end: number; tag: string }>;
        mentions?: Array<{ start: number; end: number; username: string }>;
        urls?: Array<{ start: number; end: number; url: string; expanded_url: string }>;
    referenced_tweets?: Array<{
        type: 'retweeted' | 'quoted' | 'replied_to';
export interface TwitterMetrics {
    engagement_count: number;
    url_link_clicks: number;
    user_profile_clicks: number;
    video_view_count?: number;
export interface TwitterSearchParams {
    start_time?: string;
    max_results?: number;
    next_token?: string;
    since_id?: string;
    until_id?: string;
    sort_order?: 'recency' | 'relevancy';
