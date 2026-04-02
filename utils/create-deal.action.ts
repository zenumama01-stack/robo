 * Action to create a new deal in HubSpot
@RegisterClass(BaseAction, 'CreateDealAction')
export class CreateDealAction extends HubSpotBaseAction {
     * Create a new deal
            const dealName = this.getParamValue(Params, 'DealName');
            const dealStage = this.getParamValue(Params, 'DealStage');
            const pipelineId = this.getParamValue(Params, 'PipelineId');
            const amount = this.getParamValue(Params, 'Amount');
            const closeDate = this.getParamValue(Params, 'CloseDate');
            const dealType = this.getParamValue(Params, 'DealType');
            const priority = this.getParamValue(Params, 'Priority');
            const associateWithCompanyIds = this.getParamValue(Params, 'AssociateWithCompanyIds');
            if (!dealName) {
                    Message: 'Deal name is required',
            // Prepare deal properties
                dealname: dealName
            if (dealStage) properties.dealstage = dealStage;
            if (pipelineId) properties.pipeline = pipelineId;
            if (amount) properties.amount = parseFloat(amount.toString());
            if (closeDate) {
                // Convert to milliseconds timestamp if needed
                const closeDateMs = new Date(closeDate).getTime();
                properties.closedate = closeDateMs;
            if (dealType) properties.dealtype = dealType;
            if (priority) properties.hs_priority = priority;
            // Create deal
            const newDeal = await this.makeHubSpotRequest<any>(
                'objects/deals',
            // Format deal details
            const dealDetails = this.mapHubSpotProperties(newDeal);
            // Associate with contacts and companies
            const associations: any[] = [];
            // Associate with contacts
            if (associateWithContactIds) {
                const contactIds = Array.isArray(associateWithContactIds) 
                    ? associateWithContactIds 
                    : associateWithContactIds.split(',').map((id: string) => id.trim());
                for (const contactId of contactIds) {
                            'deals',
                            newDeal.id,
                        associations.push({
                            type: 'contact',
                            id: contactId,
                            error: error instanceof Error ? error.message : 'Association failed'
            // Associate with companies
            if (associateWithCompanyIds) {
                const companyIds = Array.isArray(associateWithCompanyIds) 
                    ? associateWithCompanyIds 
                    : associateWithCompanyIds.split(',').map((id: string) => id.trim());
                for (const companyId of companyIds) {
                            type: 'company',
                            id: companyId,
                dealId: dealDetails.id,
                dealName: dealDetails.dealname,
                dealStage: dealDetails.dealstage,
                pipeline: dealDetails.pipeline,
                amount: dealDetails.amount,
                closeDate: dealDetails.closedate,
                createdAt: dealDetails.createdAt,
                portalUrl: `https://app.hubspot.com/contacts/${this.getParamValue(Params, 'CompanyID')}/deal/${dealDetails.id}`,
                associations: associations
            const dealDetailsParam = outputParams.find(p => p.Name === 'DealDetails');
            if (dealDetailsParam) dealDetailsParam.Value = dealDetails;
                Message: `Successfully created deal "${dealDetails.dealname}"`,
                Message: `Error creating deal: ${errorMessage}`,
                Name: 'DealName',
                Name: 'DealStage',
                Name: 'PipelineId',
                Name: 'Amount',
                Name: 'CloseDate',
                Name: 'DealType',
                Name: 'Priority',
                Name: 'AssociateWithCompanyIds',
                Name: 'DealDetails',
        return 'Creates a new deal in HubSpot with optional contact and company associations';
