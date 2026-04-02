 * Action to create a new contact in HubSpot
@RegisterClass(BaseAction, 'CreateContactAction')
export class CreateContactAction extends HubSpotBaseAction {
     * Create a new contact
            const email = this.getParamValue(Params, 'Email');
            const firstName = this.getParamValue(Params, 'FirstName');
            const lastName = this.getParamValue(Params, 'LastName');
            const company = this.getParamValue(Params, 'Company');
            const leadStatus = this.getParamValue(Params, 'LeadStatus');
            const associateWithCompanyId = this.getParamValue(Params, 'AssociateWithCompanyId');
                    Message: 'Email is required',
            // Validate email format
            if (!this.isValidEmail(email)) {
                    Message: 'Invalid email format',
            // Prepare contact properties
                email: email,
            if (firstName) properties.firstname = firstName;
            if (lastName) properties.lastname = lastName;
            if (company) properties.company = company;
            if (jobTitle) properties.jobtitle = jobTitle;
            if (leadStatus) properties.lead_status = leadStatus;
            // Create contact
            const newContact = await this.makeHubSpotRequest<any>(
                'objects/contacts',
            // Format contact details
            const contactDetails = this.mapHubSpotProperties(newContact);
            // Associate with company if requested
            let associationResult = null;
            if (associateWithCompanyId) {
                        newContact.id,
                        associateWithCompanyId,
                    associationResult = {
                        companyId: associateWithCompanyId,
                        message: 'Successfully associated contact with company'
                contactId: contactDetails.id,
                email: contactDetails.email,
                fullName: `${contactDetails.firstname || ''} ${contactDetails.lastname || ''}`.trim(),
                lifecycleStage: contactDetails.lifecyclestage,
                company: contactDetails.company,
                createdAt: contactDetails.createdAt,
                portalUrl: `https://app.hubspot.com/contacts/${contactDetails.id}`,
                associationResult: associationResult
            const contactDetailsParam = outputParams.find(p => p.Name === 'ContactDetails');
            if (contactDetailsParam) contactDetailsParam.Value = contactDetails;
                Message: `Successfully created contact ${contactDetails.email}`,
            // Check for duplicate contact error
            if (errorMessage.includes('Contact already exists') || errorMessage.includes('CONFLICT')) {
                    ResultCode: 'DUPLICATE_CONTACT',
                    Message: `Contact with email ${this.getParamValue(Params, 'Email')} already exists`,
                Message: `Error creating contact: ${errorMessage}`,
                Name: 'Email',
                Name: 'FirstName',
                Name: 'LastName',
                Name: 'Company',
                Name: 'LeadStatus',
                Name: 'AssociateWithCompanyId',
                Name: 'ContactDetails',
        return 'Creates a new contact in HubSpot with optional company association';
