import { BaseSocialMediaAction, MediaFile } from '../../base/base-social.action';
import { UserInfo, LogStatus, LogError } from '@memberjunction/core';
 * Base class for all Instagram actions.
 * Handles Instagram Graph API and Basic Display API authentication and common functionality.
 * Instagram uses Facebook's Graph API infrastructure.
@RegisterClass(BaseAction, 'InstagramBaseAction')
export abstract class InstagramBaseAction extends BaseSocialMediaAction {
        return 'Instagram';
     * Instagram Business Account ID (stored in CustomAttribute1)
    protected get instagramBusinessAccountId(): string {
        return this.getCustomAttribute(1) || '';
     * Facebook Page ID (stored in CustomAttribute2) - required for Instagram Business API
    protected get facebookPageId(): string {
        return this.getCustomAttribute(2) || '';
     * Axios instance for API calls
     * Get or create axios instance with authentication
    protected get axios(): AxiosInstance {
                headers: this.buildHeaders()
            // Add response interceptor for error handling
                response => response,
                async error => {
                    if (error.response?.status === 401 && this.isAuthError(error)) {
                        LogStatus('Instagram token appears invalid, attempting refresh...');
                        await this.refreshAccessToken();
                        // Retry the request with new token
                        error.config.headers.Authorization = `Bearer ${this.getAccessToken()}`;
                        return axios.request(error.config);
     * Refresh the Instagram/Facebook access token
            // Instagram uses Facebook's OAuth system
            // Long-lived tokens need to be exchanged periodically
                throw new Error('No access token available to refresh');
            // Exchange for a new long-lived token
                    client_id: this.getCustomAttribute(3), // App ID stored in CustomAttribute3
                    client_secret: this.getCustomAttribute(4), // App Secret stored in CustomAttribute4
            if (response.data.access_token) {
                    response.data.access_token,
                    undefined, // Instagram doesn't use refresh tokens
                    response.data.expires_in || 5184000 // Default to 60 days
                // Reset axios instance to use new token
                this._axiosInstance = null;
            LogError('Failed to refresh Instagram access token', error);
            throw new Error('Failed to refresh Instagram access token');
     * Make an Instagram Graph API request
    protected async makeInstagramRequest<T = any>(
        method: 'GET' | 'POST' | 'DELETE' = 'GET',
        data?: any,
        params?: any
            this.logApiRequest(method, `${this.apiBaseUrl}/${endpoint}`, data || params);
            const response = await this.axios.request<T>({
                url: endpoint,
            this.logApiResponse(response.data);
                this.handleInstagramError(error);
     * Handle Instagram-specific errors
    protected handleInstagramError(error: AxiosError): void {
        const response = error.response;
            throw new Error('Network error occurred while calling Instagram API');
        const errorData = response.data as any;
        const errorMessage = errorData?.error?.message || 'Unknown Instagram API error';
        // Check for rate limiting
        if (errorCode === 32 || errorCode === 4 || response.status === 429) {
            const retryAfter = response.headers['x-app-usage'] 
                ? this.parseAppUsage(response.headers['x-app-usage']) 
                : 3600; // Default to 1 hour
                code: 'RATE_LIMIT',
                message: 'Instagram API rate limit exceeded',
                retryAfter
        // Check for permission errors
        if (errorCode === 10 || errorSubcode === 460) {
                code: 'INSUFFICIENT_PERMISSIONS',
                message: 'Insufficient permissions for this Instagram operation'
        // Media errors
        if (errorCode === 100 && errorMessage.toLowerCase().includes('media')) {
                code: 'INVALID_MEDIA',
                message: errorMessage
        // Post not found
        if (errorCode === 100 && errorSubcode === 33) {
                code: 'POST_NOT_FOUND',
                message: 'Instagram post not found'
            code: 'PLATFORM_ERROR',
            details: errorData
     * Parse Facebook's app usage header to determine rate limit status
    private parseAppUsage(appUsage: string): number {
            const usage = JSON.parse(appUsage);
            const totalCputime = usage.total_cputime || 0;
            // If any metric is above 90%, implement backoff
            if (callCount > 90 || totalTime > 90 || totalCputime > 90) {
                return 3600; // Wait 1 hour
     * Upload media to Instagram (returns container ID)
            // For Instagram, media must be hosted at a public URL
            // This is a simplified version - in production, you'd upload to a CDN first
            const mediaUrl = await this.uploadMediaToCDN(file);
            let containerParams: any = {
                access_token: this.getAccessToken()
            // Determine media type and set appropriate parameters
            if (file.mimeType.startsWith('image/')) {
                containerParams.image_url = mediaUrl;
                // Check if it's a carousel
                if (file.filename.includes('carousel')) {
                    containerParams.is_carousel_item = true;
            } else if (file.mimeType.startsWith('video/')) {
                containerParams.video_url = mediaUrl;
                containerParams.media_type = 'REELS'; // or 'VIDEO' for feed videos
            // Add caption if provided in metadata
            const metadata = (file as any).metadata;
            if (metadata?.caption) {
                containerParams.caption = metadata.caption;
            // Create media container
            const response = await this.makeInstagramRequest<{ id: string }>(
                `${this.instagramBusinessAccountId}/media`,
                containerParams
            return response.id;
            LogError('Failed to upload media to Instagram', error);
     * Upload media to a CDN (placeholder - implement based on your CDN)
    private async uploadMediaToCDN(file: MediaFile): Promise<string> {
        // In a real implementation, this would upload to S3, Cloudinary, etc.
        // For now, throw an error indicating this needs implementation
        throw new Error('Media CDN upload not implemented. Instagram requires media to be hosted at a public URL.');
     * Publish a media container
    protected async publishMediaContainer(containerId: string): Promise<string> {
            `${this.instagramBusinessAccountId}/media_publish`,
                creation_id: containerId,
     * Get insights for a media object or account
    protected async getInsights(
        objectId: string,
        period?: 'lifetime' | 'day' | 'week' | 'days_28'
        if (period) {
            params.period = period;
        const response = await this.makeInstagramRequest<{ data: any[] }>(
            `${objectId}/insights`,
     * Search for posts (limited to business account's own posts)
    protected async searchPosts(params: any): Promise<any[]> {
        // Instagram doesn't have a general search API
        // We can only search within the business account's own posts
        const fields = 'id,caption,media_type,media_url,permalink,timestamp,like_count,comments_count';
        let endpoint = `${this.instagramBusinessAccountId}/media`;
            access_token: this.getAccessToken(),
            limit: params.limit || 25
        // Add date filtering if provided
            queryParams.since = Math.floor(new Date(params.startDate).getTime() / 1000);
            queryParams.until = Math.floor(new Date(params.endDate).getTime() / 1000);
        const posts: any[] = [];
        let hasNext = true;
        while (hasNext && posts.length < (params.limit || 100)) {
            const response = await this.makeInstagramRequest<{
                data: any[];
                paging?: { next: string };
            }>(endpoint, 'GET', null, queryParams);
            if (response.data) {
                // Filter by caption if query is provided
                const filtered = params.query
                    ? response.data.filter(post => 
                        post.caption?.toLowerCase().includes(params.query.toLowerCase()))
                    : response.data;
                posts.push(...filtered);
            if (response.paging?.next) {
                // Parse next URL for pagination
                const nextUrl = new URL(response.paging.next);
                queryParams.after = nextUrl.searchParams.get('after');
                hasNext = false;
     * Normalize Instagram post to common format
    protected normalizePost(instagramPost: any): any {
            id: instagramPost.id,
            platform: 'Instagram',
            profileId: this.instagramBusinessAccountId,
            content: instagramPost.caption || '',
            mediaUrls: instagramPost.media_url ? [instagramPost.media_url] : [],
            publishedAt: new Date(instagramPost.timestamp),
            analytics: {
                impressions: instagramPost.impressions_count || 0,
                engagements: (instagramPost.like_count || 0) + (instagramPost.comments_count || 0),
                clicks: 0, // Not available in basic metrics
                shares: 0, // Instagram doesn't track shares
                comments: instagramPost.comments_count || 0,
                likes: instagramPost.like_count || 0,
                reach: instagramPost.reach || 0,
                saves: instagramPost.saved || 0,
                videoViews: instagramPost.video_views || 0,
                platformMetrics: instagramPost
                mediaType: instagramPost.media_type,
                permalink: instagramPost.permalink,
                isCarousel: instagramPost.media_type === 'CAROUSEL_ALBUM'
     * Check if media container is ready for publishing
    protected async isMediaContainerReady(containerId: string): Promise<boolean> {
        const response = await this.makeInstagramRequest<{ status_code: string }>(
            containerId,
                fields: 'status_code',
        return response.status_code === 'FINISHED';
     * Wait for media container to be ready
    protected async waitForMediaContainer(containerId: string, maxWaitTime: number = 60000): Promise<void> {
        const pollInterval = 2000; // 2 seconds
        while (Date.now() - startTime < maxWaitTime) {
            if (await this.isMediaContainerReady(containerId)) {
            await new Promise(resolve => setTimeout(resolve, pollInterval));
        throw new Error('Media container processing timeout');
