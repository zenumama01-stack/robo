import { FacebookBaseAction, GetPagePostsParams } from '../facebook-base.action';
 * Retrieves posts from a Facebook page with optional date filtering.
 * Includes post content, media, and basic engagement metrics.
@RegisterClass(BaseAction, 'FacebookGetPagePostsAction')
export class FacebookGetPagePostsAction extends FacebookBaseAction {
        return 'Retrieves posts from a Facebook page with optional date range filtering and pagination';
                Value: 100,
                Name: 'IncludeUnpublished',
                Name: 'IncludeInsights',
            const limit = this.getParamValue(Params, 'Limit') as number || 100;
            const maxResults = this.getParamValue(Params, 'MaxResults') as number;
            const includeUnpublished = this.getParamValue(Params, 'IncludeUnpublished') as boolean;
            const includeInsights = this.getParamValue(Params, 'IncludeInsights') as boolean;
            const queryParams: GetPagePostsParams = {
                limit: Math.min(limit, 100), // Facebook max is 100 per request
                published: includeUnpublished ? undefined : true
                queryParams.since = new Date(startDate);
                queryParams.until = new Date(endDate);
            LogStatus(`Retrieving posts from Facebook page ${pageId}...`);
            // Get posts with pagination support
            let allPosts = await this.getPagePosts(pageId, queryParams);
            // If we need more posts and have a maxResults, use pagination
            if (maxResults && allPosts.length < maxResults && allPosts.length === queryParams.limit) {
                LogStatus(`Fetching additional pages of posts...`);
                const fields = includeInsights ? this.getPostFields() : this.getPostFieldsWithoutInsights();
                allPosts = await this.getPaginatedResults(
                    `${this.apiBaseUrl}/${pageId}/posts`,
                        ...queryParams,
                        since: queryParams.since ? Math.floor(queryParams.since.getTime() / 1000) : undefined,
                        until: queryParams.until ? Math.floor(queryParams.until.getTime() / 1000) : undefined
                    maxResults
            // Limit results if maxResults specified
            if (maxResults && allPosts.length > maxResults) {
                allPosts = allPosts.slice(0, maxResults);
            LogStatus(`Retrieved ${allPosts.length} posts from Facebook page`);
            // Normalize posts to common format
            const normalizedPosts = allPosts.map(post => this.normalizePost(post));
                totalPosts: normalizedPosts.length,
                    earliest: normalizedPosts.length > 0 
                        ? normalizedPosts[normalizedPosts.length - 1].publishedAt 
                    latest: normalizedPosts.length > 0 
                        ? normalizedPosts[0].publishedAt 
                postTypes: this.categorizePostTypes(allPosts),
                totalEngagements: normalizedPosts.reduce((sum, post) => 
                    sum + (post.analytics?.engagements || 0), 0
                totalImpressions: normalizedPosts.reduce((sum, post) => 
                    sum + (post.analytics?.impressions || 0), 0
                Message: `Successfully retrieved ${normalizedPosts.length} posts`,
            LogError(`Failed to get Facebook page posts: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get post fields without insights (for faster queries or when insights not available)
    private getPostFieldsWithoutInsights(): string {
        return 'id,message,created_time,updated_time,from,story,permalink_url,attachments,shares,reactions.summary(true),comments.summary(true)';
     * Categorize posts by type based on attachments
    private categorizePostTypes(posts: any[]): Record<string, number> {
        const types: Record<string, number> = {
            status: 0,
            photo: 0,
            video: 0,
            link: 0,
            event: 0,
            offer: 0,
            album: 0
            if (post.attachments?.data?.[0]?.type) {
                const type = post.attachments.data[0].type.toLowerCase();
                if (type in types) {
                    types[type]++;
                    types.status++;
