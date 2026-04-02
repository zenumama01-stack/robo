 * Creates an Instagram Story with images or videos.
 * Stories are temporary content that disappears after 24 hours.
 * Supports stickers, links, and interactive elements.
@RegisterClass(BaseAction, 'Instagram - Create Story')
export class InstagramCreateStoryAction extends InstagramBaseAction {
            const mediaFile = this.getParamValue(params.Params, 'MediaFile') as MediaFile;
            const stickerType = this.getParamValue(params.Params, 'StickerType');
            const stickerData = this.getParamValue(params.Params, 'StickerData');
            const linkUrl = this.getParamValue(params.Params, 'LinkUrl');
            const linkText = this.getParamValue(params.Params, 'LinkText');
            const mentionedUsers = this.getParamValue(params.Params, 'MentionedUsers') as string[];
            const hashtags = this.getParamValue(params.Params, 'Hashtags') as string[];
            if (!mediaFile) {
                    Message: 'MediaFile is required for stories',
            // Validate media type and duration
            const validation = this.validateStoryMedia(mediaFile);
            if (!validation.isValid) {
                    Message: validation.message,
            // Check if account can add links (requires 10k+ followers or verified)
            let canAddLink = false;
            if (linkUrl) {
                canAddLink = await this.checkLinkEligibility();
                if (!canAddLink) {
                    LogError('Account not eligible for story links. Requires 10k+ followers or verification.');
            // Create story media container
            const storyParams: any = {
                media_type: 'STORIES',
            // Add media URL
            if (mediaFile.mimeType.startsWith('image/')) {
                storyParams.image_url = await this.uploadStoryMediaToCDN(mediaFile);
            } else if (mediaFile.mimeType.startsWith('video/')) {
                storyParams.video_url = await this.uploadStoryMediaToCDN(mediaFile);
            // Add stickers if specified
            if (stickerType && stickerData) {
                storyParams.sticker = this.formatSticker(stickerType, stickerData);
            // Add mentions
            if (mentionedUsers && mentionedUsers.length > 0) {
                storyParams.user_tags = mentionedUsers.map((username, index) => ({
                    x: 0.5, // Center horizontally
                    y: 0.1 + (index * 0.1) // Stack vertically
            // Add hashtags
            if (hashtags && hashtags.length > 0) {
                storyParams.hashtags = hashtags.map((tag, index) => ({
                    hashtag: tag.startsWith('#') ? tag : `#${tag}`,
                    x: 0.5,
                    y: 0.8 - (index * 0.1)
            // Create the story container
            const containerResponse = await this.makeInstagramRequest<{ id: string }>(
                storyParams
            await this.waitForMediaContainer(containerResponse.id);
            // Publish the story
                creation_id: containerResponse.id,
            // Add link if eligible and provided
            if (linkUrl && canAddLink) {
                publishParams.link = {
                    url: linkUrl,
                    text: linkText || 'See More'
            // Get story details
            const storyDetails = await this.getStoryDetails(publishResponse.id);
                Name: 'StoryID',
                Value: publishResponse.id
                Name: 'MediaType',
                Value: mediaFile.mimeType.startsWith('image/') ? 'IMAGE' : 'VIDEO'
                Name: 'ExpiresAt',
                Value: new Date(Date.now() + 24 * 60 * 60 * 1000).toISOString()
                Name: 'StoryData',
                Value: JSON.stringify({
                    features: {
                        hasSticker: !!stickerType,
                        hasLink: linkUrl && canAddLink,
                        hasMentions: mentionedUsers && mentionedUsers.length > 0,
                        hasHashtags: hashtags && hashtags.length > 0
                    insights: storyDetails.insights || {}
                Message: 'Instagram story created successfully',
            LogError('Failed to create Instagram story', error);
                Message: `Failed to create story: ${error.message}`,
     * Validate story media requirements
    private validateStoryMedia(mediaFile: MediaFile): { isValid: boolean; message?: string } {
        // Check file size (max 30MB for images, 100MB for videos)
        const maxSize = mediaFile.mimeType.startsWith('image/') ? 30 * 1024 * 1024 : 100 * 1024 * 1024;
        if (mediaFile.size > maxSize) {
                isValid: false,
                message: `File size exceeds limit. Max ${maxSize / (1024 * 1024)}MB`
        // Check media type
        const supportedTypes = ['image/jpeg', 'image/png', 'video/mp4', 'video/mov'];
        if (!supportedTypes.includes(mediaFile.mimeType)) {
                message: `Unsupported media type. Supported: ${supportedTypes.join(', ')}`
        // For videos, check duration (max 60 seconds for stories)
        // In a real implementation, you'd extract video metadata
        return { isValid: true };
     * Check if account is eligible for story links
    private async checkLinkEligibility(): Promise<boolean> {
            const accountInfo = await this.makeInstagramRequest<any>(
                this.instagramBusinessAccountId,
                    fields: 'followers_count,is_verified',
            return accountInfo.is_verified || accountInfo.followers_count >= 10000;
            LogError('Failed to check link eligibility', error);
     * Format sticker data based on type
    private formatSticker(type: string, data: any): any {
            case 'location':
                    type: 'location',
                    location_id: data.locationId,
                    x: data.x || 0.5,
                    y: data.y || 0.5,
                    width: data.width || 0.5,
                    height: data.height || 0.1,
                    rotation: data.rotation || 0
            case 'poll':
                    type: 'poll',
                    question: data.question,
                    options: data.options || ['Yes', 'No'],
                    y: data.y || 0.5
            case 'question':
                    type: 'question',
                    text_color: data.textColor || '#FFFFFF',
                    background_color: data.backgroundColor || '#000000',
            case 'music':
                    type: 'music',
                    audio_asset_id: data.audioAssetId,
                    display_type: data.displayType || 'default',
            case 'countdown':
                    type: 'countdown',
                    end_time: data.endTime,
                    text: data.text || 'Countdown',
     * Get story details after publishing
    private async getStoryDetails(storyId: string): Promise<any> {
            const response = await this.makeInstagramRequest(
                storyId,
                    fields: 'id,media_type,permalink,timestamp',
            // Try to get initial insights (may not be immediately available)
                const insights = await this.getInsights(
                    ['impressions', 'reach', 'exits', 'replies'],
                    'lifetime'
                response.insights = this.parseStoryInsights(insights);
            } catch (insightError) {
                // Insights might not be available immediately
                response.insights = null;
            LogError('Failed to get story details', error);
            return { id: storyId };
     * Parse story-specific insights
    private parseStoryInsights(insights: any[]): any {
        const parsed: any = {};
        insights.forEach(metric => {
            if (metric.values && metric.values.length > 0) {
                parsed[metric.name] = metric.values[0].value || 0;
     * Upload story media to CDN (placeholder implementation)
    private async uploadStoryMediaToCDN(file: MediaFile): Promise<string> {
        // In a real implementation, this would upload to a CDN
                Name: 'MediaFile',
                Name: 'StickerType',
                Name: 'StickerData',
                Name: 'LinkUrl',
                Name: 'LinkText',
                Name: 'MentionedUsers',
        return 'Creates an Instagram Story with support for stickers, links, mentions, and interactive elements. Stories disappear after 24 hours.';
