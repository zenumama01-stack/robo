 * Action to get all deals associated with a contact in HubSpot
@RegisterClass(BaseAction, 'GetDealsByContactAction')
export class GetDealsByContactAction extends HubSpotBaseAction {
     * Get all deals for a specific contact
                    Message: 'Contact ID is required',
            // First, get the contact to verify it exists and get basic info
                    `objects/contacts/${contactId}?properties=firstname,lastname,email,company`,
                `objects/contacts/${contactId}/associations/deals`,
                const contactInfo = this.mapHubSpotProperties(contact);
                    contactName: `${contactInfo.firstname || ''} ${contactInfo.lastname || ''}`.trim(),
                    contactEmail: contactInfo.email,
                    Message: `No deals found for contact ${contactInfo.email || contactId}`,
                            'hs_is_closed_won', 'closed_won_reason', 'closed_lost_reason'
                deals = batchResponse.results.map((deal: any) => this.mapHubSpotProperties(deal));
                openValue: 0,
                pipelineBreakdown: {} as Record<string, number>
                            stats.openValue += amount;
                contactCompany: contactInfo.company,
                winRate: stats.closedWonDeals > 0 ? (stats.closedWonDeals / (stats.closedWonDeals + stats.closedLostDeals)) * 100 : 0
                Message: `Found ${deals.length} deals for contact ${contactInfo.email || contactId}`,
                Message: `Error retrieving deals for contact: ${errorMessage}`,
        return 'Retrieves all deals associated with a specific contact including detailed statistics';
