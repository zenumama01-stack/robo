 * Action to get a contact from HubSpot by ID or email
@RegisterClass(BaseAction, 'GetContactAction')
export class GetContactAction extends HubSpotBaseAction {
     * Get a contact by ID or email
            const includeMemberships = this.getParamValue(Params, 'IncludeMemberships');
            if (contactId && email) {
                    Message: 'Provide either ContactId or Email, not both',
            let contact: any;
            if (contactId) {
                // Get by ID
                const queryParams = new URLSearchParams();
                // Add properties to include
                    queryParams.set('properties', includeProperties.join(','));
                // Add associations to include
                if (includeAssociations && Array.isArray(includeAssociations)) {
                    queryParams.set('associations', includeAssociations.join(','));
                const endpoint = `objects/contacts/${contactId}${queryParams.toString() ? '?' + queryParams.toString() : ''}`;
                // Search by email
                    includeProperties,
                contact = searchResults[0];
            let associations = null;
            if (includeAssociations && contact.associations) {
                associations = contact.associations;
            // Get list memberships if requested
            let memberships = null;
            if (includeMemberships) {
                    const membershipResponse = await this.makeHubSpotRequest<any>(
                        `objects/contacts/${contact.id}/memberships`,
                    memberships = membershipResponse.results || [];
                } catch (membershipError) {
                    // Non-critical error, continue without memberships
                    memberships = {
                        error: 'Failed to retrieve memberships',
                        message: membershipError instanceof Error ? membershipError.message : 'Unknown error'
                leadStatus: contactDetails.lead_status,
                updatedAt: contactDetails.updatedAt,
                isArchived: contactDetails.archived,
                hasAssociations: associations ? Object.keys(associations).length > 0 : false,
                membershipCount: Array.isArray(memberships) ? memberships.length : 0
            const associationsParam = outputParams.find(p => p.Name === 'Associations');
            if (associationsParam) associationsParam.Value = associations;
            const membershipsParam = outputParams.find(p => p.Name === 'Memberships');
            if (membershipsParam) membershipsParam.Value = memberships;
                Message: `Successfully retrieved contact ${contactDetails.email || contactDetails.id}`,
                    Message: `Contact not found`,
                Message: `Error retrieving contact: ${errorMessage}`,
                Name: 'IncludeMemberships',
                Name: 'Associations',
                Name: 'Memberships',
        return 'Retrieves a contact from HubSpot by ID or email with optional associations and memberships';
