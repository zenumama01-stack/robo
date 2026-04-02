import { TikTokBaseAction } from '../tiktok-base.action';
 * Action to create a video post on TikTok
 * Note: This requires special API approval from TikTok
@RegisterClass(BaseAction, 'CreateVideoPostAction')
export class CreateVideoPostAction extends TikTokBaseAction {
     * Create a video post on TikTok
        const { Params } = params;
            await this.initializeOAuth(companyIntegrationId);
            const videoUrl = this.getParamValue(Params, 'VideoURL');
            const hashtags = this.getParamValue(Params, 'Hashtags') || [];
            const privacyLevel = this.getParamValue(Params, 'PrivacyLevel') || 'PUBLIC';
            const allowComments = this.getParamValue(Params, 'AllowComments') !== false;
            const allowDuet = this.getParamValue(Params, 'AllowDuet') !== false;
            const allowStitch = this.getParamValue(Params, 'AllowStitch') !== false;
            const scheduleTime = this.getParamValue(Params, 'ScheduleTime');
            if (!videoUrl && !this.getParamValue(Params, 'VideoFile')) {
                throw new Error('Either VideoURL or VideoFile is required');
            // Build description with hashtags
            const hashtagString = hashtags.map((tag: string) => 
            const fullDescription = description ? `${description} ${hashtagString}`.trim() : hashtagString;
            // Check if we have video upload approval
            const hasUploadApproval = this.getCustomAttribute(3) === 'approved';
            if (!hasUploadApproval) {
                // Return informative message about TikTok limitations
                const alternativeSteps = {
                    manualProcess: [
                        '1. Download the TikTok mobile app',
                        '2. Log in to your TikTok account',
                        '3. Tap the + button to create a new video',
                        '4. Upload your video or record a new one',
                        `5. Add title: "${title || 'Your video title'}"`,
                        `6. Add description: "${fullDescription}"`,
                        '7. Set privacy and interaction settings',
                        '8. Post immediately or save as draft'
                    businessSolution: 'For automated posting, apply for TikTok Marketing API access at https://ads.tiktok.com/marketing_api/',
                    thirdPartyTools: [
                        'TikTok Creator Studio (web interface)',
                        'Buffer (with TikTok integration)',
                        'Hootsuite (business plans)'
                // Update output parameters with alternative instructions
                if (postIdParam) postIdParam.Value = null;
                const postUrlParam = outputParams.find(p => p.Name === 'PostURL');
                if (postUrlParam) postUrlParam.Value = null;
                const alternativesParam = outputParams.find(p => p.Name === 'Alternatives');
                if (alternativesParam) alternativesParam.Value = alternativeSteps;
                    ResultCode: 'API_LIMITATION',
                    Message: 'TikTok video upload requires special API approval. See Alternatives output for manual posting instructions.',
            // If we have approval, attempt to create the post
            // This is the theoretical implementation if approval is granted
            const createPostData = {
                video_url: videoUrl,
                description: fullDescription,
                privacy_level: privacyLevel,
                allow_comments: allowComments,
                allow_duet: allowDuet,
                allow_stitch: allowStitch,
                scheduled_publish_time: scheduleTime ? new Date(scheduleTime).toISOString() : undefined
                '/v2/video/upload/',
                createPostData
            const postData = response.data;
            if (postIdParam) postIdParam.Value = postData.video_id;
            if (postUrlParam) postUrlParam.Value = postData.share_url;
            const statusParam = outputParams.find(p => p.Name === 'Status');
            if (statusParam) statusParam.Value = postData.status;
                Message: `Successfully created TikTok video post: ${postData.video_id}`,
            // Check for specific TikTok error codes
            if (errorMessage.includes('insufficient_permissions')) {
                    ResultCode: 'INSUFFICIENT_PERMISSIONS',
                    Message: 'This action requires TikTok Marketing API approval. Contact TikTok for Business.',
                ResultCode: this.isAuthError(error) ? 'INVALID_TOKEN' : 'ERROR',
                Message: `Failed to create TikTok video post: ${errorMessage}`,
                Name: 'VideoURL',
                Name: 'VideoFile',
                Value: []
                Name: 'PrivacyLevel',
                Value: 'PUBLIC'
                Name: 'AllowComments',
                Name: 'AllowDuet',
                Name: 'AllowStitch',
                Name: 'ScheduleTime',
                Name: 'PostURL',
                Name: 'Alternatives',
        return 'Creates a video post on TikTok (requires special API approval from TikTok)';
