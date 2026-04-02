import { HubSpotBaseAction } from '../hubspot-base.action';
 * Action to associate contacts with companies in HubSpot
@RegisterClass(BaseAction, 'AssociateContactToCompanyAction')
export class AssociateContactToCompanyAction extends HubSpotBaseAction {
     * Associate contacts with companies
        const { Params, ContextUser } = params;
        this.params = Params; // Set params for base class to use
            // Extract parameters
            const contactIds = this.getParamValue(Params, 'ContactIds');
            const companyId = this.getParamValue(Params, 'CompanyId');
            const associationType = this.getParamValue(Params, 'AssociationType') || 'contact_to_company';
            const isPrimary = this.getParamValue(Params, 'IsPrimary') || false;
            const jobTitle = this.getParamValue(Params, 'JobTitle');
            const removeExisting = this.getParamValue(Params, 'RemoveExisting') || false;
            const updateContactCompany = this.getParamValue(Params, 'UpdateContactCompany') || true;
            if (!contactIds || (Array.isArray(contactIds) && contactIds.length === 0)) {
                    Message: 'ContactIds is required and must be an array with at least one ID',
                    Params
                    Message: 'CompanyId is required',
            // Ensure contactIds is an array
            const contactIdArray = Array.isArray(contactIds) ? contactIds : [contactIds];
            // Get company details first
            const company = await this.makeHubSpotRequest<any>(
                `objects/companies/${companyId}`,
                ContextUser
            const companyDetails = this.mapHubSpotProperties(company);
            // Process each contact
            const results: any[] = [];
            for (const contactId of contactIdArray) {
                    // Get contact details
                    const contact = await this.makeHubSpotRequest<any>(
                        `objects/contacts/${contactId}`,
                    const contactDetails = this.mapHubSpotProperties(contact);
                    // Remove existing company associations if requested
                    if (removeExisting) {
                            // Get current associations
                            const currentAssociations = await this.makeHubSpotRequest<any>(
                                `objects/contacts/${contactId}/associations/companies`,
                            // Remove each existing association
                            if (currentAssociations && currentAssociations.results) {
                                for (const assoc of currentAssociations.results) {
                                    await this.makeHubSpotRequest<any>(
                                        `objects/contacts/${contactId}/associations/companies/${assoc.id}/${assoc.type || associationType}`,
                                        'DELETE',
                        } catch (removeError) {
                            // Continue even if removal fails
                            console.warn(`Failed to remove existing associations for contact ${contactId}:`, removeError);
                    // Create the association
                    await this.associateObjects(
                        'contacts',
                        contactId,
                        'companies',
                        associationType,
                    // Update contact properties if requested
                    if (updateContactCompany) {
                        const updateProps: any = {
                            company: companyDetails.name
                        if (jobTitle) {
                            updateProps.jobtitle = jobTitle;
                            'PATCH',
                            { properties: updateProps },
                    // Mark as primary if requested (requires specific association type)
                    let primaryStatus = null;
                    if (isPrimary) {
                            // Set as primary company for the contact
                                `objects/contacts/${contactId}/associations/companies/${companyId}/labels`,
                                { labels: ['primary'] },
                            primaryStatus = 'Set as primary';
                        } catch (primaryError) {
                            primaryStatus = 'Failed to set as primary';
                        contactId: contactId,
                        contactName: `${contactDetails.firstname || ''} ${contactDetails.lastname || ''}`.trim() || contactDetails.email,
                        message: 'Successfully associated',
                        primaryStatus: primaryStatus
                        error: errorMessage
            // Create summary
                companyId: companyId,
                companyName: companyDetails.name,
                companyDomain: companyDetails.domain,
                totalContacts: contactIdArray.length,
                successCount: successCount,
                failureCount: failureCount,
                associationType: associationType,
                isPrimary: isPrimary,
                removedExisting: removeExisting,
                updatedContactCompany: updateContactCompany,
                results: results
            // Update output parameters
            const outputParams = [...Params];
            const resultsParam = outputParams.find(p => p.Name === 'Results');
            if (resultsParam) resultsParam.Value = results;
            const summaryParam = outputParams.find(p => p.Name === 'Summary');
            if (summaryParam) summaryParam.Value = summary;
            const companyDetailsParam = outputParams.find(p => p.Name === 'CompanyDetails');
            if (companyDetailsParam) companyDetailsParam.Value = companyDetails;
            // Determine overall success
            const overallSuccess = successCount > 0;
            const resultCode = failureCount === 0 ? 'SUCCESS' : (successCount === 0 ? 'ERROR' : 'PARTIAL_SUCCESS');
            const message = failureCount === 0 
                ? `Successfully associated ${successCount} contact(s) with company ${companyDetails.name}`
                : `Associated ${successCount} of ${contactIdArray.length} contact(s) with company ${companyDetails.name}. ${failureCount} failed.`;
                Success: overallSuccess,
                ResultCode: resultCode,
                Message: message,
                Params: outputParams
            // Check for company not found
            if (errorMessage.includes('404') && errorMessage.includes('companies')) {
                    Message: `Company with ID ${this.getParamValue(Params, 'CompanyId')} not found`,
                Message: `Error associating contacts with company: ${errorMessage}`,
     * Define the parameters this action expects
        const baseParams = this.getCommonCRMParams();
                Name: 'ContactIds',
                Name: 'CompanyId',
                Name: 'AssociationType',
                Value: 'contact_to_company'
                Name: 'IsPrimary',
                Name: 'JobTitle',
                Name: 'RemoveExisting',
                Name: 'UpdateContactCompany',
                Name: 'Results',
                Name: 'CompanyDetails',
     * Metadata about this action
        return 'Associates one or more contacts with a company in HubSpot';
