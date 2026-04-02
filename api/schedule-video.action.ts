 * Action to schedule a YouTube video for publishing
@RegisterClass(BaseAction, 'YouTubeScheduleVideoAction')
export class YouTubeScheduleVideoAction extends YouTubeBaseAction {
     * Schedule a video for publishing on YouTube
            const publishAt = this.getParamValue(Params, 'PublishAt');
            const notifySubscribers = this.getParamValue(Params, 'NotifySubscribers') ?? true;
            const premiereTiming = this.getParamValue(Params, 'PremiereTiming');
            const enableChat = this.getParamValue(Params, 'EnableChat') ?? false;
            if (!publishAt) {
                throw new Error('PublishAt date/time is required');
            // Validate publish date is in the future
            const publishDate = new Date(publishAt);
            if (publishDate <= new Date()) {
                throw new Error('PublishAt must be a future date/time');
            // Get current video details
            const currentVideo = await this.makeYouTubeRequest<any>(
                    part: 'snippet,status',
            if (!currentVideo.items || currentVideo.items.length === 0) {
                throw new Error(`Video not found: ${videoId}`);
            const video = currentVideo.items[0];
            // Verify video is currently private
            if (video.status.privacyStatus !== 'private') {
                throw new Error(`Video must be private to schedule. Current status: ${video.status.privacyStatus}`);
            // Prepare update data
            const updateData: any = {
                id: videoId,
                    privacyStatus: 'private', // Keep private until publish time
                    publishAt: this.formatDate(publishDate),
                    selfDeclaredMadeForKids: video.status.selfDeclaredMadeForKids,
                    notifySubscribers: notifySubscribers
            // Handle premiere settings
            if (premiereTiming === 'premiere') {
                // Set up as a premiere
                updateData.status.privacyStatus = 'unlisted'; // Premieres start as unlisted
                updateData.liveStreamingDetails = {
                    scheduledStartTime: this.formatDate(publishDate),
                    enableLowLatency: false,
                    enableAutoStart: true,
                    enableDvr: true
                if (enableChat) {
                    updateData.liveStreamingDetails.enableClosedCaptions = false;
                    updateData.liveStreamingDetails.enableContentEncryption = false;
                    updateData.liveStreamingDetails.enableEmbed = true;
            // Update the video
            const updateResponse = await this.makeYouTubeRequest<any>(
                updateData,
                    part: 'status' + (premiereTiming === 'premiere' ? ',liveStreamingDetails' : '')
            // Get updated video details
            const updatedVideo = await this.makeYouTubeRequest<any>(
                    part: 'snippet,status,contentDetails' + (premiereTiming === 'premiere' ? ',liveStreamingDetails' : ''),
            const finalVideo = updatedVideo.items[0];
            // Calculate time until publish
            const timeUntilPublish = publishDate.getTime() - Date.now();
            const hoursUntilPublish = Math.floor(timeUntilPublish / (1000 * 60 * 60));
            const minutesUntilPublish = Math.floor((timeUntilPublish % (1000 * 60 * 60)) / (1000 * 60));
                videoId: finalVideo.id,
                videoUrl: `https://www.youtube.com/watch?v=${finalVideo.id}`,
                title: finalVideo.snippet.title,
                scheduledFor: finalVideo.status.publishAt,
                scheduledForLocal: new Date(finalVideo.status.publishAt).toLocaleString(),
                timeUntilPublish: {
                    hours: hoursUntilPublish,
                    minutes: minutesUntilPublish,
                    formatted: `${hoursUntilPublish}h ${minutesUntilPublish}m`
                privacyStatus: finalVideo.status.privacyStatus,
                notifySubscribers: notifySubscribers,
                isPremiere: premiereTiming === 'premiere',
                premiereDetails: premiereTiming === 'premiere' ? {
                    scheduledStartTime: finalVideo.liveStreamingDetails?.scheduledStartTime,
                    chatEnabled: enableChat
                thumbnails: finalVideo.snippet.thumbnails,
                duration: finalVideo.contentDetails.duration,
                quotaCost: this.getQuotaCost('videos.update')
            const scheduleDetailsParam = outputParams.find(p => p.Name === 'ScheduleDetails');
            if (scheduleDetailsParam) scheduleDetailsParam.Value = result;
            const scheduledTimeParam = outputParams.find(p => p.Name === 'ScheduledTime');
            if (scheduledTimeParam) scheduledTimeParam.Value = result.scheduledFor;
                Message: `Video scheduled successfully for ${result.scheduledForLocal} (${result.timeUntilPublish.formatted} from now)`,
                Message: `Failed to schedule video: ${errorMessage}`,
        if (message.includes('future')) return 'INVALID_DATE';
        if (message.includes('private')) return 'INVALID_STATUS';
                Name: 'PublishAt',
                Name: 'NotifySubscribers',
                Name: 'PremiereTiming',
                Value: 'standard'
                Name: 'EnableChat',
                Name: 'ScheduleDetails',
        return 'Schedules a private YouTube video to be published at a specific date/time with optional premiere settings';
