 * Action to get a company from HubSpot by ID or domain
@RegisterClass(BaseAction, 'GetCompanyAction')
export class GetCompanyAction extends HubSpotBaseAction {
     * Get a company by ID or domain
            const includeProperties = this.getParamValue(Params, 'IncludeProperties');
            const includeAssociations = this.getParamValue(Params, 'IncludeAssociations');
            const includeContacts = this.getParamValue(Params, 'IncludeContacts');
            const includeDeals = this.getParamValue(Params, 'IncludeDeals');
            if (!companyId && !domain) {
                    Message: 'Either CompanyId or Domain is required',
            let company: any;
            if (companyId) {
                // Get company by ID
                // Add properties to retrieve
                if (includeProperties && Array.isArray(includeProperties)) {
                    queryParams.push(`properties=${includeProperties.join(',')}`);
                // Add associations if requested
                const associations: string[] = [];
                if (includeAssociations || includeContacts) associations.push('contacts');
                if (includeAssociations || includeDeals) associations.push('deals');
                if (associations.length > 0) {
                    queryParams.push(`associations=${associations.join(',')}`);
                const query = queryParams.length > 0 ? `?${queryParams.join('&')}` : '';
                company = await this.makeHubSpotRequest<any>(
                    `objects/companies/${companyId}${query}`,
                // Search by domain
                const searchResult = await this.makeHubSpotRequest<any>(
                    'objects/companies/search',
                            filters: [{
                                propertyName: 'domain',
                                value: domain
                        properties: includeProperties || [],
                        limit: 1
                if (!searchResult || !searchResult.results || searchResult.results.length === 0) {
                        Message: `No company found with domain ${domain}`,
                company = searchResult.results[0];
                // Get associations if requested
                if ((includeAssociations || includeContacts || includeDeals) && company.id) {
                        `objects/companies/${company.id}?associations=${associations.join(',')}`,
            // Process associations if included
            let contactAssociations = null;
            let dealAssociations = null;
            if (company.associations) {
                if (company.associations.contacts && includeContacts) {
                    contactAssociations = company.associations.contacts.results.map((assoc: any) => ({
                        id: assoc.id,
                        type: assoc.type
                if (company.associations.deals && includeDeals) {
                    dealAssociations = company.associations.deals.results.map((assoc: any) => ({
                website: companyDetails.website,
                phone: companyDetails.phone,
                city: companyDetails.city,
                state: companyDetails.state,
                country: companyDetails.country,
                ownerId: companyDetails.hubspot_owner_id,
                updatedAt: companyDetails.updatedAt,
                contactCount: contactAssociations ? contactAssociations.length : 0,
                dealCount: dealAssociations ? dealAssociations.length : 0
            const contactAssociationsParam = outputParams.find(p => p.Name === 'ContactAssociations');
            if (contactAssociationsParam) contactAssociationsParam.Value = contactAssociations;
            const dealAssociationsParam = outputParams.find(p => p.Name === 'DealAssociations');
            if (dealAssociationsParam) dealAssociationsParam.Value = dealAssociations;
                Message: `Successfully retrieved company ${companyDetails.name}`,
            // Check for not found error
                    Message: `Company not found`,
                Message: `Error retrieving company: ${errorMessage}`,
                Name: 'IncludeProperties',
                Name: 'IncludeAssociations',
                Name: 'IncludeContacts',
                Name: 'IncludeDeals',
                Name: 'ContactAssociations',
                Name: 'DealAssociations',
        return 'Retrieves a company from HubSpot by ID or domain with optional associations';
