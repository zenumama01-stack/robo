 * Action to get timeline tweets (home timeline or user timeline) from Twitter/X
@RegisterClass(BaseAction, 'TwitterGetTimelineAction')
export class TwitterGetTimelineAction extends TwitterBaseAction {
     * Get timeline tweets from Twitter
            const timelineType = this.getParamValue(Params, 'TimelineType') || 'home'; // 'home' or 'user'
            const excludeReplies = this.getParamValue(Params, 'ExcludeReplies') === true;
            const excludeRetweets = this.getParamValue(Params, 'ExcludeRetweets') === true;
            let tweets: Tweet[] = [];
            let endpoint: string;
                'expansions': 'author_id,attachments.media_keys,referenced_tweets.id,referenced_tweets.id.author_id',
            if (timelineType === 'home') {
                // Get home timeline (requires user context)
                endpoint = `/users/${currentUser.id}/timelines/reverse_chronological`;
                // Home timeline specific parameters
                if (excludeReplies) {
                    queryParams['exclude'] = queryParams['exclude'] ? `${queryParams['exclude']},replies` : 'replies';
                if (excludeRetweets) {
                    queryParams['exclude'] = queryParams['exclude'] ? `${queryParams['exclude']},retweets` : 'retweets';
                // Get user timeline
                let targetUserId: string;
                    targetUserId = userId;
                } else if (username) {
                    // Look up user by username
                    const userResponse = await this.axiosInstance.get(`/users/by/username/${username}`, {
                        params: { 'user.fields': 'id' }
                    targetUserId = userResponse.data.data.id;
                    // Default to authenticated user
                    targetUserId = currentUser.id;
                endpoint = `/users/${targetUserId}/tweets`;
                // User timeline specific parameters
            // Get paginated tweets
            LogStatus(`Getting ${timelineType} timeline tweets...`);
            tweets = await this.getPaginatedTweets(endpoint, queryParams, maxResults);
            const normalizedPosts: SocialPost[] = tweets.map(tweet => this.normalizePost(tweet));
                totalTweets: normalizedPosts.length,
                totalImpressions: 0
                    stats.totalLikes += post.analytics.likes;
                    stats.totalRetweets += post.analytics.shares;
                    stats.totalReplies += post.analytics.comments;
                    stats.totalImpressions += post.analytics.impressions;
            if (tweetsParam) tweetsParam.Value = tweets;
                Message: `Successfully retrieved ${normalizedPosts.length} tweets from ${timelineType} timeline`,
                Message: `Failed to get timeline: ${errorMessage}`,
            if (error.message.includes('Not Found')) return 'USER_NOT_FOUND';
                Name: 'TimelineType',
                Value: 'home' // 'home' or 'user'
                Name: 'ExcludeReplies',
                Name: 'ExcludeRetweets',
        return 'Gets timeline tweets from Twitter/X including home timeline or a specific user\'s timeline with filtering options';
