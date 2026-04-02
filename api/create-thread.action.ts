import { TwitterBaseAction, CreateTweetData, Tweet } from '../twitter-base.action';
import { MediaFile, SocialPost } from '../../../base/base-social.action';
 * Action to create a thread (series of connected tweets) on Twitter/X
@RegisterClass(BaseAction, 'TwitterCreateThreadAction')
export class TwitterCreateThreadAction extends TwitterBaseAction {
     * Create a thread on Twitter
            const tweets = this.getParamValue(Params, 'Tweets');
            const mediaFilesByTweet = this.getParamValue(Params, 'MediaFilesByTweet');
            const includeNumbers = this.getParamValue(Params, 'IncludeNumbers') !== false; // Default true
            const numberFormat = this.getParamValue(Params, 'NumberFormat') || '{n}/{total}'; // Default format
            if (!tweets || !Array.isArray(tweets) || tweets.length === 0) {
                throw new Error('Tweets array is required and must not be empty');
            if (tweets.length < 2) {
                throw new Error('A thread must contain at least 2 tweets');
            if (tweets.length > 25) {
                throw new Error('Twitter threads are limited to 25 tweets');
            // Prepare tweets with numbering if requested
            const preparedTweets = includeNumbers 
                ? this.addThreadNumbers(tweets, numberFormat)
                : tweets;
            // Validate all tweet lengths
            for (let i = 0; i < preparedTweets.length; i++) {
                if (preparedTweets[i].length > 280) {
                    throw new Error(`Tweet ${i + 1} exceeds Twitter's 280 character limit (current: ${preparedTweets[i].length} characters)`);
            const createdTweets: Tweet[] = [];
            const createdPosts: SocialPost[] = [];
            let previousTweetId: string | undefined;
                // Create tweets in sequence
                    LogStatus(`Creating tweet ${i + 1} of ${preparedTweets.length}...`);
                    const tweetData: CreateTweetData = {
                        text: preparedTweets[i]
                    // Reply to previous tweet in thread
                    if (previousTweetId) {
                        tweetData.reply = {
                            in_reply_to_tweet_id: previousTweetId
                    // Add media if provided for this tweet
                    if (mediaFilesByTweet && mediaFilesByTweet[i]) {
                        const mediaFiles = mediaFilesByTweet[i];
                        if (Array.isArray(mediaFiles) && mediaFiles.length > 0) {
                            if (mediaFiles.length > 4) {
                                throw new Error(`Tweet ${i + 1}: Twitter supports a maximum of 4 media items per tweet`);
                            LogStatus(`Uploading ${mediaFiles.length} media files for tweet ${i + 1}...`);
                            tweetData.media = {
                                media_ids: mediaIds
                    // Create the tweet
                    const tweet = await this.createTweet(tweetData);
                    createdTweets.push(tweet);
                    createdPosts.push(this.normalizePost(tweet));
                    previousTweetId = tweet.id;
                    // Small delay between tweets to avoid rate limiting
                    if (i < preparedTweets.length - 1) {
                        await new Promise(resolve => setTimeout(resolve, 1000)); // 1 second delay
                // Get user info for URLs
                const user = await this.getCurrentUser();
                // Build thread URL (link to first tweet)
                const threadUrl = `https://twitter.com/${user.username}/status/${createdTweets[0].id}`;
                if (postsParam) postsParam.Value = createdPosts;
                const tweetIdsParam = outputParams.find(p => p.Name === 'TweetIDs');
                if (tweetIdsParam) tweetIdsParam.Value = createdTweets.map(t => t.id);
                const threadUrlParam = outputParams.find(p => p.Name === 'ThreadURL');
                if (threadUrlParam) threadUrlParam.Value = threadUrl;
                    Message: `Successfully created thread with ${createdTweets.length} tweets`,
                // If thread creation fails partway through, note which tweets were created
                if (createdTweets.length > 0) {
                    LogError(`Thread creation failed after creating ${createdTweets.length} tweets. Tweet IDs: ${createdTweets.map(t => t.id).join(', ')}`);
                ResultCode: this.getErrorCode(error),
                Message: `Failed to create thread: ${errorMessage}`,
     * Add thread numbers to tweets
    private addThreadNumbers(tweets: string[], format: string): string[] {
        const total = tweets.length;
        return tweets.map((tweet, index) => {
            const number = index + 1;
            const threadNumber = format
                .replace('{n}', number.toString())
                .replace('{total}', total.toString());
            // Add number to beginning of tweet with a space
            return `${threadNumber} ${tweet}`;
     * Get error code based on error type
    private getErrorCode(error: any): string {
            if (error.message.includes('Rate Limit')) return 'RATE_LIMIT';
            if (error.message.includes('Unauthorized')) return 'INVALID_TOKEN';
            if (error.message.includes('character limit')) return 'CONTENT_TOO_LONG';
            if (error.message.includes('media')) return 'INVALID_MEDIA';
        return 'ERROR';
                Name: 'Tweets',
                Name: 'MediaFilesByTweet',
                Name: 'IncludeNumbers',
                Name: 'NumberFormat',
                Value: '{n}/{total}'
                Name: 'TweetIDs',
                Name: 'ThreadURL',
        return 'Creates a thread (series of connected tweets) on Twitter/X with optional media attachments and automatic numbering';
