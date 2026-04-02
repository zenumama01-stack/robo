import { FacebookBaseAction } from '../facebook-base.action';
import { SocialMediaErrorCode } from '../../../base/base-social.action';
import axios from 'axios';
 * Boosts (promotes) a Facebook post to reach a wider audience.
 * Creates a simple ad campaign to increase post visibility.
@RegisterClass(BaseAction, 'FacebookBoostPostAction')
export class FacebookBoostPostAction extends FacebookBaseAction {
     * Get action description
        return 'Boosts (promotes) a Facebook post to reach a wider audience through paid advertising';
                Name: 'PostID',
                Name: 'AdAccountID',
                Name: 'Budget',
                Name: 'Duration',
                Value: 7,
                Name: 'Objective',
                Value: 'POST_ENGAGEMENT',
                Name: 'AudienceType',
                Value: 'AUTO',
                Name: 'TargetingSpec',
                Name: 'StartTime',
                Name: 'CallToAction',
     * Execute the action
            const postId = this.getParamValue(Params, 'PostID');
            const adAccountId = this.getParamValue(Params, 'AdAccountID');
            const budget = this.getParamValue(Params, 'Budget') as number;
                Message: 'CompanyIntegrationID is required',
                ResultCode: 'INVALID_TOKEN'
            if (!postId) {
                Message: 'PostID is required',
                ResultCode: 'MISSING_REQUIRED_PARAM'
            if (!adAccountId) {
                Message: 'AdAccountID is required',
            if (!budget || budget <= 0) {
                Message: 'Budget must be a positive number',
                ResultCode: 'INVALID_BUDGET'
                Message: 'Failed to initialize Facebook OAuth connection',
            const duration = this.getParamValue(Params, 'Duration') as number || 7;
            const objective = this.getParamValue(Params, 'Objective') as string || 'POST_ENGAGEMENT';
            const audienceType = this.getParamValue(Params, 'AudienceType') as string || 'AUTO';
            const targetingSpec = this.getParamValue(Params, 'TargetingSpec') as any;
            const startTime = this.getParamValue(Params, 'StartTime') as string;
            const callToAction = this.getParamValue(Params, 'CallToAction') as string;
            // Validate duration
            if (duration < 1 || duration > 30) {
                Message: 'Duration must be between 1 and 30 days',
                ResultCode: 'INVALID_DURATION'
            // Extract page ID from post ID
            const pageId = postId.split('_')[0];
            // Get page access token
            LogStatus(`Creating boost campaign for post ${postId}...`);
            // Step 1: Create campaign
            const campaignName = `Boost Post ${postId} - ${new Date().toISOString()}`;
            const campaign = await this.createCampaign(adAccountId, campaignName, objective);
            // Step 2: Create ad set with targeting
            const adSetName = `Ad Set for ${postId}`;
            const targeting = this.buildTargeting(audienceType, targetingSpec, pageId);
            const adSet = await this.createAdSet(
                adAccountId,
                campaign.id,
                adSetName,
                budget,
                targeting,
            // Step 3: Create ad creative from post
            const creative = await this.createCreativeFromPost(adAccountId, postId, pageToken, callToAction);
            // Step 4: Create the ad
            const adName = `Boosted Post ${postId}`;
            const ad = await this.createAd(adAccountId, adSet.id, creative.id, adName);
            // Get boost summary
            const boostSummary = {
                campaignId: campaign.id,
                adSetId: adSet.id,
                adId: ad.id,
                creativeId: creative.id,
                postId,
                objective,
                audienceType,
                startTime: adSet.start_time,
                endTime: adSet.end_time,
                status: ad.status,
                reviewStatus: ad.review_feedback?.global_review_status || 'PENDING',
                previewUrl: `https://www.facebook.com/ads/manager/account/campaigns?act=${adAccountId}&selected_campaign_ids=${campaign.id}`
            LogStatus(`Successfully created boost campaign for post ${postId}`);
                Message: 'Post boost created successfully',
            LogError(`Failed to boost Facebook post: ${error instanceof Error ? error.message : 'Unknown error'}`);
            if (this.isAuthError(error)) {
                return this.handleOAuthError(error);
            // Check for specific ad-related errors
                if (error.message.includes('permissions')) {
                Message: 'Insufficient permissions. Ensure the token has ads_management permission.',
                ResultCode: 'INSUFFICIENT_PERMISSIONS'
                if (error.message.includes('budget')) {
                Message: 'Invalid budget. Check minimum budget requirements for your currency.',
                Message: error instanceof Error ? error.message : 'Unknown error occurred',
                ResultCode: 'ERROR'
     * Create a campaign
    private async createCampaign(adAccountId: string, name: string, objective: string): Promise<any> {
        const response = await this.axiosInstance.post(
            `/${adAccountId}/campaigns`,
                status: 'PAUSED', // Start paused for safety
                special_ad_categories: [] // Required field
     * Create an ad set
    private async createAdSet(
        adAccountId: string,
        campaignId: string,
        budget: number,
        durationDays: number,
        targeting: any,
        startTime?: string
        const start = startTime ? new Date(startTime) : now;
        const end = new Date(start);
        end.setDate(end.getDate() + durationDays);
        // Calculate daily budget
        const dailyBudget = Math.ceil((budget * 100) / durationDays); // Convert to cents
            `/${adAccountId}/adsets`,
                campaign_id: campaignId,
                daily_budget: dailyBudget,
                billing_event: 'IMPRESSIONS',
                optimization_goal: this.getOptimizationGoal(campaignId),
                bid_strategy: 'LOWEST_COST_WITHOUT_CAP',
                start_time: start.toISOString(),
                end_time: end.toISOString(),
                status: 'PAUSED'
     * Create creative from existing post
    private async createCreativeFromPost(
        postId: string,
        pageToken: string,
        callToAction?: string
        const creativeData: any = {
            name: `Creative for ${postId}`,
            object_story_id: postId
        if (callToAction) {
            // Get post details to add CTA
            const postResponse = await axios.get(
                `${this.apiBaseUrl}/${postId}`,
                        fields: 'permalink_url'
            creativeData.call_to_action = {
                type: callToAction,
                    link: postResponse.data.permalink_url
            `/${adAccountId}/adcreatives`,
            creativeData
     * Create the ad
    private async createAd(
        adSetId: string,
        creativeId: string,
            `/${adAccountId}/ads`,
                adset_id: adSetId,
                creative: { creative_id: creativeId },
     * Build targeting specification
    private buildTargeting(audienceType: string, customTargeting: any, pageId: string): any {
        const baseTargeting: any = {
            geo_locations: {
                countries: ['US'] // Default to US, can be overridden
        switch (audienceType) {
            case 'FANS':
                baseTargeting.connections = [pageId];
            case 'FANS_AND_CONNECTIONS':
                baseTargeting.friends_of_connections = [pageId];
            case 'CUSTOM':
                if (customTargeting) {
                    return { ...baseTargeting, ...customTargeting };
            case 'AUTO':
                // Facebook will automatically optimize targeting
        // Add any custom targeting on top
        if (customTargeting && audienceType !== 'CUSTOM') {
            Object.assign(baseTargeting, customTargeting);
        return baseTargeting;
     * Get optimization goal based on objective
    private getOptimizationGoal(objective: string): string {
        const goalMap: Record<string, string> = {
            'POST_ENGAGEMENT': 'POST_ENGAGEMENT',
            'REACH': 'REACH',
            'LINK_CLICKS': 'LINK_CLICKS',
            'PAGE_LIKES': 'PAGE_LIKES',
            'BRAND_AWARENESS': 'AD_RECALL_LIFT',
            'VIDEO_VIEWS': 'VIDEO_VIEWS'
        return goalMap[objective] || 'POST_ENGAGEMENT';
