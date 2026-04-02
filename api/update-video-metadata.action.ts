 * Action to update metadata for a YouTube video
@RegisterClass(BaseAction, 'YouTubeUpdateVideoMetadataAction')
export class YouTubeUpdateVideoMetadataAction extends YouTubeBaseAction {
     * Update video metadata on YouTube
            const categoryId = this.getParamValue(Params, 'CategoryID');
            const privacyStatus = this.getParamValue(Params, 'PrivacyStatus');
            const embeddable = this.getParamValue(Params, 'Embeddable');
            const publicStatsViewable = this.getParamValue(Params, 'PublicStatsViewable');
            const license = this.getParamValue(Params, 'License');
            const recordingDate = this.getParamValue(Params, 'RecordingDate');
            const thumbnailFile = this.getParamValue(Params, 'ThumbnailFile');
            // First, get the current video details to preserve unchanged fields
            const currentSnippet = video.snippet;
            const currentStatus = video.status;
            // Build update request with only changed fields
                    title: title !== undefined ? title : currentSnippet.title,
                    description: description !== undefined ? description : currentSnippet.description,
                    tags: tags !== undefined ? tags : currentSnippet.tags,
                    categoryId: categoryId !== undefined ? categoryId : currentSnippet.categoryId
                    privacyStatus: privacyStatus !== undefined ? privacyStatus : currentStatus.privacyStatus,
                    embeddable: embeddable !== undefined ? embeddable : currentStatus.embeddable,
                    publicStatsViewable: publicStatsViewable !== undefined ? publicStatsViewable : currentStatus.publicStatsViewable,
                    selfDeclaredMadeForKids: currentStatus.selfDeclaredMadeForKids // Required field
            // Add optional fields if provided
            if (publishAt) {
                updateData.status.publishAt = this.formatDate(publishAt);
            if (license) {
                updateData.status.license = license;
            if (recordingDate) {
                updateData.recordingDetails = {
                    recordingDate: this.formatDate(recordingDate)
                    part: 'snippet,status' + (recordingDate ? ',recordingDetails' : '')
            // Update thumbnail if provided
            if (thumbnailFile) {
                    await this.uploadThumbnail(videoId, thumbnailFile);
                    // Log thumbnail upload failure but don't fail the entire operation
                    console.error('Failed to update thumbnail:', error);
                    part: 'snippet,status,statistics,contentDetails',
                updatedFields: this.getUpdatedFields(currentSnippet, currentStatus, finalVideo.snippet, finalVideo.status),
                description: finalVideo.snippet.description,
                tags: finalVideo.snippet.tags,
                categoryId: finalVideo.snippet.categoryId,
                publishAt: finalVideo.status.publishAt,
                embeddable: finalVideo.status.embeddable,
                publicStatsViewable: finalVideo.status.publicStatsViewable,
                statistics: finalVideo.statistics,
            const videoDetailsParam = outputParams.find(p => p.Name === 'VideoDetails');
            if (videoDetailsParam) videoDetailsParam.Value = result;
            const updatedFieldsParam = outputParams.find(p => p.Name === 'UpdatedFields');
            if (updatedFieldsParam) updatedFieldsParam.Value = result.updatedFields;
                Message: `Video metadata updated successfully. ${result.updatedFields.length} fields changed.`,
                Message: `Failed to update video metadata: ${errorMessage}`,
     * Upload thumbnail for a video
    private async uploadThumbnail(videoId: string, thumbnailFile: any): Promise<void> {
        const thumbnailData = Buffer.isBuffer(thumbnailFile.data) 
            ? thumbnailFile.data 
            : Buffer.from(thumbnailFile.data, 'base64');
        // Validate thumbnail
        const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/bmp'];
        if (!allowedTypes.includes(thumbnailFile.mimeType)) {
            throw new Error('Invalid thumbnail format. Supported formats: JPEG, PNG, GIF, BMP');
        const maxSize = 2 * 1024 * 1024; // 2MB
        if (thumbnailData.length > maxSize) {
            throw new Error(`Thumbnail too large. Maximum size is 2MB`);
        await this.makeYouTubeRequest(
            '/thumbnails/set',
            thumbnailData,
     * Determine which fields were updated
    private getUpdatedFields(oldSnippet: any, oldStatus: any, newSnippet: any, newStatus: any): string[] {
        const updated: string[] = [];
        if (oldSnippet.title !== newSnippet.title) updated.push('title');
        if (oldSnippet.description !== newSnippet.description) updated.push('description');
        if (JSON.stringify(oldSnippet.tags) !== JSON.stringify(newSnippet.tags)) updated.push('tags');
        if (oldSnippet.categoryId !== newSnippet.categoryId) updated.push('categoryId');
        if (oldStatus.privacyStatus !== newStatus.privacyStatus) updated.push('privacyStatus');
        if (oldStatus.embeddable !== newStatus.embeddable) updated.push('embeddable');
        if (oldStatus.publicStatsViewable !== newStatus.publicStatsViewable) updated.push('publicStatsViewable');
        if (oldStatus.publishAt !== newStatus.publishAt) updated.push('publishAt');
        return updated;
                Name: 'Embeddable',
                Name: 'PublicStatsViewable',
                Name: 'License',
                Name: 'RecordingDate',
                Name: 'ThumbnailFile',
                Name: 'VideoDetails',
        return 'Updates metadata for a YouTube video including title, description, tags, privacy settings, and thumbnail';
