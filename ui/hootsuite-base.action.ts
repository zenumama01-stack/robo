import { BaseSocialMediaAction, MediaFile, SocialPost, SearchParams } from '../../base/base-social.action';
 * Base class for all HootSuite actions.
 * Handles HootSuite-specific authentication, API interactions, and rate limiting.
@RegisterClass(BaseAction, 'HootSuiteBaseAction')
export abstract class HootSuiteBaseAction extends BaseSocialMediaAction {
        return 'HootSuite';
        return 'https://platform.hootsuite.com/v1';
                    // Log rate limit info
                        LogStatus(`HootSuite Rate Limit - Remaining: ${rateLimitInfo.remaining}/${rateLimitInfo.limit}, Reset: ${rateLimitInfo.reset}`);
                        // Rate limit exceeded
                        const waitTime = retryAfter ? parseInt(retryAfter) : 60;
            throw new Error('No refresh token available for HootSuite');
            const response = await axios.post('https://platform.hootsuite.com/oauth2/token', {
                grant_type: 'refresh_token',
                client_id: this.getCustomAttribute(2), // Client ID stored in CustomAttribute2
                client_secret: this.getCustomAttribute(3) // Client Secret stored in CustomAttribute3
                    'Content-Type': 'application/x-www-form-urlencoded'
            const { access_token, refresh_token: newRefreshToken, expires_in } = response.data;
                newRefreshToken || refreshToken,
            LogStatus('HootSuite access token refreshed successfully');
            LogError(`Failed to refresh HootSuite access token: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Upload media to HootSuite
            // First, request an upload URL
            const uploadRequest = await this.axiosInstance.post('/media', {
                mimeType: file.mimeType,
                sizeBytes: file.size
            const { uploadUrl, mediaId } = uploadRequest.data;
            // Upload the file to the provided URL
            await axios.put(uploadUrl, fileData, {
                    'Content-Type': file.mimeType,
                    'Content-Length': file.size.toString()
            // Wait for processing
            await this.waitForMediaProcessing(mediaId);
            return mediaId;
            LogError(`Failed to upload media to HootSuite: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Wait for media to finish processing
    private async waitForMediaProcessing(mediaId: string, maxAttempts: number = 30): Promise<void> {
        for (let i = 0; i < maxAttempts; i++) {
                const response = await this.axiosInstance.get(`/media/${mediaId}`);
                const { state } = response.data;
                if (state === 'READY') {
                } else if (state === 'FAILED') {
                    throw new Error('Media processing failed');
                // Wait 2 seconds before next check
                await new Promise(resolve => setTimeout(resolve, 2000));
                if (i === maxAttempts - 1) {
                    throw new Error(`Media processing timeout for ${mediaId}`);
     * Get social profiles for the authenticated user
    protected async getSocialProfiles(): Promise<HootSuiteProfile[]> {
            const response = await this.axiosInstance.get('/socialProfiles');
            LogError(`Failed to get social profiles: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Make a paginated request to HootSuite API
    protected async makePaginatedRequest<T>(
        params: Record<string, any> = {}
        let cursor: string | null = null;
            const queryParams: any = { ...params, limit };
            if (cursor) {
                queryParams.cursor = cursor;
            const response = await this.axiosInstance.get(endpoint, { params: queryParams });
            const data = response.data;
            if (data.data && Array.isArray(data.data)) {
                results.push(...data.data);
            cursor = data.cursor || null;
            // Check if we've reached the desired number of results
            if (params.maxResults && results.length >= params.maxResults) {
                return results.slice(0, params.maxResults);
        } while (cursor);
     * Format date for HootSuite API (ISO 8601)
    protected formatHootSuiteDate(date: Date | string): string {
     * Parse HootSuite date string
    protected parseHootSuiteDate(dateString: string): Date {
     * Convert HootSuite post to common format
    protected normalizePost(hootsuitePost: HootSuitePost): SocialPost {
            id: hootsuitePost.id,
            platform: 'HootSuite',
            profileId: hootsuitePost.socialProfileIds.join(','), // Multiple profiles possible
            content: hootsuitePost.text,
            mediaUrls: hootsuitePost.mediaIds || [],
            publishedAt: this.parseHootSuiteDate(hootsuitePost.createdTime),
            scheduledFor: hootsuitePost.scheduledTime ? this.parseHootSuiteDate(hootsuitePost.scheduledTime) : undefined,
                state: hootsuitePost.state,
                tags: hootsuitePost.tags,
                location: hootsuitePost.location,
                socialProfileIds: hootsuitePost.socialProfileIds
        throw new Error('Search posts is implemented in HootSuiteSearchPostsAction');
     * Handle HootSuite-specific errors
    protected handleHootSuiteError(error: AxiosError): never {
                    throw new Error('Rate Limit Exceeded: Too many requests');
                    throw new Error('Internal Server Error: HootSuite service error');
                    throw new Error(`HootSuite API Error (${status}): ${errorData.message || 'Unknown error'}`);
            throw new Error('Network Error: No response from HootSuite');
 * HootSuite-specific interfaces
export interface HootSuiteProfile {
    socialNetworkId: string;
    socialNetworkUserId: string;
    avatarUrl: string;
    ownerId: string;
export interface HootSuitePost {
    socialProfileIds: string[];
    scheduledTime?: string;
    createdTime: string;
    state: 'SCHEDULED' | 'PUBLISHED' | 'FAILED' | 'DRAFT';
    mediaIds?: string[];
    location?: {
        latitude: number;
        longitude: number;
export interface HootSuiteAnalytics {
    postId: string;
    metrics: {
        start: string;
        end: string;
