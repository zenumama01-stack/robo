 * Action to search for deals in HubSpot
@RegisterClass(BaseAction, 'SearchDealsAction')
export class SearchDealsAction extends HubSpotBaseAction {
     * Search for deals using various criteria
            // Extract search parameters
            const minAmount = this.getParamValue(Params, 'MinAmount');
            const maxAmount = this.getParamValue(Params, 'MaxAmount');
            const closeDateAfter = this.getParamValue(Params, 'CloseDateAfter');
            const closeDateBefore = this.getParamValue(Params, 'CloseDateBefore');
            const includeArchived = this.getParamValue(Params, 'IncludeArchived') ?? false;
            // Build filter groups
            // Add search term filter
            if (searchTerm) {
                    propertyName: 'dealname',
            // Add deal stage filter
            if (dealStage) {
                    propertyName: 'dealstage',
                    value: dealStage
            // Add pipeline filter
            if (pipelineId) {
                    propertyName: 'pipeline',
                    value: pipelineId
            // Add amount range filters
            if (minAmount !== null && minAmount !== undefined) {
                    propertyName: 'amount',
                    value: minAmount.toString()
            if (maxAmount !== null && maxAmount !== undefined) {
                    value: maxAmount.toString()
            // Add close date range filters
            if (closeDateAfter) {
                const dateMs = new Date(closeDateAfter).getTime();
                    propertyName: 'closedate',
                    value: dateMs.toString()
            if (closeDateBefore) {
                const dateMs = new Date(closeDateBefore).getTime();
            // Add deal type filter
            if (dealType) {
                    propertyName: 'dealtype',
                    value: dealType
            // Add priority filter
            if (priority) {
                    propertyName: 'hs_priority',
                    value: priority
            // Add owner filter
            // Add archived filter
                    propertyName: 'hs_is_archived',
            // Add filters to filter group
                filterGroups.push({ filters });
            // Prepare search request
            const searchRequest: any = {
                filterGroups: filterGroups,
                    'hs_lastmodifieddate', 'description', 'hs_object_id'
                after: 0
            // Execute search
                'objects/deals/search',
                searchRequest,
            const deals = searchResults.results.map((deal: any) => this.mapHubSpotProperties(deal));
                totalResults: searchResults.total,
                returnedCount: deals.length,
                averageAmount: 0,
                overdueCount: 0
                // Sum amounts
                    stats.totalAmount += parseFloat(deal.amount);
                // Count by stage
                // Count by pipeline
                if (deal.closedate) {
                    if (closeDate < now && !['closedwon', 'closedlost'].includes(deal.dealstage?.toLowerCase() || '')) {
                        stats.overdueCount++;
            // Calculate average amount
            if (deals.length > 0 && stats.totalAmount > 0) {
                stats.averageAmount = stats.totalAmount / deals.length;
                    searchTerm,
                    dealStage,
                    pipelineId,
                    amountRange: { min: minAmount, max: maxAmount },
                    dateRange: { after: closeDateAfter, before: closeDateBefore },
                    dealType,
                    priority,
                    ownerId
                statistics: stats,
                hasMore: searchResults.total > deals.length,
                nextPageToken: searchResults.paging?.next?.after || null
                Message: `Found ${deals.length} deals out of ${searchResults.total} total`,
                Message: `Error searching deals: ${errorMessage}`,
                Name: 'CloseDateAfter',
                Name: 'CloseDateBefore',
        return 'Searches for deals in HubSpot using multiple criteria with statistics';
