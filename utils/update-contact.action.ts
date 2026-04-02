 * Action to update an existing contact in HubSpot
@RegisterClass(BaseAction, 'UpdateContactAction')
export class UpdateContactAction extends HubSpotBaseAction {
     * Update an existing contact
            const clearFields = this.getParamValue(Params, 'ClearFields');
            // Prepare properties to update
            // Add fields to update (only include if provided)
            if (email !== undefined && email !== null) {
                properties.email = email;
            if (firstName !== undefined && firstName !== null) properties.firstname = firstName;
            if (lastName !== undefined && lastName !== null) properties.lastname = lastName;
            if (phone !== undefined && phone !== null) properties.phone = this.formatPhoneNumber(phone);
            if (company !== undefined && company !== null) properties.company = company;
            if (jobTitle !== undefined && jobTitle !== null) properties.jobtitle = jobTitle;
            if (lifecycleStage !== undefined && lifecycleStage !== null) properties.lifecyclestage = lifecycleStage;
            if (leadStatus !== undefined && leadStatus !== null) properties.lead_status = leadStatus;
            if (website !== undefined && website !== null) properties.website = website;
            if (address !== undefined && address !== null) properties.address = address;
            if (city !== undefined && city !== null) properties.city = city;
            if (state !== undefined && state !== null) properties.state = state;
            if (zip !== undefined && zip !== null) properties.zip = zip;
            if (country !== undefined && country !== null) properties.country = country;
            // Clear specific fields if requested
            if (clearFields && Array.isArray(clearFields)) {
                for (const field of clearFields) {
                    properties[field] = '';
                    ResultCode: 'NO_CHANGES',
            // Get current contact data first
            const currentContact = await this.makeHubSpotRequest<any>(
            // Store before state
            const beforeState = this.mapHubSpotProperties(currentContact);
            // Update contact
            const updatedContact = await this.makeHubSpotRequest<any>(
            const afterState = this.mapHubSpotProperties(updatedContact);
            // Create change summary
            const changes: any[] = [];
            for (const key in properties) {
                if (beforeState[key] !== afterState[key]) {
                    changes.push({
                        field: key,
                        oldValue: beforeState[key],
                        newValue: afterState[key]
                contactId: afterState.id,
                email: afterState.email,
                fullName: `${afterState.firstname || ''} ${afterState.lastname || ''}`.trim(),
                fieldsUpdated: Object.keys(properties).length,
                actualChanges: changes.length,
                updatedAt: afterState.updatedAt,
                portalUrl: `https://app.hubspot.com/contacts/${afterState.id}`
            const beforeStateParam = outputParams.find(p => p.Name === 'BeforeState');
            if (beforeStateParam) beforeStateParam.Value = beforeState;
            const afterStateParam = outputParams.find(p => p.Name === 'AfterState');
            if (afterStateParam) afterStateParam.Value = afterState;
                Message: `Successfully updated contact ${afterState.email || contactId}`,
                    Message: `Contact with ID ${this.getParamValue(Params, 'ContactId')} not found`,
                Message: `Error updating contact: ${errorMessage}`,
                Name: 'ClearFields',
                Name: 'BeforeState',
                Name: 'AfterState',
        return 'Updates an existing contact in HubSpot with change tracking';
