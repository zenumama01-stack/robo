 * Action to get activity history for a contact in HubSpot
@RegisterClass(BaseAction, 'GetActivitiesByContactAction')
export class GetActivitiesByContactAction extends HubSpotBaseAction {
     * Get activity history for a specific contact
            const activityTypes = this.getParamValue(Params, 'ActivityTypes');
            const includeCompleted = this.getParamValue(Params, 'IncludeCompleted') !== false;
            const includeScheduled = this.getParamValue(Params, 'IncludeScheduled') !== false;
            const startDate = this.getParamValue(Params, 'StartDate');
            const endDate = this.getParamValue(Params, 'EndDate');
            const maxResults = this.getParamValue(Params, 'MaxResults') || 100;
            if (!contactId) {
                    Message: 'ContactId is required',
            // First, get the contact to ensure it exists
            let contact;
                contact = await this.makeHubSpotRequest<any>(
                    Message: `Contact with ID ${contactId} not found`,
            // Get all activities (engagements and tasks) associated with the contact
            const activities: any[] = [];
            // Get engagements (emails, calls, meetings, notes)
            const engagementAssociations = await this.makeHubSpotRequest<any>(
                `objects/contacts/${contactId}/associations/engagements`,
            if (engagementAssociations.results && engagementAssociations.results.length > 0) {
                // Fetch engagement details
                for (const association of engagementAssociations.results) {
                        const engagement = await this.makeHubSpotRequest<any>(
                            `engagements/${association.id}`,
                        // Check if we should include this engagement based on filters
                        const engagementType = engagement.engagement.type;
                        if (activityTypes && activityTypes.length > 0 && 
                            !activityTypes.includes(engagementType) && 
                            !activityTypes.includes('ALL')) {
                        // Check date filters
                        const activityDate = new Date(engagement.engagement.timestamp);
                        if (startDate && activityDate < new Date(startDate)) continue;
                        if (endDate && activityDate > new Date(endDate)) continue;
                        // Format engagement data
                        const activity = {
                            type: 'engagement',
                            activityType: engagementType,
                            id: engagement.engagement.id,
                            timestamp: new Date(engagement.engagement.timestamp).toISOString(),
                            subject: this.getEngagementSubject(engagement),
                            body: this.getEngagementBody(engagement),
                            status: engagement.metadata.status || 'COMPLETED',
                            ownerId: engagement.engagement.ownerId,
                            createdAt: new Date(engagement.engagement.createdAt).toISOString(),
                            updatedAt: new Date(engagement.engagement.updatedAt).toISOString(),
                            metadata: engagement.metadata,
                            associations: engagement.associations
                        activities.push(activity);
                        // Skip individual engagement errors
                        console.error(`Failed to fetch engagement ${association.id}:`, error);
            // Get tasks associated with the contact
            const taskAssociations = await this.makeHubSpotRequest<any>(
                `objects/contacts/${contactId}/associations/tasks`,
            if (taskAssociations.results && taskAssociations.results.length > 0) {
                // Fetch task details
                for (const association of taskAssociations.results) {
                            `objects/tasks/${association.id}`,
                        const taskProperties = task.properties;
                        // Check if we should include tasks
                            !activityTypes.includes('TASK') && 
                        // Check status filters
                        const isCompleted = taskProperties.hs_task_status === 'COMPLETED';
                        if (!includeCompleted && isCompleted) continue;
                        if (!includeScheduled && !isCompleted) continue;
                        const taskDate = taskProperties.hs_timestamp ? 
                            new Date(parseInt(taskProperties.hs_timestamp)) : 
                            new Date(task.createdAt);
                        if (startDate && taskDate < new Date(startDate)) continue;
                        if (endDate && taskDate > new Date(endDate)) continue;
                        // Format task data
                            type: 'task',
                            activityType: 'TASK',
                            id: task.id,
                            timestamp: taskDate.toISOString(),
                            subject: taskProperties.hs_task_subject,
                            body: taskProperties.hs_task_body,
                            status: taskProperties.hs_task_status,
                            priority: taskProperties.hs_task_priority,
                            dueDate: taskProperties.hs_timestamp ? 
                                new Date(parseInt(taskProperties.hs_timestamp)).toISOString() : null,
                            completedDate: taskProperties.hs_task_completion_date ? 
                                new Date(parseInt(taskProperties.hs_task_completion_date)).toISOString() : null,
                            ownerId: taskProperties.hubspot_owner_id,
                            createdAt: task.createdAt,
                            updatedAt: task.updatedAt,
                            properties: taskProperties
                        // Skip individual task errors
                        console.error(`Failed to fetch task ${association.id}:`, error);
            // Sort activities by timestamp (most recent first)
            activities.sort((a, b) => 
                new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime()
            // Limit results if specified
            const limitedActivities = activities.slice(0, maxResults);
            // Create activity summary
            const activitySummary = {
                totalActivities: limitedActivities.length,
                byType: this.groupActivitiesByType(limitedActivities),
                byStatus: this.groupActivitiesByStatus(limitedActivities),
                dateRange: limitedActivities.length > 0 ? {
                    earliest: limitedActivities[limitedActivities.length - 1].timestamp,
                    latest: limitedActivities[0].timestamp
                contactId: contact.id,
                contactEmail: contact.properties.email,
                contactName: `${contact.properties.firstname || ''} ${contact.properties.lastname || ''}`.trim(),
                activitySummary: activitySummary
            const activitiesParam = outputParams.find(p => p.Name === 'Activities');
            if (activitiesParam) activitiesParam.Value = limitedActivities;
                Message: `Found ${limitedActivities.length} activities for contact ${contact.properties.email}`,
                Message: `Error getting activities: ${errorMessage}`,
     * Extract subject from engagement based on type
    private getEngagementSubject(engagement: any): string {
        const metadata = engagement.metadata;
        switch (engagement.engagement.type) {
            case 'EMAIL':
                return metadata.subject || 'Email';
            case 'CALL':
                return metadata.title || 'Call';
            case 'MEETING':
                return metadata.title || 'Meeting';
            case 'NOTE':
                return 'Note';
                return engagement.engagement.type;
     * Extract body from engagement based on type
    private getEngagementBody(engagement: any): string {
                return metadata.html || metadata.text || '';
                return metadata.body || '';
     * Group activities by type
    private groupActivitiesByType(activities: any[]): Record<string, number> {
        const grouped: Record<string, number> = {};
        for (const activity of activities) {
            const type = activity.activityType;
            grouped[type] = (grouped[type] || 0) + 1;
     * Group activities by status
    private groupActivitiesByStatus(activities: any[]): Record<string, number> {
            const status = activity.status || 'UNKNOWN';
            grouped[status] = (grouped[status] || 0) + 1;
                Name: 'ActivityTypes',
                Name: 'IncludeCompleted',
                Name: 'IncludeScheduled',
                Name: 'Activities',
        return 'Gets activity history (calls, emails, meetings, notes, tasks) for a specific contact in HubSpot';
