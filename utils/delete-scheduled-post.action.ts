 * Action to delete a scheduled post from HootSuite
@RegisterClass(BaseAction, 'HootSuiteDeleteScheduledPostAction')
export class HootSuiteDeleteScheduledPostAction extends HootSuiteBaseAction {
     * Delete a scheduled post from HootSuite
            const confirmDeletion = this.getParamValue(Params, 'ConfirmDeletion') || false;
                throw new Error('PostID is required');
            // Safety check - require confirmation
            if (!confirmDeletion) {
                    ResultCode: 'CONFIRMATION_REQUIRED',
                    Message: 'Deletion not confirmed. Set ConfirmDeletion to true to proceed.',
            // First, get the post details to verify it exists and can be deleted
            LogStatus(`Fetching post ${postId} details...`);
            let postDetails: any;
                const response = await this.axiosInstance.get(`/messages/${postId}`);
                postDetails = response.data;
                if (error.response?.status === 404) {
                        ResultCode: 'POST_NOT_FOUND',
                        Message: `Post with ID ${postId} not found`,
            // Check if post can be deleted
            if (postDetails.state === 'PUBLISHED') {
                    ResultCode: 'CANNOT_DELETE_PUBLISHED',
                    Message: 'Cannot delete published posts. Only scheduled, draft, or failed posts can be deleted.',
            // Store post details before deletion
            const deletedPostInfo = {
                id: postDetails.id,
                content: postDetails.text,
                state: postDetails.state,
                scheduledTime: postDetails.scheduledTime,
                profiles: postDetails.socialProfileIds,
                mediaCount: postDetails.mediaIds?.length || 0,
                tags: postDetails.tags || [],
                deletedAt: new Date().toISOString()
            // Delete the post
            LogStatus(`Deleting post ${postId}...`);
            await this.axiosInstance.delete(`/messages/${postId}`);
            // Verify deletion by trying to fetch the post again
            let verificationFailed = false;
                await this.axiosInstance.get(`/messages/${postId}`);
                verificationFailed = true;
                if (error.response?.status !== 404) {
            if (verificationFailed) {
                LogStatus('Warning: Post deletion could not be verified');
            const deletedPostParam = outputParams.find(p => p.Name === 'DeletedPostInfo');
            if (deletedPostParam) deletedPostParam.Value = deletedPostInfo;
            const deletionVerifiedParam = outputParams.find(p => p.Name === 'DeletionVerified');
            if (deletionVerifiedParam) deletionVerifiedParam.Value = !verificationFailed;
                Message: `Successfully deleted post ${postId}`,
            if (error instanceof Error && error.message.includes('404')) {
                    Message: `Post ${this.getParamValue(Params, 'PostID')} not found or already deleted`,
                Message: `Failed to delete scheduled post: ${errorMessage}`,
                Name: 'ConfirmDeletion',
                Name: 'DeletedPostInfo',
                Name: 'DeletionVerified',
        return 'Deletes a scheduled post from HootSuite. Only SCHEDULED, DRAFT, or FAILED posts can be deleted. Published posts cannot be deleted.';
