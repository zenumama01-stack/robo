import { HootSuiteBaseAction, HootSuiteProfile } from '../hootsuite-base.action';
 * Action to retrieve social profiles connected to HootSuite account
@RegisterClass(BaseAction, 'HootSuiteGetSocialProfilesAction')
export class HootSuiteGetSocialProfilesAction extends HootSuiteBaseAction {
     * Get social profiles from HootSuite
            const includeInactive = this.getParamValue(Params, 'IncludeInactive') || false;
            const socialNetwork = this.getParamValue(Params, 'SocialNetwork');
            // Get all profiles
            LogStatus('Fetching social profiles...');
            // Filter profiles based on parameters
            let filteredProfiles = profiles;
            // Filter by social network if specified
            if (socialNetwork) {
                filteredProfiles = filteredProfiles.filter(p => 
                    p.socialNetworkId.toLowerCase() === socialNetwork.toLowerCase()
            // Process profiles to add additional information
            const enrichedProfiles = await Promise.all(
                filteredProfiles.map(async (profile) => {
                        // Get additional profile details if available
                        const details = await this.getProfileDetails(profile.id);
                            displayName: profile.displayName,
                            socialNetwork: this.mapSocialNetworkId(profile.socialNetworkId),
                            socialNetworkId: profile.socialNetworkId,
                            socialNetworkUserId: profile.socialNetworkUserId,
                            avatarUrl: profile.avatarUrl,
                            type: profile.type,
                            ownerId: profile.ownerId,
                            isActive: details?.isActive !== false,
                            followerCount: details?.followerCount,
                            followingCount: details?.followingCount,
                            postCount: details?.postCount,
                            profileUrl: details?.profileUrl,
                            verified: details?.verified || false
                        // If we can't get details, return basic info
                        LogStatus(`Could not get details for profile ${profile.id}: ${error instanceof Error ? error.message : 'Unknown error'}`);
                            isActive: true
            // Filter out inactive profiles if requested
            const finalProfiles = includeInactive 
                ? enrichedProfiles 
                : enrichedProfiles.filter(p => p.isActive);
                totalProfiles: finalProfiles.length,
                byNetwork: this.groupByNetwork(finalProfiles),
                byType: this.groupByType(finalProfiles),
                activeProfiles: finalProfiles.filter(p => p.isActive).length,
                inactiveProfiles: finalProfiles.filter(p => !p.isActive).length,
                verifiedProfiles: finalProfiles.filter(p => p.verified).length
            // Store default profile in custom attribute if not set
            if (finalProfiles.length > 0 && !this.getCustomAttribute(1)) {
                const defaultProfile = finalProfiles.find(p => p.isActive) || finalProfiles[0];
                await this.setCustomAttribute(1, defaultProfile.id);
                LogStatus(`Set default profile to: ${defaultProfile.displayName} (${defaultProfile.id})`);
            if (profilesParam) profilesParam.Value = finalProfiles;
                Message: `Retrieved ${finalProfiles.length} social profiles`,
                Message: `Failed to get social profiles: ${errorMessage}`,
     * Get additional details for a profile
    private async getProfileDetails(profileId: string): Promise<any> {
            const response = await this.axiosInstance.get(`/socialProfiles/${profileId}`);
            // Details endpoint might not be available for all profiles
     * Map social network ID to readable name
    private mapSocialNetworkId(networkId: string): string {
        const networkMap: Record<string, string> = {
            'TWITTER': 'Twitter',
            'FACEBOOK': 'Facebook',
            'FACEBOOK_PAGE': 'Facebook Page',
            'INSTAGRAM': 'Instagram',
            'INSTAGRAM_BUSINESS': 'Instagram Business',
            'LINKEDIN': 'LinkedIn',
            'LINKEDIN_COMPANY': 'LinkedIn Company',
            'PINTEREST': 'Pinterest',
            'YOUTUBE': 'YouTube',
            'TIKTOK': 'TikTok'
        return networkMap[networkId.toUpperCase()] || networkId;
     * Group profiles by network
    private groupByNetwork(profiles: any[]): Record<string, number> {
        return profiles.reduce((groups, profile) => {
            const network = profile.socialNetwork;
            groups[network] = (groups[network] || 0) + 1;
     * Group profiles by type
    private groupByType(profiles: any[]): Record<string, number> {
            const type = profile.type || 'Unknown';
            groups[type] = (groups[type] || 0) + 1;
                Name: 'IncludeInactive',
                Name: 'SocialNetwork',
        return 'Retrieves all social profiles connected to the HootSuite account with optional filtering';
