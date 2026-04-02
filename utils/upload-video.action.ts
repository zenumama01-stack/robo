 * Action to upload a video to YouTube
@RegisterClass(BaseAction, 'YouTubeUploadVideoAction')
export class YouTubeUploadVideoAction extends YouTubeBaseAction {
            const videoFile = this.getParamValue(Params, 'VideoFile');
                throw new Error('Title is required for video upload');
            if (!videoFile) {
                throw new Error('VideoFile is required');
            // Validate video file
            const video: MediaFile = {
                filename: videoFile.filename || 'video.mp4',
                mimeType: videoFile.mimeType || 'video/mp4',
                data: videoFile.data,
                size: videoFile.size || (videoFile.data.length || 0)
            this.validateVideoFile(video);
            // Prepare video metadata
            const videoMetadata: any = {
                    categoryId: categoryId || '22' // Default to People & Blogs
                    privacyStatus: privacyStatus,
                    selfDeclaredMadeForKids: false,
            // Handle scheduled publishing
            if (publishAt && privacyStatus === 'private') {
                videoMetadata.status.publishAt = this.formatDate(publishAt);
            const uploadResponse = await this.makeYouTubeRequest<any>(
                videoMetadata,
                    uploadType: 'media'
            const videoId = uploadResponse.id;
            // Upload the actual video data using resumable upload
            const videoUrl = await this.uploadVideo(video, {
                tags: tags,
                categoryId: categoryId,
            // Upload thumbnail if provided
                    console.error('Failed to upload thumbnail:', error);
            // Get the uploaded video details
            const videoDetails = await this.makeYouTubeRequest<any>(
            const uploadedVideo = videoDetails.items[0];
                videoId: uploadedVideo.id,
                videoUrl: `https://www.youtube.com/watch?v=${uploadedVideo.id}`,
                title: uploadedVideo.snippet.title,
                description: uploadedVideo.snippet.description,
                tags: uploadedVideo.snippet.tags,
                categoryId: uploadedVideo.snippet.categoryId,
                channelId: uploadedVideo.snippet.channelId,
                publishedAt: uploadedVideo.snippet.publishedAt,
                privacyStatus: uploadedVideo.status.privacyStatus,
                publishAt: uploadedVideo.status.publishAt,
                duration: uploadedVideo.contentDetails.duration,
                thumbnails: uploadedVideo.snippet.thumbnails,
                quotaCost: this.getQuotaCost('videos.insert')
            const videoIdParam = outputParams.find(p => p.Name === 'VideoID');
            if (videoIdParam) videoIdParam.Value = videoId;
            const videoUrlParam = outputParams.find(p => p.Name === 'VideoURL');
            if (videoUrlParam) videoUrlParam.Value = result.videoUrl;
                Message: `Video uploaded successfully: ${result.videoUrl}`,
                Message: `Failed to upload video: ${errorMessage}`,
        const thumbnail: MediaFile = {
            filename: thumbnailFile.filename || 'thumbnail.jpg',
            mimeType: thumbnailFile.mimeType || 'image/jpeg',
            data: thumbnailFile.data,
            size: thumbnailFile.size || (thumbnailFile.data.length || 0)
        if (!['image/jpeg', 'image/png', 'image/gif', 'image/bmp'].includes(thumbnail.mimeType)) {
        if (thumbnail.size > maxSize) {
            thumbnail.data,
        return 'Uploads a video to YouTube with metadata, thumbnail, and scheduling options';
