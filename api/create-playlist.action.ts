import { YouTubeBaseAction } from '../youtube-base.action';
 * Action to create a playlist on YouTube
@RegisterClass(BaseAction, 'YouTubeCreatePlaylistAction')
export class YouTubeCreatePlaylistAction extends YouTubeBaseAction {
     * Create a playlist on YouTube
            const initialized = await this.initializeOAuth(companyIntegrationId);
            if (!initialized) {
                throw new Error('Failed to initialize YouTube OAuth connection');
            const privacyStatus = this.getParamValue(Params, 'PrivacyStatus') || 'private';
            const defaultLanguage = this.getParamValue(Params, 'DefaultLanguage');
                throw new Error('Title is required for playlist creation');
            // Create playlist
            const playlistData = {
                    description: description || '',
                    tags: tags || [],
                    defaultLanguage: defaultLanguage || 'en'
                    privacyStatus: privacyStatus
            const playlistResponse = await this.makeYouTubeRequest<any>(
                '/playlists',
                playlistData,
            const playlistId = playlistResponse.id;
            // Add videos to playlist if provided
            let addedVideos: any[] = [];
            if (videoIds && videoIds.length > 0) {
                addedVideos = await this.addVideosToPlaylist(playlistId, videoIds, ContextUser);
            // Get full playlist details
            const playlistDetails = await this.makeYouTubeRequest<any>(
                    part: 'snippet,status,contentDetails',
                    id: playlistId
            const playlist = playlistDetails.items[0];
            // Prepare result
                playlistId: playlist.id,
                playlistUrl: `https://www.youtube.com/playlist?list=${playlist.id}`,
                title: playlist.snippet.title,
                description: playlist.snippet.description,
                tags: playlist.snippet.tags,
                privacyStatus: playlist.status.privacyStatus,
                itemCount: playlist.contentDetails.itemCount,
                channelId: playlist.snippet.channelId,
                channelTitle: playlist.snippet.channelTitle,
                publishedAt: playlist.snippet.publishedAt,
                thumbnails: playlist.snippet.thumbnails,
                videosAdded: addedVideos.length,
                videoDetails: addedVideos,
                quotaCost: this.getQuotaCost('playlists.insert') + (addedVideos.length * this.getQuotaCost('playlistItems.insert'))
            const playlistDetailsParam = outputParams.find(p => p.Name === 'PlaylistDetails');
            if (playlistDetailsParam) playlistDetailsParam.Value = result;
            const playlistIdParam = outputParams.find(p => p.Name === 'PlaylistID');
            if (playlistIdParam) playlistIdParam.Value = playlistId;
            const playlistUrlParam = outputParams.find(p => p.Name === 'PlaylistURL');
            if (playlistUrlParam) playlistUrlParam.Value = result.playlistUrl;
                Message: `Playlist created successfully with ${addedVideos.length} videos: ${result.playlistUrl}`,
                ResultCode: this.getErrorCode(errorMessage),
                Message: `Failed to create playlist: ${errorMessage}`,
     * Add videos to a playlist
    private async addVideosToPlaylist(playlistId: string, videoIds: string[], contextUser?: any): Promise<any[]> {
        const addedVideos: any[] = [];
        for (let i = 0; i < videoIds.length; i++) {
            const videoId = videoIds[i];
                const playlistItem = {
                        playlistId: playlistId,
                        position: i,
                        resourceId: {
                            kind: 'youtube#video',
                            videoId: videoId
                    '/playlistItems',
                    playlistItem,
                        part: 'snippet'
                addedVideos.push({
                    videoId: videoId,
                    position: response.snippet.position,
                    addedAt: response.snippet.publishedAt,
                    playlistItemId: response.id
                // Log error but continue adding other videos
                console.error(`Failed to add video ${videoId} to playlist:`, error);
        return addedVideos;
     * Get error code from error message
    private getErrorCode(message: string): string {
        if (message.includes('quota')) return 'QUOTA_EXCEEDED';
        if (message.includes('401') || message.includes('unauthorized')) return 'INVALID_TOKEN';
        if (message.includes('403') || message.includes('forbidden')) return 'INSUFFICIENT_PERMISSIONS';
        if (message.includes('404')) return 'NOT_FOUND';
        if (message.includes('rate limit')) return 'RATE_LIMIT';
        const baseParams = this.commonSocialParams;
                Name: 'PrivacyStatus',
                Value: 'private'
                Name: 'DefaultLanguage',
                Value: 'en'
                Name: 'PlaylistDetails',
                Name: 'PlaylistID',
                Name: 'PlaylistURL',
        return 'Creates a YouTube playlist and optionally adds videos to it';
