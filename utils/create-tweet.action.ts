import { TwitterBaseAction, CreateTweetData } from '../twitter-base.action';
 * Action to create a tweet on Twitter/X
@RegisterClass(BaseAction, 'TwitterCreateTweetAction')
export class TwitterCreateTweetAction extends TwitterBaseAction {
     * Create a tweet on Twitter
            const replyToTweetId = this.getParamValue(Params, 'ReplyToTweetID');
            const quoteTweetId = this.getParamValue(Params, 'QuoteTweetID');
            const pollOptions = this.getParamValue(Params, 'PollOptions');
            const pollDurationMinutes = this.getParamValue(Params, 'PollDurationMinutes') || 1440; // Default 24 hours
            // Validate content length (Twitter's limit is 280 characters)
            if (content.length > 280) {
                throw new Error(`Content exceeds Twitter's 280 character limit (current: ${content.length} characters)`);
            // Build tweet data
            // Add reply if specified
            if (replyToTweetId) {
                    in_reply_to_tweet_id: replyToTweetId
            // Add quote tweet if specified
            if (quoteTweetId) {
                tweetData.quote_tweet_id = quoteTweetId;
            // Add poll if specified
            if (pollOptions && Array.isArray(pollOptions) && pollOptions.length >= 2) {
                if (pollOptions.length > 4) {
                    throw new Error('Twitter polls support a maximum of 4 options');
                // Validate poll option lengths (max 25 characters each)
                for (const option of pollOptions) {
                    if (option.length > 25) {
                        throw new Error(`Poll option "${option}" exceeds 25 character limit`);
                tweetData.poll = {
                    options: pollOptions,
                    duration_minutes: Math.min(Math.max(pollDurationMinutes, 5), 10080) // Min 5 minutes, max 7 days
                    throw new Error('Twitter supports a maximum of 4 media items per tweet');
            LogStatus('Creating tweet...');
            // Normalize the created tweet
            const normalizedPost = this.normalizePost(tweet);
            // Get user info for additional context
            const tweetIdParam = outputParams.find(p => p.Name === 'TweetID');
            if (tweetIdParam) tweetIdParam.Value = tweet.id;
            const tweetUrlParam = outputParams.find(p => p.Name === 'TweetURL');
            if (tweetUrlParam) tweetUrlParam.Value = `https://twitter.com/${user.username}/status/${tweet.id}`;
                Message: `Successfully created tweet (ID: ${tweet.id})`,
                Message: `Failed to create tweet: ${errorMessage}`,
                Name: 'ReplyToTweetID',
                Name: 'QuoteTweetID',
                Name: 'PollOptions',
                Name: 'PollDurationMinutes',
                Value: 1440 // Default 24 hours
                Name: 'TweetID',
                Name: 'TweetURL',
        return 'Creates a tweet on Twitter/X with optional media attachments, polls, replies, or quote tweets';
