 * Action to get trending hashtags on TikTok
@RegisterClass(BaseAction, 'GetTrendingHashtagsAction')
export class GetTrendingHashtagsAction extends TikTokBaseAction {
     * Get trending hashtags from TikTok
            const country = this.getParamValue(Params, 'Country') || 'US';
            const category = this.getParamValue(Params, 'Category');
            const limit = this.getParamValue(Params, 'Limit') || 20;
            // Note: TikTok's public API has limited access to trending data
            // This implementation uses available endpoints and user's own content for insights
            // Get user's recent videos to analyze hashtag performance
            const userVideos = await this.getUserVideos();
            // Extract and analyze hashtags from user's content
            const hashtagAnalysis = this.analyzeHashtagsFromVideos(userVideos);
            // Since TikTok doesn't provide a direct trending hashtags API,
            // we'll provide insights based on the user's content and general trends
            const trendingInsights = {
                disclaimer: 'TikTok API does not provide direct access to trending hashtags. These insights are based on your content analysis.',
                userHashtagPerformance: hashtagAnalysis.topPerforming.slice(0, limit),
                recommendedHashtags: this.generateHashtagRecommendations(category, country),
                generalTrends: this.getGeneralTrendingTopics(category, country),
                hashtagStrategy: this.generateHashtagStrategy(hashtagAnalysis)
            // Create formatted output
            const formattedHashtags = trendingInsights.userHashtagPerformance.map((tag, index) => ({
                rank: index + 1,
                hashtag: tag.hashtag,
                usageCount: tag.count,
                averageViews: tag.avgViews,
                averageEngagement: tag.avgEngagement,
                performanceScore: tag.score,
                trend: tag.trend
                topHashtag: formattedHashtags[0] || null,
                totalHashtagsAnalyzed: hashtagAnalysis.allHashtags.length,
                averageHashtagsPerVideo: hashtagAnalysis.avgHashtagsPerVideo,
                bestPerformingCategory: this.identifyBestCategory(hashtagAnalysis),
                recommendations: trendingInsights.recommendedHashtags.slice(0, 5),
                strategy: trendingInsights.hashtagStrategy
            const hashtagsParam = outputParams.find(p => p.Name === 'TrendingHashtags');
            if (hashtagsParam) hashtagsParam.Value = formattedHashtags;
            const insightsParam = outputParams.find(p => p.Name === 'Insights');
            if (insightsParam) insightsParam.Value = trendingInsights;
                Message: `Analyzed hashtag performance from ${userVideos.length} videos. Note: Direct trending data not available via TikTok API.`,
                Message: `Failed to get TikTok trending hashtags: ${errorMessage}`,
     * Analyze hashtags from user's videos
    private analyzeHashtagsFromVideos(videos: any[]): any {
        const hashtagMap = new Map<string, any>();
        videos.forEach(video => {
            const hashtags = this.extractHashtags(video.description);
                if (!hashtagMap.has(tag)) {
                    hashtagMap.set(tag, {
                        hashtag: `#${tag}`,
                        totalViews: 0,
                        totalEngagement: 0,
                        videos: []
                const tagData = hashtagMap.get(tag)!;
                tagData.count++;
                tagData.totalViews += video.view_count;
                tagData.totalEngagement += video.like_count + video.comment_count + video.share_count;
                tagData.videos.push({
                    views: video.view_count,
                    engagement: video.like_count + video.comment_count + video.share_count
        // Calculate averages and scores
        const allHashtags = Array.from(hashtagMap.values()).map(tag => ({
            ...tag,
            avgViews: Math.round(tag.totalViews / tag.count),
            avgEngagement: Math.round(tag.totalEngagement / tag.count),
            score: this.calculateHashtagScore(tag),
            trend: this.calculateTrend(tag.videos)
        // Sort by performance score
        const topPerforming = allHashtags.sort((a, b) => b.score - a.score);
            allHashtags,
            topPerforming,
            avgHashtagsPerVideo: videos.length > 0 
                ? allHashtags.reduce((sum, tag) => sum + tag.count, 0) / videos.length
     * Calculate hashtag performance score
    private calculateHashtagScore(tagData: any): number {
        const viewWeight = 1;
        const engagementWeight = 10;
        const frequencyWeight = 5;
            (tagData.avgViews * viewWeight) +
            (tagData.avgEngagement * engagementWeight) +
            (tagData.count * frequencyWeight)
     * Calculate trend direction
    private calculateTrend(videos: any[]): 'rising' | 'stable' | 'declining' {
        if (videos.length < 2) return 'stable';
        // Simple trend calculation based on recent vs older performance
        const midpoint = Math.floor(videos.length / 2);
        const recentAvg = videos.slice(0, midpoint).reduce((sum, v) => sum + v.views, 0) / midpoint;
        const olderAvg = videos.slice(midpoint).reduce((sum, v) => sum + v.views, 0) / (videos.length - midpoint);
        const changePercent = ((recentAvg - olderAvg) / olderAvg) * 100;
        if (changePercent > 20) return 'rising';
        if (changePercent < -20) return 'declining';
     * Generate hashtag recommendations based on category and country
    private generateHashtagRecommendations(category?: string, country?: string): string[] {
        // These are example recommendations - in a real implementation,
        // these would be based on actual trending data
        const recommendations: Record<string, string[]> = {
            entertainment: ['#fyp', '#foryou', '#viral', '#trending', '#comedy', '#funny', '#entertainment'],
            music: ['#music', '#newmusic', '#musician', '#singer', '#song', '#cover', '#originalmusic'],
            dance: ['#dance', '#dancer', '#dancechallenge', '#choreography', '#dancing', '#dancetrend'],
            food: ['#foodtok', '#cooking', '#recipe', '#foodie', '#homecooking', '#easyrecipe', '#foodlover'],
            fitness: ['#fitness', '#workout', '#gym', '#fitnessmotivation', '#healthylifestyle', '#exercise'],
            beauty: ['#beauty', '#makeup', '#skincare', '#beautytips', '#makeuptutorial', '#glowup'],
            fashion: ['#fashion', '#ootd', '#style', '#outfitideas', '#fashiontrends', '#streetstyle'],
            tech: ['#tech', '#technology', '#techtok', '#gadgets', '#innovation', '#ai', '#coding'],
            education: ['#education', '#learning', '#edutok', '#learnontiktok', '#knowledge', '#facts'],
            general: ['#fyp', '#foryoupage', '#viral', '#trending', '#tiktok', '#explore', '#viralvideo']
        const categoryTags = recommendations[category?.toLowerCase() || 'general'] || recommendations.general;
        // Add country-specific tags if applicable
        const countryTags: Record<string, string[]> = {
            US: ['#usa', '#american', '#unitedstates'],
            UK: ['#uk', '#british', '#unitedkingdom'],
            CA: ['#canada', '#canadian'],
            AU: ['#australia', '#aussie'],
            IN: ['#india', '#indian']
        const locationTags = countryTags[country || 'US'] || [];
        return [...categoryTags, ...locationTags];
     * Get general trending topics
    private getGeneralTrendingTopics(category?: string, country?: string): any {
        // Placeholder for general trends
            currentEvents: ['Major sporting events', 'Seasonal celebrations', 'Viral challenges'],
            contentTypes: ['Short tutorials', 'Behind-the-scenes', 'Day in the life', 'Transformations'],
            musicTrends: ['Trending sounds', 'Popular audio clips', 'Remix challenges'],
            formatTrends: ['Vertical video', 'Quick cuts', 'Text overlays', 'Transitions']
     * Generate hashtag strategy
    private generateHashtagStrategy(analysis: any): string[] {
        const strategies: string[] = [];
        // Mix of hashtag sizes
        strategies.push('Use a mix of popular (1M+ posts), medium (100K-1M), and niche (<100K) hashtags');
        // Optimal number
        strategies.push('Include 3-5 highly relevant hashtags per video for best reach');
        // Based on performance
        if (analysis.topPerforming.length > 0) {
            const bestTag = analysis.topPerforming[0];
            strategies.push(`Your best performing hashtag is ${bestTag.hashtag} - use it consistently`);
        // Timing
        strategies.push('Add trending hashtags within the first hour of a trend for maximum visibility');
        // Branded hashtags
        strategies.push('Create a unique branded hashtag to build community around your content');
        return strategies;
     * Identify best performing category
    private identifyBestCategory(analysis: any): string {
        // Simplified category identification based on hashtag patterns
        const categories: Record<string, string[]> = {
            entertainment: ['funny', 'comedy', 'lol', 'humor', 'entertainment'],
            music: ['music', 'song', 'singer', 'musician', 'cover'],
            dance: ['dance', 'dancer', 'choreography', 'dancing'],
            lifestyle: ['lifestyle', 'life', 'daily', 'routine', 'vlog']
        const categoryCounts: Record<string, number> = {};
        analysis.allHashtags.forEach((tag: any) => {
            const tagLower = tag.hashtag.toLowerCase();
            Object.entries(categories).forEach(([category, keywords]) => {
                if (keywords.some(keyword => tagLower.includes(keyword))) {
                    categoryCounts[category] = (categoryCounts[category] || 0) + tag.score;
        const bestCategory = Object.entries(categoryCounts)
            .sort(([, a], [, b]) => b - a)[0];
        return bestCategory ? bestCategory[0] : 'general';
                Value: 'US'
                Name: 'Category',
                Value: 20
                Name: 'TrendingHashtags',
                Name: 'Insights',
        return 'Analyzes hashtag performance from your TikTok content and provides trending insights (Note: Direct trending API not available)';
