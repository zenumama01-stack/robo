 * Retrieves comments for an Instagram post, including nested replies.
 * Supports filtering, pagination, and sentiment analysis.
@RegisterClass(BaseAction, 'Instagram - Get Comments')
export class InstagramGetCommentsAction extends InstagramBaseAction {
            const postId = this.getParamValue(params.Params, 'PostID');
            const includeReplies = this.getParamValue(params.Params, 'IncludeReplies') !== false;
            const includeHidden = this.getParamValue(params.Params, 'IncludeHidden') || false;
            const limit = this.getParamValue(params.Params, 'Limit') || 50;
                    ResultCode: 'MISSING_PARAMS'
            // Build fields for comment data
            const fields = 'id,text,username,timestamp,like_count,replies{id,text,username,timestamp,like_count}';
                limit: Math.min(limit, 100)
            // Get comments
                `${postId}/comments`,
            const comments = response.data || [];
            // Process comments
            const processedComments = await this.processComments(comments, includeReplies);
            // Get hidden comments if requested
            let hiddenComments: any[] = [];
            if (includeHidden) {
                hiddenComments = await this.getHiddenComments(postId);
            // Calculate engagement metrics
            const metrics = this.calculateCommentMetrics(processedComments);
            // Analyze sentiment patterns
            const sentimentAnalysis = this.analyzeSentiment(processedComments);
                    comments: processedComments,
                    hiddenComments,
                    sentimentAnalysis,
                        afterCursor: response.paging?.cursors?.after
                Message: `Retrieved ${processedComments.length} comments`,
            LogError('Failed to retrieve Instagram comments', error);
            if (error.code === 'POST_NOT_FOUND') {
                    Message: 'Instagram post not found or access denied',
                    ResultCode: 'POST_NOT_FOUND'
                Message: `Failed to retrieve comments: ${error.message}`,
     * Process comments and fetch replies if needed
    private async processComments(comments: any[], includeReplies: boolean): Promise<any[]> {
        const processed: any[] = [];
        for (const comment of comments) {
            const processedComment: any = {
                id: comment.id,
                text: comment.text,
                username: comment.username,
                timestamp: comment.timestamp,
                likeCount: comment.like_count || 0,
                replies: [],
                    wordCount: this.countWords(comment.text),
                    hasEmojis: this.containsEmojis(comment.text),
                    hasMentions: this.containsMentions(comment.text),
                    hasHashtags: this.containsHashtags(comment.text)
            // Process replies if they exist and are requested
            if (includeReplies && comment.replies?.data) {
                processedComment.replies = comment.replies.data.map((reply: any) => ({
                    id: reply.id,
                    text: reply.text,
                    username: reply.username,
                    timestamp: reply.timestamp,
                    likeCount: reply.like_count || 0,
                        wordCount: this.countWords(reply.text),
                        hasEmojis: this.containsEmojis(reply.text),
                        hasMentions: this.containsMentions(reply.text),
                        hasHashtags: this.containsHashtags(reply.text)
            processed.push(processedComment);
     * Get hidden comments (comments hidden by the account)
    private async getHiddenComments(postId: string): Promise<any[]> {
                    fields: 'id,text,username,timestamp,hidden',
                    filter: 'hidden'
            LogError('Failed to get hidden comments', error);
     * Calculate comment metrics
    private calculateCommentMetrics(comments: any[]): any {
            totalComments: comments.length,
            totalReplies: 0,
            avgCommentLength: 0,
            avgLikesPerComment: 0,
            topCommenters: [] as any[],
            engagementRate: 0,
            responseRate: 0
        if (comments.length === 0) return metrics;
        let totalLength = 0;
        const commenterCounts: Record<string, number> = {};
        comments.forEach(comment => {
            totalLength += comment.metrics.wordCount;
            totalLikes += comment.likeCount;
            // Count commenters
            commenterCounts[comment.username] = (commenterCounts[comment.username] || 0) + 1;
            // Count replies
            metrics.totalReplies += comment.replies.length;
            // Add reply stats
            comment.replies.forEach((reply: any) => {
                totalLength += reply.metrics.wordCount;
                totalLikes += reply.likeCount;
                commenterCounts[reply.username] = (commenterCounts[reply.username] || 0) + 1;
        const totalInteractions = comments.length + metrics.totalReplies;
        metrics.avgCommentLength = Math.round(totalLength / totalInteractions);
        metrics.avgLikesPerComment = Math.round(totalLikes / totalInteractions);
        // Get top commenters
        metrics.topCommenters = Object.entries(commenterCounts)
            .map(([username, count]) => ({ username, count }));
        // Calculate response rate (comments with replies)
        const commentsWithReplies = comments.filter(c => c.replies.length > 0).length;
        metrics.responseRate = (commentsWithReplies / comments.length) * 100;
     * Analyze sentiment patterns in comments
    private analyzeSentiment(comments: any[]): any {
        const analysis = {
            positive: 0,
            negative: 0,
            neutral: 0,
            questions: 0,
            keywords: [] as any[],
            emojis: [] as any[]
        const keywordCounts: Record<string, number> = {};
        const emojiCounts: Record<string, number> = {};
        // Simple sentiment analysis based on keywords and patterns
        const positiveWords = ['love', 'amazing', 'beautiful', 'great', 'awesome', 'perfect', 'excellent', 'wonderful', '❤️', '😍', '🔥', '💯'];
        const negativeWords = ['hate', 'awful', 'terrible', 'bad', 'worst', 'ugly', 'disgusting', '😠', '😡', '👎'];
        const questionWords = ['?', 'what', 'where', 'when', 'how', 'why', 'who'];
            const text = comment.text.toLowerCase();
            // Check sentiment
            const hasPositive = positiveWords.some(word => text.includes(word));
            const hasNegative = negativeWords.some(word => text.includes(word));
            const hasQuestion = questionWords.some(word => text.includes(word));
            if (hasQuestion) {
                analysis.questions++;
            if (hasPositive && !hasNegative) {
                analysis.positive++;
            } else if (hasNegative && !hasPositive) {
                analysis.negative++;
                analysis.neutral++;
            // Extract keywords (simple word frequency)
            const words = text.split(/\s+/).filter(word => word.length > 3);
            words.forEach(word => {
                keywordCounts[word] = (keywordCounts[word] || 0) + 1;
            // Extract emojis
            const emojis = text.match(/[\u{1F300}-\u{1F9FF}]|[\u{2600}-\u{26FF}]/gu) || [];
            emojis.forEach(emoji => {
                emojiCounts[emoji] = (emojiCounts[emoji] || 0) + 1;
            // Analyze replies too
                // Similar analysis for replies...
        // Get top keywords and emojis
        analysis.keywords = Object.entries(keywordCounts)
            .slice(0, 20)
            .map(([keyword, count]) => ({ keyword, count }));
        analysis.emojis = Object.entries(emojiCounts)
            .map(([emoji, count]) => ({ emoji, count }));
        return analysis;
     * Helper methods for text analysis
    private countWords(text: string): number {
        return text.trim().split(/\s+/).length;
    private containsEmojis(text: string): boolean {
        return /[\u{1F300}-\u{1F9FF}]|[\u{2600}-\u{26FF}]/u.test(text);
    private containsMentions(text: string): boolean {
        return /@\w+/.test(text);
    private containsHashtags(text: string): boolean {
        return /#\w+/.test(text);
                Name: 'IncludeReplies',
                Name: 'IncludeHidden',
                Value: 50
        return 'Retrieves comments for an Instagram post including replies, metrics, and sentiment analysis.';
 * Comment data structure
interface TikTokComment {
    comment_id: string;
    reply_count: number;
    parent_comment_id?: string;
 * Action to get comments from TikTok videos
@RegisterClass(BaseAction, 'GetCommentsAction')
export class GetCommentsAction extends TikTokBaseAction {
     * Get comments from TikTok videos
            const videoId = this.getParamValue(Params, 'VideoID');
            const maxComments = this.getParamValue(Params, 'MaxComments') || 100;
            const includeReplies = this.getParamValue(Params, 'IncludeReplies') !== false;
            const sortBy = this.getParamValue(Params, 'SortBy') || 'time'; // time or likes
            if (!videoId) {
                throw new Error('VideoID is required');
            // Get comments from TikTok API
                `/v2/video/comment/list/`,
                    video_id: videoId,
                    max_count: Math.min(maxComments, 100), // API limit
                    sort_by: sortBy
            const comments: TikTokComment[] = response.data?.comments || [];
            const processedComments = comments.map(comment => ({
                id: comment.comment_id,
                author: {
                    id: comment.user.open_id,
                    username: comment.user.display_name,
                    avatarUrl: comment.user.avatar_url
                createdAt: new Date(comment.create_time * 1000),
                likes: comment.like_count,
                replies: comment.reply_count,
                isReply: !!comment.parent_comment_id,
                parentCommentId: comment.parent_comment_id,
                sentiment: this.analyzeSentiment(comment.text),
                containsQuestion: this.containsQuestion(comment.text),
                length: comment.text.length
            // Separate top-level comments and replies
            const topLevelComments = processedComments.filter(c => !c.isReply);
            const replies = processedComments.filter(c => c.isReply);
            const engagementMetrics = {
                topLevelComments: topLevelComments.length,
                totalReplies: replies.length,
                averageLikes: comments.length > 0 
                    ? Math.round(comments.reduce((sum, c) => sum + c.like_count, 0) / comments.length)
                mostLikedComment: processedComments.length > 0
                    ? processedComments.reduce((max, c) => c.likes > max.likes ? c : max)
                sentimentBreakdown: this.calculateSentimentBreakdown(processedComments),
                questionsCount: processedComments.filter(c => c.containsQuestion).length,
                averageCommentLength: processedComments.length > 0
                    ? Math.round(processedComments.reduce((sum, c) => sum + c.length, 0) / processedComments.length)
            // Identify notable comments (high engagement, questions, etc.)
            const notableComments = this.identifyNotableComments(processedComments);
                videoId,
                totalCommentsRetrieved: processedComments.length,
                hasMoreComments: response.data?.has_more || false,
                engagementMetrics,
                notableComments,
                topCommenters: this.getTopCommenters(processedComments),
                timeDistribution: this.analyzeTimeDistribution(processedComments)
            const commentsParam = outputParams.find(p => p.Name === 'Comments');
            if (commentsParam) commentsParam.Value = includeReplies ? processedComments : topLevelComments;
            const rawDataParam = outputParams.find(p => p.Name === 'RawData');
            if (rawDataParam) rawDataParam.Value = comments;
                Message: `Retrieved ${processedComments.length} comments from TikTok video`,
                Message: `Failed to get TikTok comments: ${errorMessage}`,
     * Simple sentiment analysis
    private analyzeSentiment(text: string): 'positive' | 'negative' | 'neutral' {
        const lowerText = text.toLowerCase();
        // Positive indicators
        const positiveWords = ['love', 'amazing', 'great', 'awesome', 'fantastic', 'excellent', 'good', '❤️', '😍', '🔥', '👏', '💯'];
        const negativeWords = ['hate', 'terrible', 'awful', 'bad', 'worst', 'disappointing', 'trash', '👎', '😠', '😡'];
        const positiveScore = positiveWords.filter(word => lowerText.includes(word)).length;
        const negativeScore = negativeWords.filter(word => lowerText.includes(word)).length;
        if (positiveScore > negativeScore) return 'positive';
        if (negativeScore > positiveScore) return 'negative';
        return 'neutral';
     * Check if comment contains a question
    private containsQuestion(text: string): boolean {
        return text.includes('?') || 
               /\b(what|when|where|who|why|how|is|are|can|could|would|should)\b/i.test(text);
     * Calculate sentiment breakdown
    private calculateSentimentBreakdown(comments: any[]): Record<string, number> {
        const breakdown = {
            neutral: 0
            breakdown[comment.sentiment]++;
        return breakdown;
     * Identify notable comments
    private identifyNotableComments(comments: any[]): any[] {
        const notable: any[] = [];
        // High engagement comments
        const avgLikes = comments.length > 0 
            ? comments.reduce((sum, c) => sum + c.likes, 0) / comments.length
            const reasons: string[] = [];
            if (comment.likes > avgLikes * 2) {
                reasons.push('high_engagement');
            if (comment.containsQuestion) {
                reasons.push('contains_question');
            if (comment.replies > 5) {
                reasons.push('many_replies');
            if (comment.text.length > 200) {
                reasons.push('detailed_feedback');
            if (reasons.length > 0) {
                notable.push({
                    ...comment,
                    notableReasons: reasons
        // Return top 10 notable comments
        return notable.sort((a, b) => b.likes - a.likes).slice(0, 10);
     * Get top commenters
    private getTopCommenters(comments: any[]): any[] {
        const commenterMap = new Map<string, any>();
            const key = comment.author.id;
            if (!commenterMap.has(key)) {
                commenterMap.set(key, {
                    ...comment.author,
                    commentCount: 0,
                    totalLikes: 0
            const commenter = commenterMap.get(key)!;
            commenter.commentCount++;
            commenter.totalLikes += comment.likes;
        return Array.from(commenterMap.values())
            .sort((a, b) => b.commentCount - a.commentCount)
     * Analyze time distribution of comments
    private analyzeTimeDistribution(comments: any[]): any {
        if (comments.length === 0) return null;
        const hourBuckets: Record<string, number> = {};
            const hoursSincePost = Math.floor((now.getTime() - comment.createdAt.getTime()) / (1000 * 60 * 60));
            let bucket: string;
            if (hoursSincePost < 1) bucket = '< 1 hour';
            else if (hoursSincePost < 24) bucket = '1-24 hours';
            else if (hoursSincePost < 168) bucket = '1-7 days';
            else bucket = '> 7 days';
            hourBuckets[bucket] = (hourBuckets[bucket] || 0) + 1;
        return hourBuckets;
                Name: 'VideoID',
                Name: 'MaxComments',
                Value: 'time'
                Name: 'Comments',
                Name: 'RawData',
        return 'Retrieves and analyzes comments from TikTok videos including sentiment, engagement metrics, and notable comments';
 * Action to get comments from YouTube videos
@RegisterClass(BaseAction, 'YouTubeGetCommentsAction')
export class YouTubeGetCommentsAction extends YouTubeBaseAction {
     * Get comments from YouTube videos
            const channelId = this.getParamValue(Params, 'ChannelID');
            const orderBy = this.getParamValue(Params, 'OrderBy') || 'time';
            const searchTerms = this.getParamValue(Params, 'SearchTerms');
            const includeReplies = this.getParamValue(Params, 'IncludeReplies') ?? true;
            const textFormat = this.getParamValue(Params, 'TextFormat') || 'plainText';
            // Validate parameters - need either videoId or channelId
            if (!videoId && !channelId) {
                throw new Error('Either VideoID or ChannelID is required');
            // Build request parameters
            const requestParams: any = {
                part: 'snippet,replies',
                maxResults: Math.min(maxResults, 100), // YouTube max is 100
                order: orderBy,
                textFormat: textFormat
            if (videoId) {
                requestParams.videoId = videoId;
            } else if (channelId) {
                requestParams.allThreadsRelatedToChannelId = channelId;
            if (searchTerms) {
                requestParams.searchTerms = searchTerms;
                requestParams.pageToken = pageToken;
            // Get comment threads
            const commentsResponse = await this.makeYouTubeRequest<any>(
                '/commentThreads',
                requestParams,
            const comments = this.processComments(commentsResponse.items || [], includeReplies);
            // Get video details if videoId provided
            let videoDetails = null;
                const videoResponse = await this.makeYouTubeRequest<any>(
                        part: 'snippet,statistics',
                        id: videoId
                if (videoResponse.items && videoResponse.items.length > 0) {
                    videoDetails = videoResponse.items[0];
            const stats = this.calculateCommentStats(comments);
                totalThreads: commentsResponse.items?.length || 0,
                videoDetails: videoDetails ? {
                    id: videoDetails.id,
                    title: videoDetails.snippet.title,
                    channelId: videoDetails.snippet.channelId,
                    channelTitle: videoDetails.snippet.channelTitle,
                    viewCount: parseInt(videoDetails.statistics.viewCount || '0'),
                    commentCount: parseInt(videoDetails.statistics.commentCount || '0')
                sentiment: this.analyzeSentiment(comments),
                topCommenters: this.getTopCommenters(comments),
                nextPageToken: commentsResponse.nextPageToken,
                prevPageToken: commentsResponse.prevPageToken,
                quotaCost: this.getQuotaCost('commentThreads.list') + (videoId ? this.getQuotaCost('videos.list') : 0)
            if (commentsParam) commentsParam.Value = comments;
            if (nextPageTokenParam) nextPageTokenParam.Value = commentsResponse.nextPageToken;
                Message: `Retrieved ${comments.length} comments from ${commentsResponse.items?.length || 0} threads`,
                Message: `Failed to get comments: ${errorMessage}`,
     * Process raw comment threads into a flat array
    private processComments(threads: any[], includeReplies: boolean): any[] {
        const comments: any[] = [];
        for (const thread of threads) {
            const topComment = thread.snippet.topLevelComment;
            // Add top-level comment
            comments.push({
                id: topComment.id,
                threadId: thread.id,
                text: topComment.snippet.textDisplay,
                authorDisplayName: topComment.snippet.authorDisplayName,
                authorChannelId: topComment.snippet.authorChannelId?.value,
                authorProfileImageUrl: topComment.snippet.authorProfileImageUrl,
                likeCount: topComment.snippet.likeCount,
                publishedAt: topComment.snippet.publishedAt,
                updatedAt: topComment.snippet.updatedAt,
                isTopLevel: true,
                replyCount: thread.snippet.totalReplyCount,
                videoId: topComment.snippet.videoId,
                canReply: thread.snippet.canReply,
                isPublic: thread.snippet.isPublic
            // Add replies if requested and available
            if (includeReplies && thread.replies?.comments) {
                for (const reply of thread.replies.comments) {
                        parentId: topComment.id,
                        text: reply.snippet.textDisplay,
                        authorDisplayName: reply.snippet.authorDisplayName,
                        authorChannelId: reply.snippet.authorChannelId?.value,
                        authorProfileImageUrl: reply.snippet.authorProfileImageUrl,
                        likeCount: reply.snippet.likeCount,
                        publishedAt: reply.snippet.publishedAt,
                        updatedAt: reply.snippet.updatedAt,
                        isTopLevel: false,
                        videoId: reply.snippet.videoId
        return comments;
     * Calculate comment statistics
    private calculateCommentStats(comments: any[]): any {
            averageLikes: 0,
            topLevelComments: 0,
            commentersCount: new Set(),
            averageCommentLength: 0,
            longestComment: 0,
            shortestComment: Infinity
            stats.totalLikes += comment.likeCount || 0;
            if (comment.isTopLevel) {
                stats.topLevelComments++;
                stats.replies++;
            if (comment.authorChannelId) {
                stats.commentersCount.add(comment.authorChannelId);
            const length = comment.text.length;
            totalLength += length;
            stats.longestComment = Math.max(stats.longestComment, length);
            stats.shortestComment = Math.min(stats.shortestComment, length);
        stats.averageLikes = comments.length > 0 ? Math.round(stats.totalLikes / comments.length * 100) / 100 : 0;
        stats.averageCommentLength = comments.length > 0 ? Math.round(totalLength / comments.length) : 0;
            totalLikes: stats.totalLikes,
            averageLikes: stats.averageLikes,
            topLevelComments: stats.topLevelComments,
            replies: stats.replies,
            uniqueCommenters: stats.commentersCount.size,
            averageCommentLength: stats.averageCommentLength,
            longestComment: stats.longestComment,
            shortestComment: stats.shortestComment === Infinity ? 0 : stats.shortestComment
     * Basic sentiment analysis (simplified)
        const positiveWords = ['love', 'great', 'amazing', 'awesome', 'excellent', 'fantastic', 'wonderful', 'best', 'perfect', 'thank'];
        const negativeWords = ['hate', 'bad', 'terrible', 'awful', 'worst', 'horrible', 'disgusting', 'trash', 'waste', 'dislike'];
        let positive = 0;
        let negative = 0;
        let neutral = 0;
            let hasPositive = false;
            let hasNegative = false;
            for (const word of positiveWords) {
                if (text.includes(word)) {
                    hasPositive = true;
            for (const word of negativeWords) {
                    hasNegative = true;
                positive++;
                negative++;
                neutral++;
        const total = comments.length || 1;
            positive: positive,
            negative: negative,
            neutral: neutral,
            positivePercentage: Math.round((positive / total) * 100),
            negativePercentage: Math.round((negative / total) * 100),
            neutralPercentage: Math.round((neutral / total) * 100)
     * Get top commenters by frequency
            if (!comment.authorChannelId) continue;
            const key = comment.authorChannelId;
                    channelId: comment.authorChannelId,
                    displayName: comment.authorDisplayName,
                    profileImage: comment.authorProfileImageUrl,
            const commenter = commenterMap.get(key);
            commenter.totalLikes += comment.likeCount || 0;
        if (message.includes('disabled')) return 'COMMENTS_DISABLED';
                Name: 'SearchTerms',
                Name: 'TextFormat',
                Value: 'plainText'
        return 'Gets comments from YouTube videos or channels with sentiment analysis and statistics';
