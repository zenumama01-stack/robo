import { BufferBaseAction } from '../buffer-base.action';
import { MediaFile } from '../../../base/base-social.action';
 * Action to create a new post in Buffer (scheduled or immediate)
@RegisterClass(BaseAction, 'BufferCreatePostAction')
export class BufferCreatePostAction extends BufferBaseAction {
     * Create a Buffer post
            // Get parameters
            const companyIntegrationId = this.getParamValue(Params, 'CompanyIntegrationID');
            const profileIds = this.getParamValue(Params, 'ProfileIDs');
            const content = this.getParamValue(Params, 'Content');
            const mediaFiles = this.getParamValue(Params, 'MediaFiles');
            const scheduledTime = this.getParamValue(Params, 'ScheduledTime');
            const postNow = this.getParamValue(Params, 'PostNow');
            const addToTop = this.getParamValue(Params, 'AddToTop');
            const shortenLinks = this.getParamValue(Params, 'ShortenLinks');
            const mediaLink = this.getParamValue(Params, 'MediaLink');
            const mediaDescription = this.getParamValue(Params, 'MediaDescription');
            if (!companyIntegrationId) {
                throw new Error('CompanyIntegrationID is required');
            if (!profileIds || !Array.isArray(profileIds) || profileIds.length === 0) {
                throw new Error('ProfileIDs array is required with at least one profile');
            if (!content && !mediaFiles && !mediaLink) {
                throw new Error('Content, MediaFiles, or MediaLink is required');
            // Initialize OAuth
            if (!await this.initializeOAuth(companyIntegrationId)) {
                    ResultCode: 'INVALID_TOKEN',
                    Message: 'Failed to initialize Buffer OAuth connection',
            // Prepare media array
            const media: any[] = [];
            // Upload media files if provided
            if (mediaFiles && Array.isArray(mediaFiles) && mediaFiles.length > 0) {
                const uploadedUrls = await this.uploadMedia(mediaFiles as MediaFile[]);
                uploadedUrls.forEach(url => {
                    media.push({ picture: url });
            // Add media link if provided
            if (mediaLink) {
                const mediaItem: any = { link: mediaLink };
                if (mediaDescription) {
                    mediaItem.description = mediaDescription;
                media.push(mediaItem);
            // Create the update
            const result = await this.createUpdate(
                profileIds,
                content || '',
                media.length > 0 ? media : undefined,
                scheduledTime ? new Date(scheduledTime) : undefined,
                    now: postNow === true,
                    top: addToTop === true,
                    shorten: shortenLinks !== false // Default to true
            // Format the response
            const createdPosts = result.updates || [];
            const formattedPosts = createdPosts.map((update: any) => ({
                id: update.id,
                profileId: update.profile_id,
                service: update.profile_service,
                status: update.status,
                scheduledAt: update.due_at ? new Date(update.due_at * 1000) : null,
                text: update.text,
                media: update.media,
                createdAt: update.created_at ? new Date(update.created_at * 1000) : new Date()
                totalCreated: formattedPosts.length,
                profilesPosted: profileIds,
                scheduled: !postNow,
                scheduledTime: scheduledTime || null,
                hasMedia: media.length > 0
            const postsParam = outputParams.find(p => p.Name === 'CreatedPosts');
            if (postsParam) postsParam.Value = formattedPosts;
                Message: `Successfully created ${formattedPosts.length} Buffer post(s)`,
            const resultCode = this.mapBufferError(error);
                Message: `Failed to create Buffer post: ${errorMessage}`,
            ...this.commonSocialParams,
                Name: 'ProfileIDs',
                Name: 'MediaFiles',
                Name: 'MediaLink',
                Name: 'MediaDescription',
                Name: 'ScheduledTime',
                Name: 'PostNow',
                Name: 'AddToTop',
                Name: 'ShortenLinks',
                Name: 'CreatedPosts',
        return 'Creates a new post in Buffer that can be scheduled or posted immediately to one or more social media profiles';
import { FacebookBaseAction, CreatePostData } from '../facebook-base.action';
 * Creates a new post on a Facebook page.
 * Supports text, links, images, videos, and scheduling.
@RegisterClass(BaseAction, 'FacebookCreatePostAction') 
export class FacebookCreatePostAction extends FacebookBaseAction {
        return 'Creates a new post on a Facebook page with optional media attachments and scheduling';
                Name: 'Link',
                Name: 'PlaceID',
                Name: 'Published',
            const content = this.getParamValue(Params, 'Content') as string;
            const link = this.getParamValue(Params, 'Link') as string;
            const mediaFiles = this.getParamValue(Params, 'MediaFiles') as MediaFile[];
            const scheduledTime = this.getParamValue(Params, 'ScheduledTime') as string;
            const tags = this.getParamValue(Params, 'Tags') as string[];
            const placeId = this.getParamValue(Params, 'PlaceID') as string;
            const published = this.getParamValue(Params, 'Published') !== false;
            // Validate that we have some content
            if (!content && !link && (!mediaFiles || mediaFiles.length === 0)) {
                Message: 'At least one of Content, Link, or MediaFiles is required',
                ResultCode: 'MISSING_CONTENT'
            // Build post data
            const postData: CreatePostData = {};
                postData.message = content;
            if (link) {
                postData.link = link;
            if (placeId) {
                postData.place = placeId;
            if (tags && tags.length > 0) {
                postData.tags = tags;
            if (privacy) {
                postData.privacy = {
                    value: privacy as 'EVERYONE' | 'ALL_FRIENDS' | 'FRIENDS_OF_FRIENDS' | 'SELF'
            postData.published = published;
            // Handle scheduling
            if (scheduledTime) {
                const scheduledDate = new Date(scheduledTime);
                const minScheduleTime = new Date(now.getTime() + 10 * 60 * 1000); // 10 minutes from now
                const maxScheduleTime = new Date(now.getTime() + 180 * 24 * 60 * 60 * 1000); // 6 months from now
                if (scheduledDate < minScheduleTime) {
                Message: 'Scheduled time must be at least 10 minutes in the future',
                ResultCode: 'INVALID_SCHEDULE_TIME'
                if (scheduledDate > maxScheduleTime) {
                Message: 'Scheduled time cannot be more than 6 months in the future',
                postData.scheduled_publish_time = Math.floor(scheduledDate.getTime() / 1000);
                postData.published = false; // Must be unpublished when scheduling
            // Handle media uploads
            if (mediaFiles && mediaFiles.length > 0) {
                LogStatus(`Uploading ${mediaFiles.length} media files to Facebook...`);
                const mediaIds: string[] = [];
                for (const file of mediaFiles) {
                        const mediaId = await this.uploadMediaToPage(pageId, file);
                        mediaIds.push(mediaId);
                        LogStatus(`Uploaded media: ${file.filename}`);
                        LogError(`Failed to upload media ${file.filename}: ${error}`);
                Message: `Failed to upload media: ${error instanceof Error ? error.message : 'Unknown error'}`,
                // Attach media to post
                postData.attached_media = mediaIds.map(id => ({ media_fbid: id }));
            // Create the post
            LogStatus('Creating Facebook post...');
            const post = await this.createPost(pageId, postData);
            LogStatus(`Facebook post created successfully: ${post.id}`);
            // Return normalized post data
            const normalizedPost = this.normalizePost(post);
            // TODO: Set output parameters based on result
                Message: scheduledTime ? 'Post scheduled successfully' : 'Post created successfully',
            LogError(`Failed to create Facebook post: ${error instanceof Error ? error.message : 'Unknown error'}`);
import { InstagramBaseAction } from '../instagram-base.action';
 * Creates a new Instagram post (feed post, carousel, or reel).
 * Supports images and videos with captions, hashtags, and location tagging.
@RegisterClass(BaseAction, 'Instagram - Create Post')
export class InstagramCreatePostAction extends InstagramBaseAction {
            const companyIntegrationId = this.getParamValue(params.Params, 'CompanyIntegrationID');
            const content = this.getParamValue(params.Params, 'Content');
            const mediaFiles = this.getParamValue(params.Params, 'MediaFiles') as MediaFile[];
            const postType = this.getParamValue(params.Params, 'PostType') || 'FEED';
            const locationId = this.getParamValue(params.Params, 'LocationID');
            const taggedUsers = this.getParamValue(params.Params, 'TaggedUsers') as string[];
            const scheduledTime = this.getParamValue(params.Params, 'ScheduledTime');
                    Message: 'Failed to initialize Instagram authentication',
                    ResultCode: 'AUTH_FAILED'
            if (!mediaFiles || mediaFiles.length === 0) {
                    Message: 'At least one media file is required for Instagram posts',
                    ResultCode: 'MISSING_MEDIA'
            // Instagram has specific requirements for different post types
            if (postType === 'CAROUSEL' && mediaFiles.length < 2) {
                    Message: 'Carousel posts require at least 2 media files',
                    ResultCode: 'INVALID_CAROUSEL'
            if (postType === 'REELS' && (!mediaFiles[0].mimeType.startsWith('video/') || mediaFiles.length > 1)) {
                    Message: 'Reels require exactly one video file',
                    ResultCode: 'INVALID_REEL'
            // Handle scheduled posts differently
                const scheduleDate = new Date(scheduledTime);
                if (scheduleDate <= new Date()) {
                        Message: 'Scheduled time must be in the future',
                // Instagram requires Facebook Creator Studio for scheduling
                    Message: 'Instagram post scheduling requires Facebook Creator Studio integration',
                    ResultCode: 'SCHEDULING_NOT_SUPPORTED'
            let postId: string;
            if (postType === 'CAROUSEL') {
                postId = await this.createCarouselPost(mediaFiles, content, locationId, taggedUsers);
            } else if (postType === 'REELS') {
                postId = await this.createReelPost(mediaFiles[0], content, locationId, taggedUsers);
                postId = await this.createFeedPost(mediaFiles[0], content, locationId, taggedUsers);
            // Store result in output params
            const outputParams = [...params.Params];
                Value: postId
                Name: 'Permalink',
                Value: `https://www.instagram.com/p/${postId}/`
                Name: 'PostType',
                Value: postType
                Message: `Instagram ${postType.toLowerCase()} created successfully`,
            LogError('Failed to create Instagram post', error);
            if (error.code === 'RATE_LIMIT') {
                    Message: 'Instagram API rate limit exceeded. Please try again later.',
                    ResultCode: 'RATE_LIMIT'
            if (error.code === 'INVALID_MEDIA') {
                    Message: error.message,
                Message: `Failed to create Instagram post: ${error.message}`,
     * Create a standard feed post (single image or video)
    private async createFeedPost(
        mediaFile: MediaFile,
        locationId?: string,
        taggedUsers?: string[]
        // Add metadata to media file
        (mediaFile as any).metadata = { caption };
        // Upload media and get container ID
        const containerId = await this.uploadSingleMedia(mediaFile);
        // Wait for media to be processed
        await this.waitForMediaContainer(containerId);
        // Add additional parameters if provided
        const publishParams: any = {
        if (locationId) {
            publishParams.location_id = locationId;
        if (taggedUsers && taggedUsers.length > 0) {
            publishParams.user_tags = taggedUsers.map(userId => ({
                username: userId,
                x: 0.5, // Center of image
                y: 0.5
        // Publish the post
            publishParams
     * Create a carousel post (multiple images/videos)
    private async createCarouselPost(
        mediaFiles: MediaFile[],
        // Upload all media items as carousel items
        const containerIds: string[] = [];
        for (const file of mediaFiles.slice(0, 10)) { // Instagram allows max 10 items
            file.filename = `carousel_${file.filename}`; // Mark as carousel item
            const containerId = await this.uploadSingleMedia(file);
            containerIds.push(containerId);
        // Wait for all media to be processed
        await Promise.all(containerIds.map(id => this.waitForMediaContainer(id)));
        // Create carousel container
        const carouselParams: any = {
            media_type: 'CAROUSEL',
            children: containerIds,
            caption: caption,
            carouselParams.location_id = locationId;
        const carouselResponse = await this.makeInstagramRequest<{ id: string }>(
            carouselParams
        // Wait for carousel container to be ready
        await this.waitForMediaContainer(carouselResponse.id);
        // Publish the carousel
        const publishResponse = await this.makeInstagramRequest<{ id: string }>(
                creation_id: carouselResponse.id,
        return publishResponse.id;
     * Create a Reel post
    private async createReelPost(
        videoFile: MediaFile,
        // Validate video duration (Reels must be 90 seconds or less)
        // In production, you'd check the actual video duration
        (videoFile as any).metadata = { 
            media_type: 'REELS'
        // Upload video
        const containerId = await this.uploadSingleMedia(videoFile);
        // Wait for video to be processed (videos take longer)
        await this.waitForMediaContainer(containerId, 300000); // 5 minute timeout for videos
        // Publish the reel
                Value: 'FEED',
                Name: 'LocationID',
                Name: 'TaggedUsers',
     * Get the description for this action
        return 'Creates a new Instagram post with images or videos. Supports feed posts, carousels, and reels.';
import { LinkedInBaseAction, LinkedInShareData } from '../linkedin-base.action';
 * Action to create a post on LinkedIn
@RegisterClass(BaseAction, 'LinkedInCreatePostAction')
export class LinkedInCreatePostAction extends LinkedInBaseAction {
     * Create a post on LinkedIn (personal or organization)
            const visibleToGuest = this.getParamValue(Params, 'VisibleToGuest') !== false; // Default true
                // Personal post
            let mediaUrns: string[] = [];
                mediaUrns = await this.uploadMedia(mediaFiles as MediaFile[]);
            // Build share data
                        shareMediaCategory: mediaUrns.length > 0 ? 'IMAGE' : 'NONE',
                        media: mediaUrns.length > 0 ? mediaUrns.map(urn => ({
                            media: urn
                        })) : undefined
            // Add distribution settings if public
            if (visibility === 'PUBLIC') {
                shareData.distribution = {
                    linkedInDistributionTarget: {
                        visibleToGuest: visibleToGuest
            LogStatus('Creating LinkedIn post...');
            // Get the created post details
            const shares = await this.getShares(authorUrn, 1);
            const createdPost = shares.find(s => s.id === postId);
            if (!createdPost) {
                throw new Error('Failed to retrieve created post');
            if (postIdParam) postIdParam.Value = postId;
                Message: `Successfully created LinkedIn post (ID: ${postId})`,
                Message: `Failed to create LinkedIn post: ${errorMessage}`,
                Name: 'VisibleToGuest',
        return 'Creates a post on LinkedIn for personal profiles or organization pages with optional media attachments';
