 * Action to get all Buffer profiles (social media accounts) for the authenticated user
@RegisterClass(BaseAction, 'BufferGetProfilesAction')
export class BufferGetProfilesAction extends BufferBaseAction {
     * Get Buffer profiles
            // Get company integration ID
            // Get profiles
            // Format profile data
            const formattedProfiles = profiles.map(profile => ({
                id: profile.id,
                service: profile.service,
                serviceId: profile.service_id,
                serviceUsername: profile.service_username,
                serviceType: profile.service_type,
                default: profile.default,
                createdAt: new Date(profile.created_at * 1000),
                formattedUsername: profile.formatted_username,
                formattedServiceType: profile.formatted_service,
                avatar: profile.avatar,
                avatarHttps: profile.avatar_https,
                    followers: profile.statistics?.followers || 0
                timezone: profile.timezone,
                schedules: profile.schedules || []
                totalProfiles: formattedProfiles.length,
                profilesByService: this.groupByService(formattedProfiles),
                defaultProfile: formattedProfiles.find(p => p.default)?.id
            const profilesParam = outputParams.find(p => p.Name === 'Profiles');
            if (profilesParam) profilesParam.Value = formattedProfiles;
                Message: `Retrieved ${formattedProfiles.length} Buffer profiles`,
                Message: `Failed to get Buffer profiles: ${errorMessage}`,
     * Group profiles by service
    private groupByService(profiles: any[]): Record<string, number> {
        return profiles.reduce((acc, profile) => {
            const service = profile.service || 'unknown';
            acc[service] = (acc[service] || 0) + 1;
                Name: 'Profiles',
        return 'Retrieves all Buffer profiles (social media accounts) associated with the authenticated user';
