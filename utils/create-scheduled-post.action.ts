import { HootSuiteBaseAction } from '../hootsuite-base.action';
 * Action to create a scheduled post in HootSuite
@RegisterClass(BaseAction, 'HootSuiteCreateScheduledPostAction')
export class HootSuiteCreateScheduledPostAction extends HootSuiteBaseAction {
     * Create a scheduled post in HootSuite
            const location = this.getParamValue(Params, 'Location');
            const targetingOptions = this.getParamValue(Params, 'TargetingOptions');
            if (!content) {
                throw new Error('Content is required');
            // Get profile IDs - either from parameter or default profiles
            let socialProfileIds: string[] = [];
            if (profileIds && Array.isArray(profileIds)) {
                socialProfileIds = profileIds;
            } else if (profileIds && typeof profileIds === 'string') {
                socialProfileIds = [profileIds];
                // Get default profiles if none specified
                    throw new Error('No social profiles found. Please specify ProfileIDs.');
                socialProfileIds = profiles.map(p => p.id);
                LogStatus(`Using ${socialProfileIds.length} default profiles`);
            if (mediaFiles && Array.isArray(mediaFiles)) {
                LogStatus(`Uploading ${mediaFiles.length} media files...`);
                mediaIds = await this.uploadMedia(mediaFiles as MediaFile[]);
                text: content,
                socialProfileIds: socialProfileIds,
                tags: tags && Array.isArray(tags) ? tags : undefined,
                location: location ? {
                    latitude: location.latitude,
                    longitude: location.longitude
            // Add targeting options if provided
            if (targetingOptions) {
                postData.targeting = targetingOptions;
            LogStatus('Creating scheduled post...');
            // Normalize the created post
            const normalizedPost = this.normalizePost(createdPost);
            const postParam = outputParams.find(p => p.Name === 'CreatedPost');
            if (postParam) postParam.Value = normalizedPost;
            const postIdParam = outputParams.find(p => p.Name === 'PostID');
            if (postIdParam) postIdParam.Value = createdPost.id;
                Message: `Successfully created scheduled post (ID: ${createdPost.id})`,
                Message: `Failed to create scheduled post: ${errorMessage}`,
     * Validate media files before upload
        // HootSuite-specific media limits
        const supportedTypes = [
            'video/quicktime'
        // Check file sizes
            'image/jpeg': 10 * 1024 * 1024,   // 10MB
            'image/png': 10 * 1024 * 1024,    // 10MB
            'image/gif': 15 * 1024 * 1024,    // 15MB
            'video/mp4': 500 * 1024 * 1024,   // 500MB
            'video/quicktime': 500 * 1024 * 1024 // 500MB
            throw new Error(`File size exceeds limit for ${file.mimeType}. Max: ${maxSize / 1024 / 1024}MB, Got: ${file.size / 1024 / 1024}MB`);
                Name: 'TargetingOptions',
                Name: 'CreatedPost',
        return 'Creates a scheduled post in HootSuite with support for multiple social profiles, media attachments, and scheduling';
