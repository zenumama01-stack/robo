 * Action to create a new company in HubSpot
@RegisterClass(BaseAction, 'CreateCompanyAction')
export class CreateCompanyAction extends HubSpotBaseAction {
     * Create a new company
            const name = this.getParamValue(Params, 'Name');
            const domain = this.getParamValue(Params, 'Domain');
            const industry = this.getParamValue(Params, 'Industry');
            const description = this.getParamValue(Params, 'Description');
            const phone = this.getParamValue(Params, 'Phone');
            const website = this.getParamValue(Params, 'Website');
            const address = this.getParamValue(Params, 'Address');
            const city = this.getParamValue(Params, 'City');
            const state = this.getParamValue(Params, 'State');
            const zip = this.getParamValue(Params, 'Zip');
            const country = this.getParamValue(Params, 'Country');
            const numberOfEmployees = this.getParamValue(Params, 'NumberOfEmployees');
            const annualRevenue = this.getParamValue(Params, 'AnnualRevenue');
            const type = this.getParamValue(Params, 'Type');
            const lifecycleStage = this.getParamValue(Params, 'LifecycleStage') || 'lead';
            const ownerId = this.getParamValue(Params, 'OwnerId');
            const customProperties = this.getParamValue(Params, 'CustomProperties');
            const associateWithContactIds = this.getParamValue(Params, 'AssociateWithContactIds');
                    Message: 'Company name is required',
            // Prepare company properties
            const properties: any = {
                lifecyclestage: lifecycleStage
            // Add optional fields
            if (domain) properties.domain = domain;
            if (industry) properties.industry = industry;
            if (description) properties.description = description;
            if (phone) properties.phone = this.formatPhoneNumber(phone);
            if (website) properties.website = website;
            if (address) properties.address = address;
            if (city) properties.city = city;
            if (state) properties.state = state;
            if (zip) properties.zip = zip;
            if (country) properties.country = country;
            if (numberOfEmployees) properties.numberofemployees = numberOfEmployees;
            if (annualRevenue) properties.annualrevenue = annualRevenue;
            if (type) properties.type = type;
            if (ownerId) properties.hubspot_owner_id = ownerId;
            // Add custom properties if provided
            if (customProperties && typeof customProperties === 'object') {
                Object.assign(properties, customProperties);
            // Create company
            const newCompany = await this.makeHubSpotRequest<any>(
                'objects/companies',
                { properties },
            // Format company details
            const companyDetails = this.mapHubSpotProperties(newCompany);
            // Associate with contacts if requested
            let associationResults = null;
            if (associateWithContactIds && Array.isArray(associateWithContactIds)) {
                associationResults = [];
                for (const contactId of associateWithContactIds) {
                            newCompany.id,
                        associationResults.push({
                            message: 'Successfully associated'
                    } catch (assocError) {
                            error: assocError instanceof Error ? assocError.message : 'Association failed'
                companyId: companyDetails.id,
                name: companyDetails.name,
                domain: companyDetails.domain,
                industry: companyDetails.industry,
                lifecycleStage: companyDetails.lifecyclestage,
                numberOfEmployees: companyDetails.numberofemployees,
                annualRevenue: companyDetails.annualrevenue,
                createdAt: companyDetails.createdAt,
                portalUrl: `https://app.hubspot.com/contacts/${companyDetails.id}`,
                associationResults: associationResults
                Message: `Successfully created company ${companyDetails.name}`,
            // Check for duplicate company error
            if (errorMessage.includes('Company already exists') || errorMessage.includes('CONFLICT')) {
                    ResultCode: 'DUPLICATE_COMPANY',
                    Message: `Company with name ${this.getParamValue(Params, 'Name')} may already exist`,
                Message: `Error creating company: ${errorMessage}`,
                Name: 'Name',
                Name: 'Domain',
                Name: 'Industry',
                Name: 'Description',
                Name: 'Phone',
                Name: 'Website',
                Name: 'Address',
                Name: 'City',
                Name: 'State',
                Name: 'Zip',
                Name: 'Country',
                Name: 'NumberOfEmployees',
                Name: 'AnnualRevenue',
                Name: 'Type',
                Name: 'LifecycleStage',
                Value: 'lead'
                Name: 'OwnerId',
                Name: 'CustomProperties',
                Name: 'AssociateWithContactIds',
        return 'Creates a new company in HubSpot with optional contact associations';
