 * Action to update a scheduled post in HootSuite
@RegisterClass(BaseAction, 'HootSuiteUpdateScheduledPostAction')
export class HootSuiteUpdateScheduledPostAction extends HootSuiteBaseAction {
     * Update a scheduled post in HootSuite
            const replaceMedia = this.getParamValue(Params, 'ReplaceMedia') || false;
            // First, get the existing post
            LogStatus(`Fetching existing post ${postId}...`);
            const existingPostResponse = await this.axiosInstance.get(`/messages/${postId}`);
            const existingPost = existingPostResponse.data;
            // Check if post can be updated
            if (existingPost.state !== 'SCHEDULED' && existingPost.state !== 'DRAFT') {
                throw new Error(`Cannot update post in ${existingPost.state} state. Only SCHEDULED and DRAFT posts can be updated.`);
            // Build update data
            const updateData: any = {};
            // Update content if provided
            if (content !== undefined && content !== null) {
                updateData.text = content;
            // Update profile IDs if provided
            if (profileIds) {
                if (Array.isArray(profileIds)) {
                    updateData.socialProfileIds = profileIds;
                } else if (typeof profileIds === 'string') {
                    updateData.socialProfileIds = [profileIds];
            // Update scheduled time if provided
            if (scheduledTime !== undefined) {
                updateData.scheduledTime = scheduledTime ? this.formatHootSuiteDate(scheduledTime) : null;
            // Handle media updates
            if (mediaFiles !== undefined) {
                if (replaceMedia || !existingPost.mediaIds || existingPost.mediaIds.length === 0) {
                    // Replace all media or add new media
                        const mediaIds = await this.uploadMedia(mediaFiles as MediaFile[]);
                        updateData.mediaIds = mediaIds;
                        // Remove all media
                        updateData.mediaIds = [];
                } else if (mediaFiles && Array.isArray(mediaFiles) && mediaFiles.length > 0) {
                    // Append new media to existing
                    LogStatus(`Uploading ${mediaFiles.length} additional media files...`);
                    const newMediaIds = await this.uploadMedia(mediaFiles as MediaFile[]);
                    updateData.mediaIds = [...(existingPost.mediaIds || []), ...newMediaIds];
            // Update tags if provided
            if (tags !== undefined) {
                updateData.tags = Array.isArray(tags) ? tags : [];
            // Update location if provided
            if (location !== undefined) {
                updateData.location = location ? {
            // Only proceed if there are updates
            if (Object.keys(updateData).length === 0) {
                    Message: 'No updates were provided',
            // Update the post
            LogStatus(`Updating post ${postId}...`);
            const response = await this.axiosInstance.patch(`/messages/${postId}`, updateData);
            const updatedPost = response.data;
            // Normalize the updated post
            const normalizedPost = this.normalizePost(updatedPost);
            const postParam = outputParams.find(p => p.Name === 'UpdatedPost');
            const changesSummaryParam = outputParams.find(p => p.Name === 'ChangesSummary');
            if (changesSummaryParam) changesSummaryParam.Value = {
                fieldsUpdated: Object.keys(updateData),
                previousState: existingPost.state,
                newState: updatedPost.state,
                mediaChanges: {
                    before: existingPost.mediaIds?.length || 0,
                    after: updatedPost.mediaIds?.length || 0
                Message: `Successfully updated scheduled post (ID: ${postId})`,
                Message: `Failed to update scheduled post: ${errorMessage}`,
                Name: 'ReplaceMedia',
                Name: 'UpdatedPost',
                Name: 'ChangesSummary',
        return 'Updates a scheduled post in HootSuite. Only SCHEDULED and DRAFT posts can be updated.';
