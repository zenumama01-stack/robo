 * Retrieves account-level insights for an Instagram Business account.
 * Includes follower demographics, reach, impressions, and profile activity.
@RegisterClass(BaseAction, 'Instagram - Get Account Insights')
export class InstagramGetAccountInsightsAction extends InstagramBaseAction {
            const period = this.getParamValue(params.Params, 'Period') || 'day';
            const includeDemographics = this.getParamValue(params.Params, 'IncludeDemographics') !== false;
            // Define metrics based on period
            const metrics = this.getAccountMetrics(period);
            // Get account insights
                period as any
            // Parse insights data
            const parsedInsights = this.parseAccountInsights(insights);
            // Get account info
            const accountInfo = await this.getAccountInfo();
            // Get demographics if requested
                demographics = await this.getAccountDemographics();
            // Get content performance summary
            const contentSummary = await this.getContentPerformanceSummary(startDate, endDate);
            // Calculate growth metrics
            const growthMetrics = this.calculateGrowthMetrics(parsedInsights, accountInfo);
                Name: 'ResultData',
                    account: {
                        id: this.instagramBusinessAccountId,
                        username: accountInfo.username,
                        name: accountInfo.name,
                        biography: accountInfo.biography,
                        followersCount: accountInfo.followers_count,
                        followsCount: accountInfo.follows_count,
                        mediaCount: accountInfo.media_count,
                        profilePictureUrl: accountInfo.profile_picture_url,
                        websiteUrl: accountInfo.website
                    insights: parsedInsights,
                    growth: growthMetrics,
                    contentPerformance: contentSummary,
                    dataCollectedAt: new Date().toISOString()
                Message: 'Successfully retrieved Instagram account insights',
            LogError('Failed to retrieve Instagram account insights', error);
            if (error.code === 'INSUFFICIENT_PERMISSIONS') {
                    Message: 'Insufficient permissions to access Instagram account insights',
                Message: `Failed to retrieve account insights: ${error.message}`,
     * Get appropriate metrics based on period
    private getAccountMetrics(period: string): string[] {
        const baseMetrics = [
            'impressions',
            'reach',
            'profile_views',
            'website_clicks'
        if (period === 'day') {
                ...baseMetrics,
                'follower_count',
                'email_contacts',
                'phone_call_clicks',
                'text_message_clicks',
                'get_directions_clicks'
        } else if (period === 'week' || period === 'days_28') {
                'accounts_engaged'
        return baseMetrics;
     * Get account information
    private async getAccountInfo(): Promise<any> {
                fields: 'username,name,biography,followers_count,follows_count,media_count,profile_picture_url,website',
     * Get account demographics
    private async getAccountDemographics(): Promise<any> {
            // Get follower demographics
                'audience_city',
                'audience_country',
                'audience_gender_age',
                'audience_locale',
                'online_followers'
            const demographics = await this.getInsights(
                demographicMetrics,
            // Parse demographic data
            const parsed: any = {
                locations: {
                    cities: {},
                    countries: {}
                genderAge: {},
                languages: {},
                onlineHours: {}
            demographics.forEach((metric: any) => {
                const value = metric.values?.[0]?.value;
                switch (metric.name) {
                    case 'audience_city':
                        parsed.locations.cities = value || {};
                    case 'audience_country':
                        parsed.locations.countries = value || {};
                    case 'audience_gender_age':
                        parsed.genderAge = this.parseGenderAge(value || {});
                    case 'audience_locale':
                        parsed.languages = value || {};
                    case 'online_followers':
                        parsed.onlineHours = this.parseOnlineHours(value || {});
            LogError('Failed to get demographics', error);
     * Parse gender/age demographics
    private parseGenderAge(data: any): any {
            male: {},
            female: {},
            unknown: {}
        Object.keys(data).forEach(key => {
            const [gender, ageRange] = key.split('.');
            if (parsed[gender]) {
                parsed[gender][ageRange] = data[key];
     * Parse online hours data
    private parseOnlineHours(data: any): any {
        Object.keys(data).forEach(hour => {
            parsed[hour] = {
                count: data[hour],
                label: `${hour}:00`
     * Get content performance summary
    private async getContentPerformanceSummary(startDate?: string, endDate?: string): Promise<any> {
            // Get recent posts
                fields: 'id,media_type,like_count,comments_count,timestamp',
                limit: 50
                queryParams.since = Math.floor(new Date(startDate).getTime() / 1000);
                queryParams.until = Math.floor(new Date(endDate).getTime() / 1000);
                queryParams
            const posts = response.data || [];
            // Calculate performance metrics
                avgLikes: 0,
                avgComments: 0,
                topPerformingPost: null as any,
                mediaTypeBreakdown: {
                    IMAGE: 0,
                    VIDEO: 0,
                    CAROUSEL_ALBUM: 0,
                    REELS: 0
            if (posts.length > 0) {
                let totalLikes = 0;
                let totalComments = 0;
                let topPost = posts[0];
                    totalLikes += post.like_count || 0;
                    totalComments += post.comments_count || 0;
                    // Track media types
                    if (summary.mediaTypeBreakdown[post.media_type] !== undefined) {
                        summary.mediaTypeBreakdown[post.media_type]++;
                    // Find top performing post
                    const engagement = (post.like_count || 0) + (post.comments_count || 0);
                    const topEngagement = (topPost.like_count || 0) + (topPost.comments_count || 0);
                    if (engagement > topEngagement) {
                        topPost = post;
                summary.avgLikes = Math.round(totalLikes / posts.length);
                summary.avgComments = Math.round(totalComments / posts.length);
                summary.totalEngagements = totalLikes + totalComments;
                summary.topPerformingPost = {
                    id: topPost.id,
                    likes: topPost.like_count,
                    comments: topPost.comments_count,
                    engagement: (topPost.like_count || 0) + (topPost.comments_count || 0),
                    publishedAt: topPost.timestamp
            LogError('Failed to get content performance', error);
     * Parse account insights
    private parseAccountInsights(insights: any[]): Record<string, any> {
        const parsed: Record<string, any> = {};
            const name = metric.name;
            const values = metric.values || [];
                parsed[name] = {
                    value: values[0].value || 0,
                    title: metric.title,
                    description: metric.description,
                    period: metric.period
                // For time series data
                if (values.length > 1) {
                    parsed[name].timeSeries = values.map((v: any) => ({
                        value: v.value,
                        endTime: v.end_time
     * Calculate growth metrics
    private calculateGrowthMetrics(insights: any, accountInfo: any): any {
        const growth: any = {
            followersGrowth: 0,
            followersGrowthRate: 0,
            reachGrowth: 0,
            impressionsGrowth: 0,
            engagementRate: 0
        // Calculate follower growth if we have time series data
        const followerData = insights.follower_count;
        if (followerData?.timeSeries && followerData.timeSeries.length > 1) {
            const firstValue = followerData.timeSeries[0].value;
            const lastValue = followerData.timeSeries[followerData.timeSeries.length - 1].value;
            growth.followersGrowth = lastValue - firstValue;
            if (firstValue > 0) {
                growth.followersGrowthRate = ((lastValue - firstValue) / firstValue) * 100;
        // Calculate engagement rate
        const reach = insights.reach?.value || 0;
        const profileViews = insights.profile_views?.value || 0;
        const websiteClicks = insights.website_clicks?.value || 0;
        if (reach > 0) {
            const totalEngagements = profileViews + websiteClicks;
            growth.engagementRate = (totalEngagements / reach) * 100;
        return growth;
                Value: 'day'
        return 'Retrieves comprehensive account-level insights including follower demographics, reach, impressions, and growth metrics.';
