 * Action to delete/archive a contact in HubSpot
@RegisterClass(BaseAction, 'DeleteContactAction')
export class DeleteContactAction extends HubSpotBaseAction {
     * Delete/archive a contact
            const contactId = this.getParamValue(Params, 'ContactId');
            const permanentDelete = this.getParamValue(Params, 'PermanentDelete') || false;
            const archiveOnly = this.getParamValue(Params, 'ArchiveOnly') || true;
            if (!contactId && !email) {
                    Message: 'Either ContactId or Email is required',
            let contactToDelete: any;
            let actualContactId: string;
            // If email provided, search for contact first
            if (email && !contactId) {
                const searchResults = await this.searchHubSpotObjects<any>(
                    [{
                        propertyName: 'email',
                        operator: 'EQ',
                        value: email
                    ['email', 'firstname', 'lastname', 'company'],
                if (searchResults.length === 0) {
                        ResultCode: 'CONTACT_NOT_FOUND',
                        Message: `No contact found with email ${email}`,
                if (searchResults.length > 1) {
                        ResultCode: 'MULTIPLE_CONTACTS_FOUND',
                        Message: `Multiple contacts found with email ${email}. Please use ContactId instead.`,
                contactToDelete = searchResults[0];
                actualContactId = contactToDelete.id;
                actualContactId = contactId;
                // Get contact details before deletion
                    contactToDelete = await this.makeHubSpotRequest<any>(
                        `objects/contacts/${actualContactId}`,
                } catch (getError: any) {
                    if (getError.message.includes('404')) {
                            Message: `Contact with ID ${actualContactId} not found`,
                    throw getError;
            // Store contact details before deletion
            const contactDetails = this.mapHubSpotProperties(contactToDelete);
            const deletionTime = new Date().toISOString();
            // Perform deletion based on parameters
            if (permanentDelete && !archiveOnly) {
                // Permanent deletion (GDPR compliant)
                    `objects/contacts/${actualContactId}/gdpr-delete`,
                // Archive contact (soft delete)
            // Create deletion summary
                contactId: actualContactId,
                deletionType: permanentDelete && !archiveOnly ? 'permanent' : 'archived',
                deletedAt: deletionTime,
                wasActive: !contactDetails.archived,
                lifecycleStageAtDeletion: contactDetails.lifecyclestage,
                lastModifiedAt: contactDetails.updatedAt
            const deletedContactParam = outputParams.find(p => p.Name === 'DeletedContact');
            if (deletedContactParam) deletedContactParam.Value = contactDetails;
            const message = permanentDelete && !archiveOnly 
                ? `Permanently deleted contact ${contactDetails.email || actualContactId}`
                : `Archived contact ${contactDetails.email || actualContactId}`;
            // Check for specific error types
            if (errorMessage.includes('404') || errorMessage.includes('not found')) {
                    Message: 'Contact not found',
            if (errorMessage.includes('403') || errorMessage.includes('forbidden')) {
                    Message: 'Permission denied to delete this contact',
                Message: `Error deleting contact: ${errorMessage}`,
                Name: 'ContactId',
                Name: 'PermanentDelete',
                Name: 'ArchiveOnly',
                Name: 'DeletedContact',
        return 'Deletes or archives a contact in HubSpot by ID or email with GDPR compliance options';
