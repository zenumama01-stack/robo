 * Schedules a post to be published on a Facebook page at a future time.
 * Posts can be scheduled from 10 minutes to 6 months in the future.
@RegisterClass(BaseAction, 'FacebookSchedulePostAction')
export class FacebookSchedulePostAction extends FacebookBaseAction {
        return 'Schedules a post to be published on a Facebook page at a specified future time (10 minutes to 6 months in advance)';
                Name: 'TargetingRestrictions',
                Name: 'AllowReschedule',
            if (!scheduledTime) {
                Message: 'ScheduledTime is required',
            // Validate scheduled time
            if (isNaN(scheduledDate.getTime())) {
                Message: 'Invalid scheduled time format. Use ISO 8601 format.',
            const targetingRestrictions = this.getParamValue(Params, 'TargetingRestrictions') as any;
            const allowReschedule = this.getParamValue(Params, 'AllowReschedule') as boolean;
            // Check for scheduling conflicts if requested
            if (!allowReschedule) {
                const hasConflict = await this.checkSchedulingConflict(pageId, scheduledDate);
                if (hasConflict) {
                Message: 'Another post is already scheduled within 5 minutes of this time',
                ResultCode: 'SCHEDULE_CONFLICT'
            const postData: CreatePostData = {
                scheduled_publish_time: Math.floor(scheduledDate.getTime() / 1000),
                published: false // Must be false for scheduled posts
            if (targetingRestrictions) {
                (postData as any).targeting = targetingRestrictions;
                LogStatus(`Uploading ${mediaFiles.length} media files for scheduled post...`);
            // Create the scheduled post
            LogStatus(`Scheduling Facebook post for ${scheduledDate.toISOString()}...`);
            // Get the scheduled post details
            const scheduledPost = await this.getScheduledPost(pageId, post.id);
            LogStatus(`Facebook post scheduled successfully: ${post.id}`);
                Message: `Post scheduled for ${scheduledDate.toISOString()}`,
            LogError(`Failed to schedule Facebook post: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Check if there's a scheduling conflict within 5 minutes
    private async checkSchedulingConflict(pageId: string, scheduledTime: Date): Promise<boolean> {
            const fiveMinutesBefore = new Date(scheduledTime.getTime() - 5 * 60 * 1000);
            const fiveMinutesAfter = new Date(scheduledTime.getTime() + 5 * 60 * 1000);
            // Get scheduled posts in the time window
            const response = await axios.get(`${this.apiBaseUrl}/${pageId}/scheduled_posts`, {
                    since: Math.floor(fiveMinutesBefore.getTime() / 1000),
                    until: Math.floor(fiveMinutesAfter.getTime() / 1000),
                    limit: 10
            const scheduledPosts = response.data.data || [];
            return scheduledPosts.length > 0;
            LogError(`Failed to check scheduling conflicts: ${error}`);
            return false; // Don't block on error
     * Get details of a scheduled post
    private async getScheduledPost(pageId: string, postId: string): Promise<any> {
            const response = await axios.get(`${this.apiBaseUrl}/${postId}`, {
                    fields: 'id,message,scheduled_publish_time,is_published'
            LogError(`Failed to get scheduled post details: ${error}`);
 * Schedules an Instagram post for future publication.
 * Note: Instagram API has limited native scheduling support. This action provides
 * scheduling information that needs to be integrated with Facebook Creator Studio
 * or third-party scheduling tools.
@RegisterClass(BaseAction, 'Instagram - Schedule Post')
export class InstagramSchedulePostAction extends InstagramBaseAction {
            const mediaUrls = this.getParamValue(params.Params, 'MediaUrls') as string[];
            const firstComment = this.getParamValue(params.Params, 'FirstComment');
            if (!mediaUrls || mediaUrls.length === 0) {
                    Message: 'At least one media URL is required',
            if (scheduleDate <= now) {
            // Instagram requires scheduling at least 10 minutes in the future
            const minScheduleTime = new Date(now.getTime() + 10 * 60 * 1000);
            if (scheduleDate < minScheduleTime) {
                    Message: 'Posts must be scheduled at least 10 minutes in the future',
                    ResultCode: 'SCHEDULE_TOO_SOON'
            // Instagram limits scheduling to 75 days in the future
            const maxScheduleTime = new Date(now.getTime() + 75 * 24 * 60 * 60 * 1000);
            if (scheduleDate > maxScheduleTime) {
                    Message: 'Posts cannot be scheduled more than 75 days in the future',
                    ResultCode: 'SCHEDULE_TOO_FAR'
            // Create scheduling payload
            const schedulingData = {
                scheduledTime: scheduleDate.toISOString(),
                postType,
                locationId,
                taggedUsers,
                firstComment,
                accountId: this.instagramBusinessAccountId,
                pageId: this.facebookPageId
            // Instagram doesn't have a direct scheduling API endpoint
            // Instead, we need to use Facebook's Content Publishing API
            // or integrate with Creator Studio
            // For now, we'll store the scheduling data and return instructions
            const schedulingId = this.generateSchedulingId();
            // In a production environment, you would:
            // 1. Store this in a database
            // 2. Set up a cron job or scheduler to publish at the scheduled time
            // 3. Or integrate with Facebook Creator Studio API
            // Store scheduling data (this is a placeholder - implement your storage solution)
            await this.storeSchedulingData(schedulingId, schedulingData);
                    schedulingId,
                    mediaCount: mediaUrls.length,
                        creatorStudio: 'This post can be managed in Facebook Creator Studio',
                        api: 'Use the scheduling ID to retrieve and publish this post at the scheduled time',
                        webhook: 'Set up a webhook to be notified when it\'s time to publish'
                    schedulingData
                Message: 'Instagram post scheduled successfully',
            LogError('Failed to schedule Instagram post', error);
                Message: `Failed to schedule post: ${error.message}`,
     * Generate a unique scheduling ID
    private generateSchedulingId(): string {
        const random = Math.random().toString(36).substring(2, 9);
        return `ig_schedule_${timestamp}_${random}`;
     * Store scheduling data (placeholder - implement based on your storage solution)
    private async storeSchedulingData(schedulingId: string, data: any): Promise<void> {
        // In a real implementation, this would:
        // 1. Store in a database (e.g., in a ScheduledPosts table)
        // 2. Set up a scheduled job in your job queue
        // 3. Or send to a scheduling service
        // For demonstration, we'll just log it
        LogError('Scheduling data would be stored:', schedulingId, data);
        // You could also use MemberJunction entities to store this
        // For example, create a "Social Media Scheduled Posts" entity
        // and save the scheduling data there
     * Create a scheduled post using Facebook Creator Studio API
     * Note: This is a conceptual implementation as Creator Studio API
     * has specific requirements and approval process
    private async createCreatorStudioScheduledPost(data: any): Promise<string> {
        // This would require:
        // 1. Creator Studio API access
        // 2. Additional permissions
        // 3. Different API endpoints
        // For now, we return a placeholder
        throw new Error('Creator Studio integration not implemented. Use the scheduling data to create your own scheduling solution.');
                Name: 'MediaUrls',
                Value: 'FEED'
                Name: 'FirstComment',
        return 'Schedules an Instagram post for future publication. Returns scheduling data that can be used with Creator Studio or custom scheduling solutions.';
 * Action to schedule a post on LinkedIn
 * Note: LinkedIn API v2 does not have native scheduling support.
 * This action stores the post data for later publishing via a separate scheduler service.
@RegisterClass(BaseAction, 'LinkedInSchedulePostAction')
export class LinkedInSchedulePostAction extends LinkedInBaseAction {
     * Schedule a post for future publishing on LinkedIn
                throw new Error('ScheduledTime is required');
            // Validate scheduled time is in the future
            if (scheduledDate <= new Date()) {
                throw new Error('ScheduledTime must be in the future');
            // Upload media if provided (to ensure they're valid and get URNs)
            // Build share data for future publishing
            // Create a scheduled post record
            // Note: This would typically be stored in a database or queue for processing
            const scheduledPost = {
                id: `scheduled_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`,
                companyIntegrationId: companyIntegrationId,
                scheduledTime: scheduledDate.toISOString(),
                shareData: shareData,
                status: 'SCHEDULED',
                createdAt: new Date().toISOString()
            LogStatus(`Post scheduled for ${scheduledDate.toISOString()}`);
            const scheduledPostParam = outputParams.find(p => p.Name === 'ScheduledPost');
            if (scheduledPostParam) scheduledPostParam.Value = scheduledPost;
            const scheduledIdParam = outputParams.find(p => p.Name === 'ScheduledID');
            if (scheduledIdParam) scheduledIdParam.Value = scheduledPost.id;
                Message: `Successfully scheduled LinkedIn post for ${scheduledDate.toISOString()}`,
                Message: `Failed to schedule LinkedIn post: ${errorMessage}`,
                Name: 'ScheduledPost',
                Name: 'ScheduledID',
        return 'Schedules a post for future publishing on LinkedIn. Note: LinkedIn API does not support native scheduling, so this stores the post for later publishing via a scheduler service.';
