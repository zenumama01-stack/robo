 * Base class for all LinkedIn actions.
 * Handles LinkedIn-specific authentication, API interactions, and rate limiting.
 * Uses LinkedIn Marketing Developer Platform API v2.
@RegisterClass(BaseAction, 'LinkedInBaseAction')
export abstract class LinkedInBaseAction extends BaseSocialMediaAction {
        return 'LinkedIn';
        return 'https://api.linkedin.com/v2';
                    'X-Restli-Protocol-Version': '2.0.0' // LinkedIn specific header
                        LogStatus(`LinkedIn Rate Limit - Remaining: ${rateLimitInfo.remaining}/${rateLimitInfo.limit}, Reset: ${rateLimitInfo.reset}`);
            throw new Error('No refresh token available for LinkedIn');
            const response = await axios.post('https://www.linkedin.com/oauth/v2/accessToken', 
                new URLSearchParams({
                    client_id: this.getCustomAttribute(2) || '', // Client ID stored in CustomAttribute2
                    client_secret: this.getCustomAttribute(3) || '' // Client Secret stored in CustomAttribute3
                }).toString(),
            LogStatus('LinkedIn access token refreshed successfully');
            LogError(`Failed to refresh LinkedIn access token: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get the authenticated user's profile URN
    protected async getCurrentUserUrn(): Promise<string> {
            const response = await this.axiosInstance.get('/me');
            return `urn:li:person:${response.data.id}`;
            LogError(`Failed to get current user URN: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get organizations the user has admin access to
    protected async getAdminOrganizations(): Promise<LinkedInOrganization[]> {
            const response = await this.axiosInstance.get('/organizationalEntityAcls', {
                    q: 'roleAssignee',
                    role: 'ADMINISTRATOR',
                    projection: '(elements*(*,organizationalTarget~(localizedName)))'
            const organizations: LinkedInOrganization[] = [];
            if (response.data.elements) {
                for (const element of response.data.elements) {
                    if (element.organizationalTarget) {
                        organizations.push({
                            urn: element.organizationalTarget,
                            name: element['organizationalTarget~']?.localizedName || 'Unknown',
                            id: element.organizationalTarget.split(':').pop() || ''
            return organizations;
            LogError(`Failed to get admin organizations: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Upload media to LinkedIn
            // Step 1: Register upload
            const registerResponse = await this.axiosInstance.post('/assets?action=registerUpload', {
                registerUploadRequest: {
                    recipes: ['urn:li:digitalmediaRecipe:feedshare-image'],
                    owner: await this.getCurrentUserUrn(),
                    serviceRelationships: [{
                        relationshipType: 'OWNER',
                        identifier: 'urn:li:userGeneratedContent'
            const uploadUrl = registerResponse.data.value.uploadMechanism['com.linkedin.digitalmedia.uploading.MediaUploadHttpRequest'].uploadUrl;
            const asset = registerResponse.data.value.asset;
            // Step 2: Upload the file
                    'Authorization': `Bearer ${this.getAccessToken()}`,
                    'Content-Type': file.mimeType
            // Return the asset URN
            return asset;
            LogError(`Failed to upload media to LinkedIn: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Validate media file meets LinkedIn requirements
            'image/webp'
        // LinkedIn image size limits
        const maxSize = 10 * 1024 * 1024; // 10MB
     * Create a share (post) on LinkedIn
    protected async createShare(shareData: LinkedInShareData): Promise<string> {
            const response = await this.axiosInstance.post('/ugcPosts', shareData);
            this.handleLinkedInError(error as AxiosError);
     * Get shares for a specific author (person or organization)
    protected async getShares(authorUrn: string, count: number = 50, start: number = 0): Promise<LinkedInShare[]> {
            const response = await this.axiosInstance.get('/ugcPosts', {
                    q: 'authors',
                    authors: `List(${authorUrn})`,
                    count: count,
                    start: start
            return response.data.elements || [];
            LogError(`Failed to get shares: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Convert LinkedIn share to common format
    protected normalizePost(linkedInShare: LinkedInShare): SocialPost {
        const publishedAt = new Date(linkedInShare.firstPublishedAt || linkedInShare.created.time);
        // Extract media URLs
        if (linkedInShare.specificContent?.['com.linkedin.ugc.ShareContent']?.media) {
            for (const media of linkedInShare.specificContent['com.linkedin.ugc.ShareContent'].media) {
                if (media.media) {
                    mediaUrls.push(media.media);
            id: linkedInShare.id,
            platform: 'LinkedIn',
            profileId: linkedInShare.author,
            content: linkedInShare.specificContent?.['com.linkedin.ugc.ShareContent']?.shareCommentary?.text || '',
            mediaUrls: mediaUrls,
            publishedAt: publishedAt,
                lifecycleState: linkedInShare.lifecycleState,
                visibility: linkedInShare.visibility,
                distribution: linkedInShare.distribution
     * Normalize LinkedIn analytics to common format
    protected normalizeAnalytics(linkedInAnalytics: LinkedInAnalytics): SocialAnalytics {
            impressions: linkedInAnalytics.totalShareStatistics?.impressionCount || 0,
            engagements: linkedInAnalytics.totalShareStatistics?.engagement || 0,
            clicks: linkedInAnalytics.totalShareStatistics?.clickCount || 0,
            shares: linkedInAnalytics.totalShareStatistics?.shareCount || 0,
            comments: linkedInAnalytics.totalShareStatistics?.commentCount || 0,
            likes: linkedInAnalytics.totalShareStatistics?.likeCount || 0,
            reach: linkedInAnalytics.totalShareStatistics?.uniqueImpressionsCount || 0,
            platformMetrics: linkedInAnalytics
        throw new Error('Search posts is implemented in LinkedInSearchPostsAction');
     * Handle LinkedIn-specific errors
    protected handleLinkedInError(error: AxiosError): never {
                    throw new Error('Forbidden: Insufficient permissions. Ensure the app has required LinkedIn scopes.');
                    throw new Error(`Unprocessable Entity: ${errorData.message || 'Invalid data provided'}`);
                    throw new Error('Internal Server Error: LinkedIn service error');
                    throw new Error(`LinkedIn API Error (${status}): ${errorData.message || 'Unknown error'}`);
            throw new Error('Network Error: No response from LinkedIn');
     * Parse LinkedIn-specific rate limit headers
        // LinkedIn uses different header names
        const appRemaining = headers['x-app-rate-limit-remaining'];
        const appLimit = headers['x-app-rate-limit-limit'];
        const memberRemaining = headers['x-member-rate-limit-remaining'];
        const memberLimit = headers['x-member-rate-limit-limit'];
        // Use the more restrictive limit
        const remaining = Math.min(
            appRemaining ? parseInt(appRemaining) : Infinity,
            memberRemaining ? parseInt(memberRemaining) : Infinity
        const limit = Math.min(
            appLimit ? parseInt(appLimit) : Infinity,
            memberLimit ? parseInt(memberLimit) : Infinity
        if (remaining !== Infinity && limit !== Infinity) {
            // LinkedIn resets rate limits at the top of each hour
            const reset = new Date(now);
            reset.setHours(reset.getHours() + 1, 0, 0, 0);
            return { remaining, reset, limit };
 * LinkedIn-specific interfaces
export interface LinkedInOrganization {
    urn: string;
export interface LinkedInShareData {
    author: string; // URN of the author (person or organization)
    lifecycleState: 'PUBLISHED' | 'DRAFT';
            shareCommentary: {
            shareMediaCategory: 'NONE' | 'ARTICLE' | 'IMAGE' | 'VIDEO' | 'RICH';
            media?: Array<{
                status: 'READY';
                media: string; // Asset URN
                title?: {
                description?: {
        'com.linkedin.ugc.MemberNetworkVisibility': 'PUBLIC' | 'CONNECTIONS' | 'LOGGED_IN' | 'CONTAINER';
    distribution?: {
        linkedInDistributionTarget?: {
            visibleToGuest?: boolean;
export interface LinkedInShare {
    author: string;
    created: {
        actor: string;
        time: number;
    firstPublishedAt?: number;
    lastModified?: {
    lifecycleState: string;
            shareMediaCategory: string;
                media: string;
        'com.linkedin.ugc.MemberNetworkVisibility': string;
    distribution?: any;
export interface LinkedInAnalytics {
    totalShareStatistics?: {
        impressionCount: number;
        clickCount: number;
        engagement: number;
        likeCount: number;
        commentCount: number;
        shareCount: number;
        uniqueImpressionsCount: number;
    timeRange?: {
        start: number;
        end: number;
export interface LinkedInArticle {
    publishedAt: number;
    coverImage?: string;
    visibility: string;
