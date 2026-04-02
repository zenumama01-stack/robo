import { TikTokBaseAction, TikTokVideo } from '../tiktok-base.action';
 * Action to get videos from a TikTok user account
@RegisterClass(BaseAction, 'GetUserVideosAction')
export class GetUserVideosAction extends TikTokBaseAction {
     * Get user videos from TikTok
            const userId = this.getParamValue(Params, 'UserID') || this.getCustomAttribute(1);
            const maxVideos = this.getParamValue(Params, 'MaxVideos') || 20;
            // Get user videos
                    fields: 'id,share_url,title,description,duration,cover_image_url,share_count,view_count,like_count,comment_count,create_time',
                    max_count: Math.min(maxVideos, 100) // TikTok limit
            const videos: TikTokVideo[] = response.data?.videos || [];
            // Convert to social posts format
            const socialPosts: SocialPost[] = videos.map(video => this.normalizePost(video));
                averageViews: videos.length > 0 ? Math.round(videos.reduce((sum, v) => sum + v.view_count, 0) / videos.length) : 0,
                averageLikes: videos.length > 0 ? Math.round(videos.reduce((sum, v) => sum + v.like_count, 0) / videos.length) : 0,
                dateRange: videos.length > 0 ? {
                    oldest: new Date(Math.min(...videos.map(v => v.create_time)) * 1000),
                    newest: new Date(Math.max(...videos.map(v => v.create_time)) * 1000)
            const videosParam = outputParams.find(p => p.Name === 'Videos');
            if (videosParam) videosParam.Value = socialPosts;
            if (rawDataParam) rawDataParam.Value = videos;
                Message: `Retrieved ${videos.length} videos from TikTok user`,
                Message: `Failed to get TikTok user videos: ${errorMessage}`,
                Name: 'MaxVideos',
                Name: 'Videos',
        return 'Retrieves videos from a TikTok user account with analytics and metadata';
