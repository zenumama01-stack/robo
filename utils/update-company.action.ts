 * Action to update an existing company in HubSpot
@RegisterClass(BaseAction, 'UpdateCompanyAction')
export class UpdateCompanyAction extends HubSpotBaseAction {
     * Update an existing company
            // Prepare update properties - only include fields that have values
            const properties: any = {};
            // Add fields only if they have values (not null or undefined)
            if (name != null) properties.name = name;
            if (domain != null) properties.domain = domain;
            if (industry != null) properties.industry = industry;
            if (description != null) properties.description = description;
            if (phone != null) properties.phone = this.formatPhoneNumber(phone);
            if (website != null) properties.website = website;
            if (address != null) properties.address = address;
            if (city != null) properties.city = city;
            if (state != null) properties.state = state;
            if (zip != null) properties.zip = zip;
            if (country != null) properties.country = country;
            if (numberOfEmployees != null) properties.numberofemployees = numberOfEmployees;
            if (annualRevenue != null) properties.annualrevenue = annualRevenue;
            if (type != null) properties.type = type;
            if (lifecycleStage != null) properties.lifecyclestage = lifecycleStage;
            if (ownerId != null) properties.hubspot_owner_id = ownerId;
            // Check if there are any properties to update
            if (Object.keys(properties).length === 0) {
                    Message: 'No properties provided to update',
            // Get current company data
            const currentCompany = await this.makeHubSpotRequest<any>(
            // Update company
            const updatedCompany = await this.makeHubSpotRequest<any>(
            const companyDetails = this.mapHubSpotProperties(updatedCompany);
            const previousDetails = this.mapHubSpotProperties(currentCompany);
            // Create summary of changes
            const changes: any = {};
            for (const key of Object.keys(properties)) {
                const oldValue = previousDetails[key];
                const newValue = companyDetails[key];
                if (oldValue !== newValue) {
                    changes[key] = {
                        from: oldValue,
                        to: newValue
                changes: changes,
                fieldsUpdated: Object.keys(changes).length
            const previousDetailsParam = outputParams.find(p => p.Name === 'PreviousDetails');
            if (previousDetailsParam) previousDetailsParam.Value = previousDetails;
                Message: `Successfully updated company ${companyDetails.name}. Updated ${summary.fieldsUpdated} field(s)`,
                Message: `Error updating company: ${errorMessage}`,
                Name: 'PreviousDetails',
        return 'Updates an existing company in HubSpot with provided properties';
