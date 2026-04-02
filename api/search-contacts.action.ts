 * Action to search contacts in HubSpot by various criteria
@RegisterClass(BaseAction, 'SearchContactsAction')
export class SearchContactsAction extends HubSpotBaseAction {
     * Search contacts by criteria
            const searchTerm = this.getParamValue(Params, 'SearchTerm');
            const filters = this.getParamValue(Params, 'Filters');
            const sortBy = this.getParamValue(Params, 'SortBy') || 'createdate';
            const includeArchived = this.getParamValue(Params, 'IncludeArchived') || false;
            // Build search filters
            const searchFilters: any[] = [];
            if (filters && Array.isArray(filters)) {
                searchFilters.push(...filters);
            // Add specific field filters
            if (email) {
                searchFilters.push({
            if (firstName) {
                    propertyName: 'firstname',
                    value: firstName
            if (lastName) {
                    propertyName: 'lastname',
                    value: lastName
            if (company) {
                    propertyName: 'company',
                    value: company
            if (leadStatus) {
                    propertyName: 'lead_status',
                    value: leadStatus
            // Use general search term if no specific filters
            if (searchTerm && searchFilters.length === 0) {
                // Search across multiple fields
                    value: searchTerm
            // Build search body
            const searchBody: any = {
                limit: Math.min(limit, 100), // HubSpot max is 100 per request
                after: 0,
                sorts: [{
                    propertyName: sortBy,
                    direction: sortOrder
                searchBody.properties = includeProperties;
            if (searchFilters.length > 0) {
                searchBody.filterGroups = [{
                    filters: searchFilters
            // Handle archived contacts
            if (!includeArchived) {
                if (!searchBody.filterGroups) {
                    searchBody.filterGroups = [];
                searchBody.filterGroups.push({
                        propertyName: 'archived',
                        value: 'false'
            // Perform search
            const response = await this.makeHubSpotRequest<any>(
                'objects/contacts/search',
            const contacts = response.results || [];
            // Format contact results
            const formattedContacts = contacts.map((contact: any) => {
                const props = this.mapHubSpotProperties(contact);
                    id: props.id,
                    email: props.email,
                    firstName: props.firstname,
                    lastName: props.lastname,
                    fullName: `${props.firstname || ''} ${props.lastname || ''}`.trim(),
                    company: props.company,
                    lifecycleStage: props.lifecyclestage,
                    leadStatus: props.lead_status,
                    createdAt: props.createdAt,
                    updatedAt: props.updatedAt,
                    archived: props.archived
                totalResults: contacts.length,
                hasMore: response.paging?.next?.after ? true : false,
                searchCriteria: {
                    searchTerm: searchTerm,
                    filters: searchFilters.length,
                    sortBy: sortBy,
                    sortOrder: sortOrder,
                    includeArchived: includeArchived
                resultStats: {
                    byLifecycleStage: this.groupBy(formattedContacts, 'lifecycleStage'),
                    byLeadStatus: this.groupBy(formattedContacts, 'leadStatus'),
                    archived: formattedContacts.filter(c => c.archived).length,
                    active: formattedContacts.filter(c => !c.archived).length
            const contactsParam = outputParams.find(p => p.Name === 'Contacts');
            if (contactsParam) contactsParam.Value = formattedContacts;
            const pagingParam = outputParams.find(p => p.Name === 'PagingInfo');
            if (pagingParam) pagingParam.Value = response.paging;
                Message: `Found ${formattedContacts.length} contacts matching criteria`,
                Message: `Error searching contacts: ${errorMessage}`,
     * Helper to group array by property
    private groupBy(array: any[], key: string): Record<string, number> {
        return array.reduce((result, item) => {
            const value = item[key] || 'unknown';
            result[value] = (result[value] || 0) + 1;
        }, {} as Record<string, number>);
                Name: 'SearchTerm',
                Name: 'Filters',
                Value: 'createdate'
                Name: 'IncludeArchived',
                Name: 'Contacts',
                Name: 'PagingInfo',
        return 'Searches contacts in HubSpot using flexible criteria with sorting and filtering';
