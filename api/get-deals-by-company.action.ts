 * Action to get all deals associated with a company in HubSpot
@RegisterClass(BaseAction, 'GetDealsByCompanyAction')
export class GetDealsByCompanyAction extends HubSpotBaseAction {
     * Get all deals for a specific company
            const companyId = this.getParamValue(Params, 'HubSpotCompanyId');
            const includeDetails = this.getParamValue(Params, 'IncludeDetails') ?? true;
            const groupByContact = this.getParamValue(Params, 'GroupByContact') ?? false;
            const onlyOpen = this.getParamValue(Params, 'OnlyOpen') ?? false;
            const sortBy = this.getParamValue(Params, 'SortBy') || 'closedate';
            const sortOrder = this.getParamValue(Params, 'SortOrder') || 'DESC';
                    Message: 'HubSpot Company ID is required',
            // First, get the company to verify it exists and get basic info
            let company;
                    `objects/companies/${companyId}?properties=name,domain,industry,numberofemployees,annualrevenue`,
                    ResultCode: 'COMPANY_NOT_FOUND',
                    Message: `Company with ID ${companyId} not found`,
            // Get associated deals
            const associations = await this.makeHubSpotRequest<any>(
                `objects/companies/${companyId}/associations/deals`,
            // Extract deal IDs
            const dealIds = associations.results.map((assoc: any) => assoc.id);
            if (dealIds.length === 0) {
                // No deals found
                const companyInfo = this.mapHubSpotProperties(company);
                    companyName: companyInfo.name,
                    companyDomain: companyInfo.domain,
                    totalDeals: 0,
                    openDeals: 0,
                    closedWonDeals: 0,
                    closedLostDeals: 0,
                    totalValue: 0,
                    potentialValue: 0,
                    deals: []
                const dealsParam = outputParams.find(p => p.Name === 'Deals');
                if (dealsParam) dealsParam.Value = [];
                    Message: `No deals found for company ${companyInfo.name || companyId}`,
            // Get deal details if requested
            let deals: any[] = [];
            if (includeDetails) {
                // Batch request for all deals
                const batchRequest = {
                    inputs: dealIds.map((id: string) => ({
                        id: id,
                        properties: [
                            'dealname', 'dealstage', 'pipeline', 'amount', 'closedate',
                            'dealtype', 'hs_priority', 'hubspot_owner_id', 'createdate',
                            'hs_lastmodifieddate', 'description', 'hs_is_closed',
                            'hs_is_closed_won', 'closed_won_reason', 'closed_lost_reason',
                            'hs_forecast_category', 'hs_forecast_probability'
                        associations: groupByContact ? ['contacts'] : []
                const batchResponse = await this.makeHubSpotRequest<any>(
                    'objects/deals/batch/read',
                    batchRequest,
                deals = batchResponse.results.map((deal: any) => {
                    const dealData = this.mapHubSpotProperties(deal);
                    // Add contact associations if requested
                    if (groupByContact && deal.associations?.contacts) {
                        dealData.contactIds = deal.associations.contacts.results.map((c: any) => c.id);
                    return dealData;
                // Just return basic deal info
                deals = dealIds.map((id: string) => ({ id }));
            // Filter open deals if requested
            if (onlyOpen && includeDetails) {
                deals = deals.filter(deal => !deal.hs_is_closed || deal.hs_is_closed === 'false');
            if (includeDetails && sortBy) {
                deals.sort((a, b) => {
                    const aValue = a[sortBy] || '';
                    const bValue = b[sortBy] || '';
                    if (sortOrder === 'ASC') {
            // Calculate summary statistics
                lostValue: 0,
                forecastValue: 0,
                averageTimeToClose: 0,
                stageBreakdown: {} as Record<string, number>,
                pipelineBreakdown: {} as Record<string, number>,
                forecastCategories: {} as Record<string, number>,
                ownerBreakdown: {} as Record<string, number>
            // Group by contact if requested
            let groupedDeals: Record<string, any[]> = {};
            // Calculate statistics if we have details
                let totalDaysToClose = 0;
                let closedDealsCount = 0;
                deals.forEach((deal: any) => {
                    if (groupByContact && deal.contactIds) {
                        deal.contactIds.forEach((contactId: string) => {
                            if (!groupedDeals[contactId]) {
                                groupedDeals[contactId] = [];
                            groupedDeals[contactId].push(deal);
                    // Count deal states
                    if (deal.hs_is_closed_won === 'true') {
                        stats.closedWonDeals++;
                    } else if (deal.hs_is_closed === 'true') {
                        stats.closedLostDeals++;
                        stats.openDeals++;
                    // Sum values
                    if (deal.amount) {
                        const amount = parseFloat(deal.amount);
                        stats.totalValue += amount;
                            stats.wonValue += amount;
                            stats.lostValue += amount;
                            stats.potentialValue += amount;
                            // Calculate forecast value based on probability
                            if (deal.hs_forecast_probability) {
                                const probability = parseFloat(deal.hs_forecast_probability) / 100;
                                stats.forecastValue += amount * probability;
                    // Track stages and pipelines
                    if (deal.dealstage) {
                        stats.stageBreakdown[deal.dealstage] = (stats.stageBreakdown[deal.dealstage] || 0) + 1;
                    if (deal.pipeline) {
                        stats.pipelineBreakdown[deal.pipeline] = (stats.pipelineBreakdown[deal.pipeline] || 0) + 1;
                    if (deal.hs_forecast_category) {
                        stats.forecastCategories[deal.hs_forecast_category] = (stats.forecastCategories[deal.hs_forecast_category] || 0) + 1;
                    if (deal.hubspot_owner_id) {
                        stats.ownerBreakdown[deal.hubspot_owner_id] = (stats.ownerBreakdown[deal.hubspot_owner_id] || 0) + 1;
                    // Calculate time to close for closed deals
                    if (deal.hs_is_closed === 'true' && deal.createdate && deal.closedate) {
                        const createDate = new Date(deal.createdate);
                        const closeDate = new Date(deal.closedate);
                        const daysToClose = Math.floor((closeDate.getTime() - createDate.getTime()) / (1000 * 60 * 60 * 24));
                        totalDaysToClose += daysToClose;
                        closedDealsCount++;
                    stats.averageDealSize = stats.totalValue / deals.length;
                if (closedDealsCount > 0) {
                    stats.averageTimeToClose = totalDaysToClose / closedDealsCount;
                companyIndustry: companyInfo.industry,
                companySize: companyInfo.numberofemployees,
                companyRevenue: companyInfo.annualrevenue,
                ...stats,
                winRate: stats.closedWonDeals > 0 ? (stats.closedWonDeals / (stats.closedWonDeals + stats.closedLostDeals)) * 100 : 0,
                dealsByContact: groupByContact ? groupedDeals : null
            if (dealsParam) dealsParam.Value = deals;
                Message: `Found ${deals.length} deals for company ${companyInfo.name || companyId}`,
                Message: `Error retrieving deals for company: ${errorMessage}`,
                Name: 'HubSpotCompanyId',
                Name: 'IncludeDetails',
                Name: 'GroupByContact',
                Name: 'OnlyOpen',
                Value: 'closedate'
                Name: 'SortOrder',
                Value: 'DESC'
                Name: 'Deals',
        return 'Retrieves all deals associated with a specific company including detailed statistics and forecasting';
