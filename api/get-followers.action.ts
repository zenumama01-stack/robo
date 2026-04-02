import { LinkedInBaseAction } from '../linkedin-base.action';
 * Action to get follower statistics for LinkedIn profiles and organizations
@RegisterClass(BaseAction, 'LinkedInGetFollowersAction')
export class LinkedInGetFollowersAction extends LinkedInBaseAction {
     * Get follower statistics for personal profile or organization
            const entityType = this.getParamValue(Params, 'EntityType') || 'organization'; // 'personal' or 'organization'
            const includeGrowth = this.getParamValue(Params, 'IncludeGrowth') === true;
            const timeRange = this.getParamValue(Params, 'TimeRange'); // Optional: {start: Date, end: Date}
            let followerData: any = {};
            if (entityType === 'organization') {
                // Get organization follower statistics
                let organizationUrn: string;
                    organizationUrn = orgs[0].urn;
                    organizationUrn = `urn:li:organization:${organizationId}`;
                followerData = await this.getOrganizationFollowers(organizationUrn, includeGrowth, timeRange);
                // Get personal profile follower statistics
                const userUrn = await this.getCurrentUserUrn();
                followerData = await this.getPersonalFollowers(userUrn);
            const followersParam = outputParams.find(p => p.Name === 'FollowerCount');
            if (followersParam) followersParam.Value = followerData.followerCount || 0;
            const statsParam = outputParams.find(p => p.Name === 'FollowerStatistics');
            if (statsParam) statsParam.Value = followerData;
                Message: `Successfully retrieved follower statistics`,
                Message: `Failed to get followers: ${errorMessage}`,
     * Get organization follower statistics
    private async getOrganizationFollowers(organizationUrn: string, includeGrowth: boolean, timeRange?: any): Promise<any> {
                q: 'organizationalEntity',
                organizationalEntity: organizationUrn
            // Add time range if specified for growth metrics
            if (includeGrowth && timeRange) {
                params.timeIntervals = `List((start:${new Date(timeRange.start).getTime()},end:${new Date(timeRange.end || Date.now()).getTime()}))`;
            // Get follower statistics
            const response = await this.axiosInstance.get('/organizationalEntityFollowerStatistics', { params });
            const stats = response.data.elements?.[0] || {};
            const result: any = {
                followerCount: stats.followerCounts?.organicFollowerCount || 0,
                paidFollowerCount: stats.followerCounts?.paidFollowerCount || 0,
                totalFollowerCount: (stats.followerCounts?.organicFollowerCount || 0) + (stats.followerCounts?.paidFollowerCount || 0)
            // Add growth metrics if requested
            if (includeGrowth && stats.followerGains) {
                result.followerGrowth = {
                    organicGains: stats.followerGains.organicFollowerGains || 0,
                    paidGains: stats.followerGains.paidFollowerGains || 0,
                    totalGains: (stats.followerGains.organicFollowerGains || 0) + (stats.followerGains.paidFollowerGains || 0),
                    timeRange: timeRange
            // Get demographics if available
                const demographicsResponse = await this.axiosInstance.get('/organizationalEntityFollowerStatistics', {
                        organizationalEntity: organizationUrn,
                        projection: '(followerCountsByAssociationType,followerCountsByFunction,followerCountsBySeniority,followerCountsByIndustry,followerCountsByRegion,followerCountsByCountry)'
                if (demographicsResponse.data.elements?.[0]) {
                    const demographics = demographicsResponse.data.elements[0];
                    result.demographics = {
                        byFunction: demographics.followerCountsByFunction || [],
                        bySeniority: demographics.followerCountsBySeniority || [],
                        byIndustry: demographics.followerCountsByIndustry || [],
                        byRegion: demographics.followerCountsByRegion || [],
                        byCountry: demographics.followerCountsByCountry || []
                LogError(`Failed to get follower demographics: ${error instanceof Error ? error.message : 'Unknown error'}`);
            LogError(`Failed to get organization followers: ${error instanceof Error ? error.message : 'Unknown error'}`);
     * Get personal profile follower statistics
     * Note: LinkedIn provides limited follower data for personal profiles
    private async getPersonalFollowers(userUrn: string): Promise<any> {
            // Get basic profile information including follower count
            const response = await this.axiosInstance.get('/me', {
                    projection: '(id,firstName,lastName,headline,publicProfileUrl,followerCount)'
                followerCount: response.data.followerCount || 0,
                profileInfo: {
                    name: `${response.data.firstName?.localized?.en_US || ''} ${response.data.lastName?.localized?.en_US || ''}`.trim(),
                    headline: response.data.headline?.localized?.en_US || '',
                    profileUrl: response.data.publicProfileUrl || ''
                note: 'LinkedIn API provides limited follower statistics for personal profiles'
            LogError(`Failed to get personal followers: ${error instanceof Error ? error.message : 'Unknown error'}`);
            // Fallback: return basic info
                followerCount: 0,
                note: 'Unable to retrieve follower count for personal profile',
                Name: 'EntityType',
                Value: 'organization' // 'personal' or 'organization'
                Name: 'IncludeGrowth',
                Name: 'TimeRange',
                Value: null // Optional: {start: Date, end: Date}
                Name: 'FollowerCount',
                Name: 'FollowerStatistics',
        return 'Retrieves follower statistics for LinkedIn personal profiles or organization pages, including demographics and growth metrics where available';
