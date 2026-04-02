 * Service for tracking artifact usage events.
 * Creates audit trail records for security and analytics.
export class ArtifactUseTrackingService {
     * Track that a user viewed an artifact
    async TrackViewed(artifactVersionId: string, currentUser: UserInfo, context?: any): Promise<void> {
        await this.trackUsage(artifactVersionId, currentUser, 'Viewed', context);
     * Track that a user opened an artifact (full interaction)
    async TrackOpened(artifactVersionId: string, currentUser: UserInfo, context?: any): Promise<void> {
        await this.trackUsage(artifactVersionId, currentUser, 'Opened', context);
     * Track that a user shared an artifact
    async TrackShared(artifactVersionId: string, currentUser: UserInfo, context?: any): Promise<void> {
        await this.trackUsage(artifactVersionId, currentUser, 'Shared', context);
     * Track that a user saved/bookmarked an artifact
    async TrackSaved(artifactVersionId: string, currentUser: UserInfo, context?: any): Promise<void> {
        await this.trackUsage(artifactVersionId, currentUser, 'Saved', context);
     * Track that a user exported an artifact
    async TrackExported(artifactVersionId: string, currentUser: UserInfo, context?: any): Promise<void> {
        await this.trackUsage(artifactVersionId, currentUser, 'Exported', context);
     * Internal method to create usage record
    private async trackUsage(
        artifactVersionId: string,
        currentUser: UserInfo,
        usageType: 'Viewed' | 'Opened' | 'Shared' | 'Saved' | 'Exported',
        context?: any
                console.warn('Cannot track artifact usage: No current user');
            usage.ArtifactVersionID = artifactVersionId;
            usage.UserID = currentUser.ID;
            usage.UsageContext = context ? JSON.stringify(context) : null;
                await usage.Save();
            console.error('Error in trackUsage:', error);
            // Don't throw - usage tracking should never break the UI
