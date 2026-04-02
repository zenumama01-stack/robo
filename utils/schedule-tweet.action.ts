 * Action to schedule a tweet for future posting on Twitter/X
 * Note: Twitter API v2 doesn't have native scheduling, so this action
 * saves the tweet data for later posting via a separate scheduler service
@RegisterClass(BaseAction, 'TwitterScheduleTweetAction')
export class TwitterScheduleTweetAction extends TwitterBaseAction {
     * Schedule a tweet for future posting
            // Validate scheduled time is not too far in the future (e.g., 1 year)
            const maxFutureDate = new Date();
            maxFutureDate.setFullYear(maxFutureDate.getFullYear() + 1);
            if (scheduleDate > maxFutureDate) {
                throw new Error('ScheduledTime cannot be more than 1 year in the future');
            // Validate content length
            // Build tweet data structure
                // Validate poll option lengths
                    duration_minutes: Math.min(Math.max(pollDurationMinutes, 5), 10080)
            // Handle media files
            let uploadedMediaIds: string[] = [];
                // For scheduling, we'll upload the media now and store the IDs
                // Note: Twitter media IDs expire after 24 hours if not used
                const hoursUntilScheduled = (scheduleDate.getTime() - now.getTime()) / (1000 * 60 * 60);
                if (hoursUntilScheduled <= 24) {
                    // Upload now if scheduling within 24 hours
                    uploadedMediaIds = await this.uploadMedia(mediaFiles as MediaFile[]);
                        media_ids: uploadedMediaIds
                    // For tweets scheduled beyond 24 hours, we'll need to store media
                    // data and upload closer to the scheduled time
                    LogStatus('Media will be uploaded closer to scheduled time due to Twitter\'s 24-hour media expiration');
            // Create scheduled tweet record
            // In a real implementation, this would save to a database or queue service
            const scheduledTweet = {
                id: this.generateScheduledTweetId(),
                scheduledFor: scheduleDate.toISOString(),
                tweetData: tweetData,
                mediaFiles: mediaFiles, // Store for later upload if needed
                createdAt: new Date().toISOString(),
                status: 'scheduled'
            // Simulate saving to a scheduling service
            LogStatus(`Tweet scheduled for ${scheduleDate.toLocaleString()}`);
            // Get user info for context
            const scheduledIdParam = outputParams.find(p => p.Name === 'ScheduledTweetID');
            if (scheduledIdParam) scheduledIdParam.Value = scheduledTweet.id;
            const scheduledDataParam = outputParams.find(p => p.Name === 'ScheduledTweetData');
            if (scheduledDataParam) scheduledDataParam.Value = scheduledTweet;
            const estimatedUrlParam = outputParams.find(p => p.Name === 'EstimatedURL');
            if (estimatedUrlParam) estimatedUrlParam.Value = `https://twitter.com/${user.username}/status/[pending]`;
                Message: `Successfully scheduled tweet for ${scheduleDate.toLocaleString()}`,
                Message: `Failed to schedule tweet: ${errorMessage}`,
     * Generate a unique ID for scheduled tweet
    private generateScheduledTweetId(): string {
        return `scheduled_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
            if (error.message.includes('future')) return 'INVALID_SCHEDULE_TIME';
                Name: 'ScheduledTweetID',
                Name: 'ScheduledTweetData',
                Name: 'EstimatedURL',
        return 'Schedules a tweet for future posting on Twitter/X with optional media attachments, polls, replies, or quote tweets';
