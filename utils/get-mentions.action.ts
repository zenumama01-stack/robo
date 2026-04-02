import { TwitterBaseAction, Tweet } from '../twitter-base.action';
 * Action to get mentions (tweets that mention the authenticated user) from Twitter/X
@RegisterClass(BaseAction, 'TwitterGetMentionsAction')
export class TwitterGetMentionsAction extends TwitterBaseAction {
     * Get mentions from Twitter
            const endTime = this.getParamValue(Params, 'EndTime');
            const sinceId = this.getParamValue(Params, 'SinceID');
            const untilId = this.getParamValue(Params, 'UntilID');
            const includeRetweets = this.getParamValue(Params, 'IncludeRetweets') !== false; // Default true
                'tweet.fields': 'id,text,created_at,author_id,conversation_id,public_metrics,attachments,entities,referenced_tweets,in_reply_to_user_id',
                'user.fields': 'id,name,username,profile_image_url,description,created_at,verified',
                'expansions': 'author_id,attachments.media_keys,referenced_tweets.id,referenced_tweets.id.author_id,in_reply_to_user_id',
                'max_results': Math.min(maxResults, 100) // API limit per request
            // Add time-based filters if provided
            if (startTime) {
                queryParams['start_time'] = this.formatTwitterDate(startTime);
            if (endTime) {
                queryParams['end_time'] = this.formatTwitterDate(endTime);
            if (sinceId) {
                queryParams['since_id'] = sinceId;
            if (untilId) {
                queryParams['until_id'] = untilId;
            // Get mentions using the mentions timeline endpoint
            const endpoint = `/users/${currentUser.id}/mentions`;
            LogStatus(`Getting mentions for @${currentUser.username}...`);
            const mentions = await this.getPaginatedTweets(endpoint, queryParams, maxResults);
            // Filter out retweets if requested
            let filteredMentions = mentions;
            if (!includeRetweets) {
                filteredMentions = mentions.filter(tweet => 
                    !tweet.referenced_tweets || 
                    !tweet.referenced_tweets.some(ref => ref.type === 'retweeted')
            // Convert to normalized format
            const normalizedPosts: SocialPost[] = filteredMentions.map(tweet => this.normalizePost(tweet));
            // Group mentions by type
            const mentionTypes = {
                directMentions: 0,
                replies: 0,
                quotes: 0
            filteredMentions.forEach(tweet => {
                if (tweet.referenced_tweets) {
                    const refTypes = tweet.referenced_tweets.map(ref => ref.type);
                    if (refTypes.includes('replied_to')) {
                        mentionTypes.replies++;
                    } else if (refTypes.includes('retweeted')) {
                        mentionTypes.retweets++;
                    } else if (refTypes.includes('quoted')) {
                        mentionTypes.quotes++;
                    mentionTypes.directMentions++;
            // Calculate engagement statistics
                totalMentions: normalizedPosts.length,
                mentionTypes,
                topMentions: [] as any[]
            // Calculate total engagement
            normalizedPosts.forEach(post => {
                    stats.totalEngagement += post.analytics.engagements;
            // Calculate average engagement
            if (normalizedPosts.length > 0) {
                stats.averageEngagement = Math.round(stats.totalEngagement / normalizedPosts.length);
            // Get top 5 mentions by engagement
            stats.topMentions = normalizedPosts
                .filter(post => post.analytics)
                .sort((a, b) => (b.analytics?.engagements || 0) - (a.analytics?.engagements || 0))
                .map(post => ({
                    content: post.content.substring(0, 100) + (post.content.length > 100 ? '...' : ''),
                    engagement: post.analytics?.engagements,
                    author: filteredMentions.find(t => t.id === post.id)?.author_id
            const postsParam = outputParams.find(p => p.Name === 'Mentions');
            const tweetsParam = outputParams.find(p => p.Name === 'Tweets');
            if (tweetsParam) tweetsParam.Value = filteredMentions;
            const statsParam = outputParams.find(p => p.Name === 'Statistics');
            if (statsParam) statsParam.Value = stats;
                Message: `Successfully retrieved ${normalizedPosts.length} mentions`,
                Message: `Failed to get mentions: ${errorMessage}`,
                Name: 'EndTime',
                Name: 'SinceID',
                Name: 'UntilID',
                Name: 'IncludeRetweets',
                Name: 'Mentions',
        return 'Gets tweets that mention the authenticated user on Twitter/X with engagement statistics and filtering options';
