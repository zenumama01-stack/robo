import { HootSuiteBaseAction, HootSuitePost } from '../hootsuite-base.action';
 * Interface for bulk post data
interface BulkPostData {
    mediaFiles?: MediaFile[];
    location?: { latitude: number; longitude: number; };
 * Action to bulk schedule multiple posts in HootSuite
@RegisterClass(BaseAction, 'HootSuiteBulkSchedulePostsAction')
export class HootSuiteBulkSchedulePostsAction extends HootSuiteBaseAction {
     * Bulk schedule posts in HootSuite
                throw new Error('Failed to initialize OAuth connection');
            const posts = this.getParamValue(Params, 'Posts') as BulkPostData[];
            const defaultProfileIds = this.getParamValue(Params, 'DefaultProfileIDs');
            const scheduleInterval = this.getParamValue(Params, 'ScheduleInterval');
            const startTime = this.getParamValue(Params, 'StartTime');
            const skipOnError = this.getParamValue(Params, 'SkipOnError') || true;
            const validateOnly = this.getParamValue(Params, 'ValidateOnly') || false;
            if (!posts || !Array.isArray(posts) || posts.length === 0) {
                throw new Error('Posts array is required and must not be empty');
            // Get default profiles if not specified
            let defaultProfiles: string[] = [];
            if (defaultProfileIds && Array.isArray(defaultProfileIds)) {
                defaultProfiles = defaultProfileIds;
            } else if (defaultProfileIds && typeof defaultProfileIds === 'string') {
                defaultProfiles = [defaultProfileIds];
                // Get all available profiles as default
                const profiles = await this.getSocialProfiles();
                if (profiles.length === 0) {
                    throw new Error('No social profiles found. Please specify DefaultProfileIDs.');
                defaultProfiles = profiles.map(p => p.id);
                LogStatus(`Using ${defaultProfiles.length} default profiles`);
            // Calculate schedule times if interval is specified
            let baseScheduleTime: Date | null = null;
            if (scheduleInterval && startTime) {
                baseScheduleTime = new Date(startTime);
            // Process posts
            LogStatus(`Processing ${posts.length} posts for bulk scheduling...`);
            for (let i = 0; i < posts.length; i++) {
                const post = posts[i];
                const postIndex = i + 1;
                    // Validate post data
                    this.validateBulkPost(post, postIndex);
                    // Calculate scheduled time for this post
                    let scheduledTime = post.scheduledTime;
                    if (!scheduledTime && baseScheduleTime && scheduleInterval) {
                        const offsetMinutes = i * scheduleInterval;
                        const postScheduleTime = new Date(baseScheduleTime);
                        postScheduleTime.setMinutes(postScheduleTime.getMinutes() + offsetMinutes);
                        scheduledTime = postScheduleTime.toISOString();
                    // Prepare profile IDs
                    const profileIds = post.profileIds || defaultProfiles;
                    // If validate only, just check the data
                    if (validateOnly) {
                            index: postIndex,
                            status: 'VALIDATED',
                            content: post.content.substring(0, 50) + '...',
                            scheduledTime: scheduledTime,
                            profileCount: profileIds.length
                    // Upload media if provided
                    if (post.mediaFiles && Array.isArray(post.mediaFiles)) {
                        LogStatus(`Uploading ${post.mediaFiles.length} media files for post ${postIndex}...`);
                        mediaIds = await this.uploadMedia(post.mediaFiles);
                    const postData: any = {
                        text: post.content,
                        socialProfileIds: profileIds,
                        scheduledTime: scheduledTime ? this.formatHootSuiteDate(scheduledTime) : undefined,
                        mediaIds: mediaIds.length > 0 ? mediaIds : undefined,
                        tags: post.tags,
                        location: post.location
                    const response = await this.axiosInstance.post('/messages', postData);
                    const createdPost = response.data;
                        postId: createdPost.id,
                        scheduledTime: createdPost.scheduledTime,
                    errors.push({
                    if (!skipOnError) {
                        throw new Error(`Failed at post ${postIndex}: ${errorMessage}`);
                // Log progress every 10 posts
                if (i > 0 && (i + 1) % 10 === 0) {
                    LogStatus(`Processed ${i + 1}/${posts.length} posts...`);
                validationOnly: validateOnly,
                processingTime: new Date().toISOString(),
                scheduleRange: baseScheduleTime ? {
                    start: baseScheduleTime.toISOString(),
                    end: new Date(baseScheduleTime.getTime() + (posts.length - 1) * (scheduleInterval || 0) * 60000).toISOString()
            const errorsParam = outputParams.find(p => p.Name === 'Errors');
            if (errorsParam) errorsParam.Value = errors;
            const resultCode = failureCount === 0 ? 'SUCCESS' : (successCount > 0 ? 'PARTIAL_SUCCESS' : 'FAILED');
            const message = validateOnly 
                ? `Validated ${successCount} posts, ${failureCount} failed validation`
                : `Successfully scheduled ${successCount} posts, ${failureCount} failed`;
                Success: failureCount === 0 || (skipOnError && successCount > 0),
                Message: `Failed to bulk schedule posts: ${errorMessage}`,
     * Validate bulk post data
    private validateBulkPost(post: BulkPostData, index: number): void {
        if (!post.content || typeof post.content !== 'string' || post.content.trim().length === 0) {
            throw new Error(`Post ${index}: Content is required and must not be empty`);
        if (post.content.length > 10000) {
            throw new Error(`Post ${index}: Content exceeds maximum length of 10000 characters`);
        if (post.scheduledTime && isNaN(Date.parse(post.scheduledTime))) {
            throw new Error(`Post ${index}: Invalid scheduled time format`);
        if (post.location) {
            if (typeof post.location.latitude !== 'number' || typeof post.location.longitude !== 'number') {
                throw new Error(`Post ${index}: Location must have numeric latitude and longitude`);
                Name: 'DefaultProfileIDs',
                Name: 'ScheduleInterval',
                Name: 'SkipOnError',
                Name: 'ValidateOnly',
                Name: 'Errors',
        return 'Bulk schedules multiple posts to HootSuite with support for auto-scheduling intervals and validation';
