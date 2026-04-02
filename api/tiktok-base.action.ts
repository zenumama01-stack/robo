 * TikTok video information
export interface TikTokVideo {
    share_url: string;
    cover_image_url: string;
    share_count: number;
    view_count: number;
    comment_count: number;
    create_time: number;
 * TikTok user information
export interface TikTokUser {
    open_id: string;
    union_id: string;
    avatar_url: string;
    display_name: string;
    bio_description: string;
    profile_deep_link: string;
    is_verified: boolean;
    follower_count: number;
    following_count: number;
    likes_count: number;
 * Base class for all TikTok social media actions.
 * Handles TikTok-specific authentication and API interaction patterns.
@RegisterClass(BaseAction, 'TikTokBaseAction')
export abstract class TikTokBaseAction extends BaseSocialMediaAction {
        return 'TikTok';
        return 'https://open-api.tiktok.com';
     * Initialize axios instance with interceptors
            // Request interceptor for logging
                    this.logApiRequest(config.method?.toUpperCase() || 'GET', config.url || '', config.data);
                    LogError(`TikTok API Request Error: ${error.message}`);
            // Response interceptor for logging and error handling
                        await this.handleRateLimit(retryAfter ? parseInt(retryAfter) : undefined);
                        return this.axiosInstance?.request(error.config!);
                    LogError(`TikTok API Response Error: ${error.message}`);
     * Make authenticated TikTok API request
    protected async makeTikTokRequest<T>(
            throw new Error('No access token available for TikTok');
        const axios = this.getAxiosInstance();
            const response = await axios.request<T>({
                    'Authorization': `Bearer ${token}`
            if (error instanceof AxiosError) {
                if (error.response?.status === 401) {
                    // Token might be expired, try to refresh
                    // Retry with new token
                    const newToken = this.getAccessToken();
                    const retryResponse = await axios.request<T>({
                            'Authorization': `Bearer ${newToken}`
                    return retryResponse.data;
                const errorMessage = error.response?.data?.error?.message || error.message;
                throw new Error(`TikTok API error: ${errorMessage}`);
     * Refresh TikTok access token
            throw new Error('No refresh token available for TikTok');
            const response = await axios.post(`${this.apiBaseUrl}/oauth/refresh_token/`, {
                client_key: this.getCustomAttribute(2), // Store client key in CustomAttribute2
                refresh_token: refreshToken
            const { access_token, refresh_token: newRefreshToken, expires_in } = response.data.data;
            await this.updateStoredTokens(access_token, newRefreshToken, expires_in);
            LogStatus('TikTok access token refreshed successfully');
            LogError(`Failed to refresh TikTok access token: ${error}`);
            throw new Error('Failed to refresh TikTok access token');
     * Upload media to TikTok (requires special approval)
        // TikTok video upload requires special approval
        // This is a placeholder implementation
        throw new Error('TikTok video upload requires special API approval. Please use TikTok Creator Studio for video uploads.');
     * Search posts (videos) - TikTok only allows searching user's own videos
    protected async searchPosts(params: any): Promise<SocialPost[]> {
        // TikTok doesn't provide a general search API for security/privacy reasons
        // We can only search within a user's own videos
        const videos = await this.getUserVideos();
        let filtered = videos;
        // Apply filters if provided
            filtered = filtered.filter(video => 
                video.title.toLowerCase().includes(query) ||
                video.description.toLowerCase().includes(query)
            filtered = filtered.filter(video => {
                const videoHashtags = this.extractHashtags(video.description);
                return params.hashtags.some((tag: string) => 
                    videoHashtags.includes(tag.toLowerCase())
            const startTime = new Date(params.startDate).getTime() / 1000;
            filtered = filtered.filter(video => video.create_time >= startTime);
            const endTime = new Date(params.endDate).getTime() / 1000;
            filtered = filtered.filter(video => video.create_time <= endTime);
        // Apply limit and offset
            filtered = filtered.slice(params.offset);
        if (params.limit) {
            filtered = filtered.slice(0, params.limit);
        return filtered.map(video => this.normalizePost(video));
     * Get user's videos
    protected async getUserVideos(): Promise<TikTokVideo[]> {
        const response = await this.makeTikTokRequest<any>(
            '/v2/video/list/',
                fields: 'id,share_url,title,description,duration,cover_image_url,share_count,view_count,like_count,comment_count,create_time'
        return response.data?.videos || [];
     * Convert TikTok video to common post format
    protected normalizePost(video: TikTokVideo): SocialPost {
            id: video.id,
            platform: this.platformName,
            profileId: this.getCustomAttribute(1) || '', // Store user ID in CustomAttribute1
            content: video.description || video.title,
            mediaUrls: [video.cover_image_url],
            publishedAt: new Date(video.create_time * 1000),
                impressions: video.view_count,
                engagements: video.like_count + video.comment_count + video.share_count,
                clicks: 0, // Not available in TikTok API
                shares: video.share_count,
                comments: video.comment_count,
                likes: video.like_count,
                reach: video.view_count,
                videoViews: video.view_count,
                    duration: video.duration,
                    shareUrl: video.share_url
                ...video,
                videoUrl: video.share_url
     * Normalize TikTok analytics to common format
            impressions: platformData.view_count || 0,
            engagements: (platformData.like_count || 0) + (platformData.comment_count || 0) + (platformData.share_count || 0),
            shares: platformData.share_count || 0,
            comments: platformData.comment_count || 0,
            likes: platformData.like_count || 0,
            reach: platformData.view_count || 0,
            videoViews: platformData.view_count || 0,
     * Extract hashtags from video description
    protected extractHashtags(description: string): string[] {
        const hashtagRegex = /#(\w+)/g;
        const matches = description.match(hashtagRegex) || [];
        return matches.map(tag => tag.substring(1).toLowerCase());
     * Get current user info
    protected async getCurrentUser(): Promise<TikTokUser> {
            '/v2/user/info/',
                fields: 'open_id,union_id,avatar_url,display_name,bio_description,profile_deep_link,is_verified,follower_count,following_count,likes_count'
        return response.data?.user;
     * Validate TikTok-specific media requirements
        const allowedTypes = ['video/mp4', 'video/quicktime', 'video/webm'];
        if (!allowedTypes.includes(file.mimeType)) {
            throw new Error(`TikTok only supports video files. Got: ${file.mimeType}`);
        // Max file size: 287.6 MB
        const maxSize = 287.6 * 1024 * 1024;
            throw new Error(`File size exceeds TikTok limit of 287.6 MB. Got: ${(file.size / 1024 / 1024).toFixed(2)} MB`);
