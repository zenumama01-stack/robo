import { BaseSocialMediaAction } from '../../base/base-social.action';
import { SocialPost, MediaFile } from '../../base/base-social.action';
 * Base class for all YouTube actions.
 * Handles YouTube Data API v3 authentication and common functionality.
@RegisterClass(BaseAction, 'YouTubeBaseAction')
export abstract class YouTubeBaseAction extends BaseSocialMediaAction {
    protected override get platformName(): string {
        return 'YouTube';
    protected override get apiBaseUrl(): string {
        return 'https://www.googleapis.com/youtube/v3';
     * Axios instance for API requests
    protected axiosInstance: AxiosInstance;
     * Initialize the axios instance with base configuration
    protected initializeAxios(): void {
     * YouTube-specific OAuth token refresh
            throw new Error('No refresh token available for YouTube');
            const response = await axios.post('https://oauth2.googleapis.com/token', {
                client_id: this.getCustomAttribute(2), // Store client ID in CustomAttribute2
                client_secret: this.getCustomAttribute(3), // Store client secret in CustomAttribute3
            await this.updateStoredTokens(access_token, undefined, expires_in);
            throw new Error(`Failed to refresh YouTube access token: ${error.message}`);
     * Make authenticated request to YouTube API
    protected async makeYouTubeRequest<T = any>(
        params?: Record<string, any>,
            this.initializeAxios();
        return this.makeAuthenticatedRequest(async (token) => {
                const response = await this.axiosInstance.request<T>({
                    this.handleYouTubeApiError(error);
     * Handle YouTube API errors
    protected handleYouTubeApiError(error: AxiosError): void {
            throw new Error('Network error occurred');
        const errorData: any = response.data;
        const errorMessage = errorData?.error?.message || response.statusText;
        const errorCode = errorData?.error?.code || response.status;
        // Check for quota exceeded
        if (errorCode === 403 && errorMessage.includes('quota')) {
            throw new Error(`YouTube API quota exceeded. ${errorMessage}`);
        if (errorCode === 429) {
            const retryAfter = response.headers['retry-after'];
            throw new Error(`Rate limit exceeded. Retry after ${retryAfter || '60'} seconds`);
        throw new Error(`YouTube API error (${errorCode}): ${errorMessage}`);
     * Upload video to YouTube
    protected async uploadVideo(
            privacyStatus?: 'private' | 'unlisted' | 'public';
        // YouTube requires a resumable upload for videos
        const uploadUrl = await this.initiateResumableUpload(metadata);
        const videoId = await this.performResumableUpload(uploadUrl, videoFile);
        return videoId;
     * Initiate resumable upload session
    private async initiateResumableUpload(metadata: any): Promise<string> {
        const response = await this.makeYouTubeRequest<any>(
            '/videos',
                snippet: {
                    title: metadata.title,
                    description: metadata.description || '',
                    tags: metadata.tags || [],
                    categoryId: metadata.categoryId || '22' // Default to People & Blogs
                status: {
                    privacyStatus: metadata.privacyStatus || 'private'
                uploadType: 'resumable',
                part: 'snippet,status'
        return response.headers.location;
     * Perform the actual video upload
    private async performResumableUpload(uploadUrl: string, videoFile: MediaFile): Promise<string> {
        const videoData = Buffer.isBuffer(videoFile.data) 
            ? videoFile.data 
            : Buffer.from(videoFile.data, 'base64');
        const response = await axios.put(uploadUrl, videoData, {
                'Content-Type': videoFile.mimeType,
                'Content-Length': videoData.length.toString(),
     * Upload single media file (thumbnail)
        // For YouTube, this would typically be used for thumbnails
        // The actual implementation would upload to YouTube's thumbnail endpoint
        throw new Error('Direct media upload not supported. Use uploadVideo for videos or setThumbnail for thumbnails.');
     * Convert YouTube video to standard social post format
    protected normalizePost(youtubeVideo: any): SocialPost {
            id: youtubeVideo.id,
            platform: 'YouTube',
            profileId: youtubeVideo.snippet.channelId,
            content: youtubeVideo.snippet.description || '',
            mediaUrls: [`https://www.youtube.com/watch?v=${youtubeVideo.id}`],
            publishedAt: new Date(youtubeVideo.snippet.publishedAt),
            scheduledFor: youtubeVideo.status.publishAt ? new Date(youtubeVideo.status.publishAt) : undefined,
            analytics: this.extractVideoAnalytics(youtubeVideo),
                title: youtubeVideo.snippet.title,
                tags: youtubeVideo.snippet.tags || [],
                categoryId: youtubeVideo.snippet.categoryId,
                duration: youtubeVideo.contentDetails?.duration,
                definition: youtubeVideo.contentDetails?.definition,
                privacyStatus: youtubeVideo.status.privacyStatus,
                embeddable: youtubeVideo.status.embeddable,
                thumbnails: youtubeVideo.snippet.thumbnails
     * Extract analytics from video statistics
    private extractVideoAnalytics(video: any): any {
        if (!video.statistics) {
        return this.normalizeAnalytics({
            impressions: parseInt(video.statistics.viewCount || '0'),
            engagements: parseInt(video.statistics.likeCount || '0') + 
                         parseInt(video.statistics.commentCount || '0'),
            clicks: 0, // YouTube doesn't provide click data
            shares: 0, // YouTube doesn't provide share count
            comments: parseInt(video.statistics.commentCount || '0'),
            likes: parseInt(video.statistics.likeCount || '0'),
            reach: parseInt(video.statistics.viewCount || '0'),
            saves: parseInt(video.statistics.favoriteCount || '0'),
            videoViews: parseInt(video.statistics.viewCount || '0'),
            dislikes: parseInt(video.statistics.dislikeCount || '0')
     * Search for videos
        const searchParams: any = {
            part: 'snippet',
            type: 'video',
            maxResults: params.limit || 50,
            order: params.sortBy || 'relevance'
        // Add search query
            searchParams.q = params.query;
        // Add channel filter if searching within a specific channel
        if (params.channelId || this.getCustomAttribute(1)) {
            searchParams.channelId = params.channelId || this.getCustomAttribute(1);
            searchParams.publishedAfter = this.formatDate(params.startDate);
            searchParams.publishedBefore = this.formatDate(params.endDate);
        // Add pagination
        if (params.pageToken) {
            searchParams.pageToken = params.pageToken;
            '/search',
            searchParams
        // Get full video details for search results
        const videoIds = response.items.map((item: any) => item.id.videoId).join(',');
        const videosResponse = await this.makeYouTubeRequest<any>(
                part: 'snippet,statistics,status,contentDetails',
                id: videoIds
        return videosResponse.items.map((video: any) => this.normalizePost(video));
     * Get quota cost for an operation
    protected getQuotaCost(operation: string): number {
        const quotaCosts: Record<string, number> = {
            'videos.list': 1,
            'videos.insert': 1600,
            'videos.update': 50,
            'videos.delete': 50,
            'search.list': 100,
            'channels.list': 1,
            'playlists.list': 1,
            'playlists.insert': 50,
            'playlistItems.insert': 50,
            'comments.list': 1,
            'commentThreads.list': 1
        return quotaCosts[operation] || 1;
     * Parse ISO 8601 duration to seconds
    protected parseDuration(isoDuration: string): number {
        const matches = isoDuration.match(/PT(?:(\d+)H)?(?:(\d+)M)?(?:(\d+)S)?/);
        if (!matches) return 0;
        const hours = parseInt(matches[1] || '0');
        const minutes = parseInt(matches[2] || '0');
        const seconds = parseInt(matches[3] || '0');
        return hours * 3600 + minutes * 60 + seconds;
     * Format bytes to human readable size
    protected formatBytes(bytes: number): string {
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        if (bytes === 0) return '0 Bytes';
        const i = Math.floor(Math.log(bytes) / Math.log(1024));
        return Math.round(bytes / Math.pow(1024, i) * 100) / 100 + ' ' + sizes[i];
     * Validate video file
    protected validateVideoFile(file: MediaFile): void {
        const allowedTypes = [
            'video/x-msvideo', // AVI
            'video/quicktime', // MOV
            'video/x-ms-wmv', // WMV
            'video/x-flv', // FLV
            throw new Error(`Unsupported video format: ${file.mimeType}`);
        // YouTube max file size is 128GB or 12 hours
        const maxSize = 128 * 1024 * 1024 * 1024; // 128GB
            throw new Error(`Video file too large. Maximum size is ${this.formatBytes(maxSize)}`);
