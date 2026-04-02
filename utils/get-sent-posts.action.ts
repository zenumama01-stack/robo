 * Action to get sent (published) posts from Buffer
@RegisterClass(BaseAction, 'BufferGetSentPostsAction')
export class BufferGetSentPostsAction extends BufferBaseAction {
     * Get sent posts from Buffer
            // Get sent posts
                        formattedPosts[formattedPosts.length - 1].publishedAt : null,
                        formattedPosts[0].publishedAt : null
                postsByDay: this.groupPostsByDay(formattedPosts),
                totalEngagements: this.calculateTotalEngagements(formattedPosts),
                topPerformingPost: this.findTopPerformingPost(formattedPosts)
                Message: `Retrieved ${formattedPosts.length} sent posts from Buffer`,
                Message: `Failed to get sent posts: ${errorMessage}`,
     * Group posts by published day
            if (post.publishedAt) {
                const day = post.publishedAt.toISOString().split('T')[0];
     * Calculate total engagements across all posts
    private calculateTotalEngagements(posts: any[]): number {
        return posts.reduce((total, post) => {
            if (post.analytics) {
                return total + post.analytics.engagements;
            return total;
     * Find the top performing post by engagements
    private findTopPerformingPost(posts: any[]): any {
        if (posts.length === 0) return null;
        return posts.reduce((top, post) => {
            if (!top || (post.analytics?.engagements || 0) > (top.analytics?.engagements || 0)) {
                return post;
            return top;
        }, posts[0]);
        return 'Retrieves sent (published) posts from Buffer for a specific social media profile with analytics data';
