 * Action to retrieve deal details from HubSpot
@RegisterClass(BaseAction, 'GetDealAction')
export class GetDealAction extends HubSpotBaseAction {
     * Get deal details by ID
            const dealId = this.getParamValue(Params, 'DealId');
            const includeAssociations = this.getParamValue(Params, 'IncludeAssociations') ?? true;
            const includeTimeline = this.getParamValue(Params, 'IncludeTimeline') ?? false;
            if (!dealId) {
                    Message: 'Deal ID is required',
            // Build query parameters
            // Always include all properties
            queryParams.push('properties=*');
            if (includeAssociations) {
                queryParams.push('associations=contacts,companies,line_items,quotes');
            // Get deal details
            const deal = await this.makeHubSpotRequest<any>(
                `objects/deals/${dealId}${queryString}`,
            const dealDetails = this.mapHubSpotProperties(deal);
            if (includeAssociations && deal.associations) {
                associations = {
                    contacts: [],
                    companies: [],
                    lineItems: [],
                    quotes: []
                // Process contact associations
                if (deal.associations.contacts) {
                    associations.contacts = deal.associations.contacts.results.map((assoc: any) => ({
                // Process company associations
                if (deal.associations.companies) {
                    associations.companies = deal.associations.companies.results.map((assoc: any) => ({
                // Process line item associations
                if (deal.associations.line_items) {
                    associations.lineItems = deal.associations.line_items.results.map((assoc: any) => ({
                // Process quote associations
                if (deal.associations.quotes) {
                    associations.quotes = deal.associations.quotes.results.map((assoc: any) => ({
            // Get timeline events if requested
            let timeline = null;
            if (includeTimeline) {
                    const timelineResponse = await this.makeHubSpotRequest<any>(
                        `objects/deals/${dealId}/timeline`,
                    timeline = timelineResponse.results.map((event: any) => ({
                        id: event.id,
                        eventType: event.eventType,
                        occurredAt: event.occurredAt,
                        properties: event.properties
                } catch (timelineError) {
                    // Timeline API might not be available for all accounts
                    console.warn('Unable to fetch timeline:', timelineError);
            // Calculate deal metrics
                daysOpen: null as number | null,
                daysInCurrentStage: null as number | null,
                isOverdue: false,
                probability: null as number | null
            // Calculate days open
            if (dealDetails.createdAt) {
                const createdDate = new Date(dealDetails.createdAt);
                metrics.daysOpen = Math.floor((now.getTime() - createdDate.getTime()) / (1000 * 60 * 60 * 24));
            // Check if overdue
            if (dealDetails.closedate) {
                const closeDate = new Date(dealDetails.closedate);
                metrics.isOverdue = closeDate < now && !['closedwon', 'closedlost'].includes(dealDetails.dealstage?.toLowerCase() || '');
                dealType: dealDetails.dealtype,
                priority: dealDetails.hs_priority,
                owner: dealDetails.hubspot_owner_id,
                updatedAt: dealDetails.updatedAt,
                associations: associations,
                timeline: timeline,
                metrics: metrics
                Message: `Successfully retrieved deal "${dealDetails.dealname}"`,
                    ResultCode: 'DEAL_NOT_FOUND',
                    Message: `Deal with ID ${this.getParamValue(Params, 'DealId')} not found`,
                Message: `Error retrieving deal: ${errorMessage}`,
                Name: 'DealId',
                Name: 'IncludeTimeline',
        return 'Retrieves complete deal information from HubSpot including associations and timeline';
