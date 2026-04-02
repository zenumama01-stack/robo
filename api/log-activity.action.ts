 * Action to log activities (calls, emails, meetings, notes) in HubSpot
@RegisterClass(BaseAction, 'LogActivityAction')
export class LogActivityAction extends HubSpotBaseAction {
     * Log an activity (engagement) in HubSpot
            const activityType = this.getParamValue(Params, 'ActivityType');
            const status = this.getParamValue(Params, 'Status') || 'COMPLETED';
            const activityDate = this.getParamValue(Params, 'ActivityDate');
            const durationMilliseconds = this.getParamValue(Params, 'DurationMilliseconds');
            const metadata = this.getParamValue(Params, 'Metadata');
            if (!activityType) {
                    Message: 'ActivityType is required',
            const validTypes = ['EMAIL', 'CALL', 'MEETING', 'NOTE'];
            if (!validTypes.includes(activityType.toUpperCase())) {
                    Message: `Invalid ActivityType. Must be one of: ${validTypes.join(', ')}`,
            // Prepare engagement properties based on type
            const engagementProperties: any = {
                type: activityType.toUpperCase()
            // Set activity date (default to now)
            const timestamp = activityDate ? new Date(activityDate).getTime() : Date.now();
            engagementProperties.timestamp = timestamp;
            // Add common properties
            if (ownerId) engagementProperties.ownerId = ownerId;
            // Prepare metadata based on activity type
            const engagementMetadata: any = {};
            switch (activityType.toUpperCase()) {
                    engagementMetadata.subject = subject || 'Email activity';
                    engagementMetadata.html = body || '';
                    engagementMetadata.status = status;
                    if (metadata?.from) engagementMetadata.from = metadata.from;
                    if (metadata?.to) engagementMetadata.to = metadata.to;
                    if (metadata?.cc) engagementMetadata.cc = metadata.cc;
                    if (metadata?.bcc) engagementMetadata.bcc = metadata.bcc;
                    engagementMetadata.title = subject || 'Call activity';
                    engagementMetadata.body = body || '';
                    if (durationMilliseconds) engagementMetadata.durationMilliseconds = durationMilliseconds;
                    if (metadata?.toNumber) engagementMetadata.toNumber = metadata.toNumber;
                    if (metadata?.fromNumber) engagementMetadata.fromNumber = metadata.fromNumber;
                    if (metadata?.recordingUrl) engagementMetadata.recordingUrl = metadata.recordingUrl;
                    engagementMetadata.title = subject || 'Meeting';
                    engagementMetadata.startTime = timestamp;
                    if (durationMilliseconds) {
                        engagementMetadata.endTime = timestamp + durationMilliseconds;
                    if (metadata?.location) engagementMetadata.location = metadata.location;
                    if (metadata?.meetingOutcome) engagementMetadata.meetingOutcome = metadata.meetingOutcome;
                    engagementMetadata.body = body || subject || 'Note';
            // Prepare associations
            const associations: any = {};
                associations.contactIds = contactIds;
                associations.companyIds = companyIds;
                associations.dealIds = dealIds;
            // Create the engagement
            const engagementBody = {
                engagement: engagementProperties,
                associations,
                metadata: engagementMetadata
                'engagements',
                engagementBody,
            // Format activity details
            const activityDetails = {
                engagementId: engagement.engagement.id,
                type: engagement.engagement.type,
                subject: subject || engagementMetadata.title || engagementMetadata.body,
                portalUrl: `https://app.hubspot.com/contacts/engagements/${engagement.engagement.id}`,
                activityId: activityDetails.engagementId,
                type: activityDetails.type,
                subject: activityDetails.subject,
                timestamp: activityDetails.timestamp,
                associatedContacts: contactIds?.length || 0,
                associatedCompanies: companyIds?.length || 0,
                associatedDeals: dealIds?.length || 0,
                portalUrl: activityDetails.portalUrl
            const activityDetailsParam = outputParams.find(p => p.Name === 'ActivityDetails');
            if (activityDetailsParam) activityDetailsParam.Value = activityDetails;
                Message: `Successfully logged ${activityType.toLowerCase()} activity`,
                Message: `Error logging activity: ${errorMessage}`,
                Name: 'ActivityType',
                Value: 'COMPLETED'
                Name: 'ActivityDate',
                Name: 'DurationMilliseconds',
                Name: 'Metadata',
                Name: 'ActivityDetails',
        return 'Logs activities (calls, emails, meetings, notes) in HubSpot with optional associations to contacts, companies, and deals';
