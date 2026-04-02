 * Action to get videos from a YouTube channel
@RegisterClass(BaseAction, 'YouTubeGetChannelVideosAction')
export class YouTubeGetChannelVideosAction extends YouTubeBaseAction {
     * Get videos from a YouTube channel
            const maxResults = this.getParamValue(Params, 'MaxResults') || 50;
            const orderBy = this.getParamValue(Params, 'OrderBy') || 'date';
            const publishedAfter = this.getParamValue(Params, 'PublishedAfter');
            const publishedBefore = this.getParamValue(Params, 'PublishedBefore');
            const includePrivate = this.getParamValue(Params, 'IncludePrivate') || false;
            const videoType = this.getParamValue(Params, 'VideoType') || 'any';
            const videoDuration = this.getParamValue(Params, 'VideoDuration') || 'any';
            const pageToken = this.getParamValue(Params, 'PageToken');
                        part: 'id,snippet',
                channelId: actualChannelId,
                maxResults: Math.min(maxResults, 50), // YouTube max is 50
                order: orderBy
            if (publishedAfter) {
                searchParams.publishedAfter = this.formatDate(publishedAfter);
            if (publishedBefore) {
                searchParams.publishedBefore = this.formatDate(publishedBefore);
            // Add video type filter
            if (videoType !== 'any') {
                searchParams.videoType = videoType; // 'episode' or 'movie'
            // Add duration filter
            if (videoDuration !== 'any') {
                searchParams.videoDuration = videoDuration; // 'short', 'medium', 'long'
            // Add pagination token
            if (pageToken) {
                searchParams.pageToken = pageToken;
            // Search for videos
            // Extract video IDs
            const videoIds = searchResponse.items.map((item: any) => item.id.videoId);
            if (videoIds.length === 0) {
                // No videos found
                if (videosParam) videosParam.Value = [];
                if (summaryParam) summaryParam.Value = { totalVideos: 0, channelId: actualChannelId };
                    Message: 'No videos found for the specified criteria',
            // Get detailed video information
                    part: 'snippet,contentDetails,statistics,status',
                    id: videoIds.join(',')
            // Filter out private videos if not requested
            let videos = videosResponse.items;
            if (!includePrivate) {
                videos = videos.filter((v: any) => v.status.privacyStatus !== 'private');
            const socialPosts: SocialPost[] = videos.map((video: any) => this.normalizePost(video));
                totalViews: videos.reduce((sum: number, v: any) => sum + parseInt(v.statistics.viewCount || '0'), 0),
                totalLikes: videos.reduce((sum: number, v: any) => sum + parseInt(v.statistics.likeCount || '0'), 0),
                totalComments: videos.reduce((sum: number, v: any) => sum + parseInt(v.statistics.commentCount || '0'), 0),
                averageViews: Math.round(videos.reduce((sum: number, v: any) => sum + parseInt(v.statistics.viewCount || '0'), 0) / videos.length),
                    oldest: videos.length > 0 ? videos[videos.length - 1].snippet.publishedAt : null,
                    newest: videos.length > 0 ? videos[0].snippet.publishedAt : null
                privacyBreakdown: this.groupByPrivacy(videos),
                nextPageToken: searchResponse.nextPageToken,
                prevPageToken: searchResponse.prevPageToken,
                quotaCost: this.getQuotaCost('search.list') + this.getQuotaCost('videos.list')
            const nextPageTokenParam = outputParams.find(p => p.Name === 'NextPageToken');
            if (nextPageTokenParam) nextPageTokenParam.Value = searchResponse.nextPageToken;
                Message: `Retrieved ${videos.length} videos from channel`,
                Message: `Failed to get channel videos: ${errorMessage}`,
     * Group videos by privacy status
    private groupByPrivacy(videos: any[]): Record<string, number> {
        return videos.reduce((acc, video) => {
            const privacy = video.status.privacyStatus;
            acc[privacy] = (acc[privacy] || 0) + 1;
        }, {});
                Name: 'PublishedAfter',
                Name: 'PublishedBefore',
                Name: 'IncludePrivate',
                Name: 'VideoType',
                Value: 'any'
                Name: 'VideoDuration',
                Name: 'PageToken',
                Name: 'NextPageToken',
        return 'Gets videos from a YouTube channel with filtering and pagination support';
