 * Action to get pending (scheduled) posts from Buffer
@RegisterClass(BaseAction, 'BufferGetPendingPostsAction')
export class BufferGetPendingPostsAction extends BufferBaseAction {
     * Get pending posts from Buffer
            const profileId = this.getParamValue(Params, 'ProfileID');
            const page = this.getParamValue(Params, 'Page') || 1;
            const count = this.getParamValue(Params, 'Count') || 10;
            const since = this.getParamValue(Params, 'Since');
            const useUTC = this.getParamValue(Params, 'UseUTC') !== false;
            if (!profileId) {
                throw new Error('ProfileID is required');
            // Get pending posts
            const result = await this.getUpdates(profileId, 'pending', {
                count,
                since: since ? new Date(since) : undefined,
                utc: useUTC
            // Format posts
            const posts = result.updates || [];
            const formattedPosts = posts.map((update: any) => this.normalizePost(update));
                totalPosts: formattedPosts.length,
                hasMore: posts.length === count,
                profileId: profileId,
                    earliest: formattedPosts.length > 0 ? 
                        formattedPosts[formattedPosts.length - 1].scheduledFor : null,
                    latest: formattedPosts.length > 0 ? 
                        formattedPosts[0].scheduledFor : null
                postsByDay: this.groupPostsByDay(formattedPosts)
            const postsParam = outputParams.find(p => p.Name === 'Posts');
                Message: `Retrieved ${formattedPosts.length} pending posts from Buffer`,
                Message: `Failed to get pending posts: ${errorMessage}`,
     * Group posts by scheduled day
    private groupPostsByDay(posts: any[]): Record<string, number> {
        return posts.reduce((acc, post) => {
            if (post.scheduledFor) {
                const day = post.scheduledFor.toISOString().split('T')[0];
                acc[day] = (acc[day] || 0) + 1;
                Value: 1
                Name: 'UseUTC',
                Name: 'Posts',
        return 'Retrieves pending (scheduled) posts from Buffer for a specific social media profile';
