 * Action to search for companies in HubSpot
@RegisterClass(BaseAction, 'SearchCompaniesAction')
export class SearchCompaniesAction extends HubSpotBaseAction {
     * Search for companies
            const searchQuery = this.getParamValue(Params, 'SearchQuery');
            const lifecycleStage = this.getParamValue(Params, 'LifecycleStage');
            const minEmployees = this.getParamValue(Params, 'MinEmployees');
            const maxEmployees = this.getParamValue(Params, 'MaxEmployees');
            const minRevenue = this.getParamValue(Params, 'MinRevenue');
            const maxRevenue = this.getParamValue(Params, 'MaxRevenue');
            const createdAfter = this.getParamValue(Params, 'CreatedAfter');
            const createdBefore = this.getParamValue(Params, 'CreatedBefore');
            const updatedAfter = this.getParamValue(Params, 'UpdatedAfter');
            const updatedBefore = this.getParamValue(Params, 'UpdatedBefore');
            const customFilters = this.getParamValue(Params, 'CustomFilters');
            const limit = this.getParamValue(Params, 'Limit') || 100;
            const after = this.getParamValue(Params, 'After');
            const properties = this.getParamValue(Params, 'Properties');
            const sorts = this.getParamValue(Params, 'Sorts');
            // Build filters
            const filters: any[] = [];
            // Add search query if provided
                filters.push({
                    propertyName: 'name',
                    operator: 'CONTAINS_TOKEN',
                    value: searchQuery
            // Add specific filters
                    value: name
            if (domain) {
            if (industry) {
                    propertyName: 'industry',
                    value: industry
            if (city) {
                    propertyName: 'city',
                    value: city
                    propertyName: 'state',
                    value: state
            if (country) {
                    propertyName: 'country',
                    value: country
            if (lifecycleStage) {
                    propertyName: 'lifecyclestage',
                    value: lifecycleStage
            if (minEmployees != null) {
                    propertyName: 'numberofemployees',
                    value: minEmployees.toString()
            if (maxEmployees != null) {
                    operator: 'LTE',
                    value: maxEmployees.toString()
            if (minRevenue != null) {
                    propertyName: 'annualrevenue',
                    value: minRevenue.toString()
            if (maxRevenue != null) {
                    value: maxRevenue.toString()
            // Date filters
            if (createdAfter) {
                    propertyName: 'createdate',
                    value: new Date(createdAfter).getTime().toString()
            if (createdBefore) {
                    value: new Date(createdBefore).getTime().toString()
            if (updatedAfter) {
                    propertyName: 'lastmodifieddate',
                    value: new Date(updatedAfter).getTime().toString()
            if (updatedBefore) {
                    value: new Date(updatedBefore).getTime().toString()
            // Add custom filters if provided
            if (customFilters && Array.isArray(customFilters)) {
                filters.push(...customFilters);
                limit: Math.min(limit, 100), // HubSpot max is 100
                properties: properties || ['name', 'domain', 'industry', 'city', 'state', 'country', 
                                         'lifecyclestage', 'numberofemployees', 'annualrevenue', 
                                         'createdate', 'lastmodifieddate', 'hubspot_owner_id']
            // Add filters if any
            if (filters.length > 0) {
                requestBody.filterGroups = [{
            // Add sorting if provided
            if (sorts && Array.isArray(sorts)) {
                requestBody.sorts = sorts;
                // Default sort by last modified date descending
                requestBody.sorts = [{
                    direction: 'DESCENDING'
            // Add pagination if provided
                requestBody.after = after;
            // Search companies
                requestBody,
            // Process results
            const companies = searchResult.results.map((company: any) => 
                this.mapHubSpotProperties(company)
                totalResults: searchResult.total || companies.length,
                returnedResults: companies.length,
                hasMore: searchResult.paging && searchResult.paging.next ? true : false,
                nextCursor: searchResult.paging?.next?.after || null,
                avgEmployees: companies.length > 0 
                    ? Math.round(companies.reduce((sum: number, c: any) => sum + (c.numberofemployees || 0), 0) / companies.length) 
                avgRevenue: companies.length > 0 
                    ? Math.round(companies.reduce((sum: number, c: any) => sum + (c.annualrevenue || 0), 0) / companies.length) 
                industries: [...new Set(companies.map((c: any) => c.industry).filter(Boolean))],
                states: [...new Set(companies.map((c: any) => c.state).filter(Boolean))],
                countries: [...new Set(companies.map((c: any) => c.country).filter(Boolean))]
                query: searchQuery || 'Custom filters',
                totalFound: stats.totalResults,
                returned: stats.returnedResults,
                hasMore: stats.hasMore,
                nextCursor: stats.nextCursor,
                filters: {
                    domain,
                    industry,
                    city,
                    country,
                    lifecycleStage,
                    minEmployees,
                    maxEmployees,
                    minRevenue,
                    maxRevenue,
                    ownerId,
                    createdAfter,
                    createdBefore,
                    updatedAfter,
                    updatedBefore,
                    customFiltersCount: customFilters ? customFilters.length : 0
                statistics: stats
            const companiesParam = outputParams.find(p => p.Name === 'Companies');
            if (companiesParam) companiesParam.Value = companies;
            const statisticsParam = outputParams.find(p => p.Name === 'Statistics');
            if (statisticsParam) statisticsParam.Value = stats;
                Message: `Found ${stats.totalResults} companies, returned ${stats.returnedResults}`,
                Message: `Error searching companies: ${errorMessage}`,
                Name: 'SearchQuery',
                Name: 'MinEmployees',
                Name: 'MaxEmployees',
                Name: 'MinRevenue',
                Name: 'MaxRevenue',
                Name: 'CreatedAfter',
                Name: 'CreatedBefore',
                Name: 'UpdatedAfter',
                Name: 'UpdatedBefore',
                Name: 'CustomFilters',
                Name: 'Limit',
                Name: 'After',
                Name: 'Properties',
                Name: 'Sorts',
                Name: 'Companies',
                Name: 'Statistics',
        return 'Searches for companies in HubSpot using various filters and criteria';
