import { TwitterBaseAction } from '../twitter-base.action';
 * Action to delete a tweet from Twitter/X
@RegisterClass(BaseAction, 'TwitterDeleteTweetAction')
export class TwitterDeleteTweetAction extends TwitterBaseAction {
     * Delete a tweet from Twitter
            const tweetId = this.getParamValue(Params, 'TweetID');
            const confirmDeletion = this.getParamValue(Params, 'ConfirmDeletion') === true;
            if (!tweetId) {
                throw new Error('TweetID is required');
            // Safety check - require explicit confirmation
                    Message: 'Tweet deletion requires explicit confirmation. Set ConfirmDeletion to true.',
            // Get the tweet details before deletion (for output)
            let tweetDetails: any = null;
                LogStatus(`Retrieving tweet details for ID: ${tweetId}...`);
                const response = await this.axiosInstance.get(`/tweets/${tweetId}`, {
                        'tweet.fields': 'id,text,created_at,author_id,public_metrics',
                        'expansions': 'author_id',
                        'user.fields': 'id,username'
                if (response.data.data) {
                    tweetDetails = {
                        id: response.data.data.id,
                        text: response.data.data.text,
                        createdAt: response.data.data.created_at,
                        metrics: response.data.data.public_metrics
                    // Verify ownership
                    const currentUser = await this.getCurrentUser();
                    if (response.data.data.author_id !== currentUser.id) {
                        throw new Error('You can only delete your own tweets');
                // If we can't retrieve the tweet, it might already be deleted or we don't have access
                LogStatus('Could not retrieve tweet details. It may already be deleted or inaccessible.');
            // Delete the tweet
            LogStatus(`Deleting tweet ID: ${tweetId}...`);
            await this.deleteTweet(tweetId);
            const deletedDetailsParam = outputParams.find(p => p.Name === 'DeletedTweetDetails');
            if (deletedDetailsParam && tweetDetails) deletedDetailsParam.Value = tweetDetails;
            const deletionTimeParam = outputParams.find(p => p.Name === 'DeletionTime');
            if (deletionTimeParam) deletionTimeParam.Value = new Date().toISOString();
                Message: `Successfully deleted tweet (ID: ${tweetId})`,
                Message: `Failed to delete tweet: ${errorMessage}`,
            if (error.message.includes('Not Found')) return 'TWEET_NOT_FOUND';
            if (error.message.includes('Forbidden')) return 'INSUFFICIENT_PERMISSIONS';
            if (error.message.includes('own tweets')) return 'NOT_OWNER';
                Name: 'DeletedTweetDetails',
                Name: 'DeletionTime',
        return 'Deletes a tweet from Twitter/X. Requires explicit confirmation and ownership of the tweet.';
