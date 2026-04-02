 * Action to get posts from a LinkedIn organization page
@RegisterClass(BaseAction, 'LinkedInGetOrganizationPostsAction')
export class LinkedInGetOrganizationPostsAction extends LinkedInBaseAction {
     * Get posts from a LinkedIn organization page
            const count = this.getParamValue(Params, 'Count') || 50;
            const startIndex = this.getParamValue(Params, 'StartIndex') || 0;
            const includeAnalytics = this.getParamValue(Params, 'IncludeAnalytics') === true;
            // Determine organization URN
            // Get posts
            LogStatus(`Fetching posts for organization...`);
            const shares = await this.getShares(organizationUrn, count, startIndex);
            for (const share of shares) {
                const post = this.normalizePost(share);
                // Optionally fetch analytics
                        const analytics = await this.getPostAnalytics(share.id, organizationUrn);
                        LogError(`Failed to get analytics for post ${share.id}: ${error instanceof Error ? error.message : 'Unknown error'}`);
            LogStatus(`Retrieved ${posts.length} posts`);
            if (totalCountParam) totalCountParam.Value = posts.length;
                Message: `Successfully retrieved ${posts.length} organization posts`,
                Message: `Failed to get organization posts: ${errorMessage}`,
    private async getPostAnalytics(shareId: string, organizationUrn: string): Promise<any> {
            const response = await this.axiosInstance.get('/organizationalEntityShareStatistics', {
                    shares: `List(${shareId})`
            if (response.data.elements && response.data.elements.length > 0) {
                return response.data.elements[0];
            LogError(`Failed to get post analytics: ${error instanceof Error ? error.message : 'Unknown error'}`);
                Name: 'StartIndex',
        return 'Retrieves posts from a LinkedIn organization page with optional analytics data';
