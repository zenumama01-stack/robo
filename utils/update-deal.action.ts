 * Action to update an existing deal in HubSpot
@RegisterClass(BaseAction, 'UpdateDealAction')
export class UpdateDealAction extends HubSpotBaseAction {
     * Update a deal's information
            const closedWonReason = this.getParamValue(Params, 'ClosedWonReason');
            const closedLostReason = this.getParamValue(Params, 'ClosedLostReason');
            // Get current deal details first
            let currentDeal;
                currentDeal = await this.makeHubSpotRequest<any>(
                    `objects/deals/${dealId}`,
                    Message: `Deal with ID ${dealId} not found`,
            // Prepare update properties
            // Add fields that have been provided
            if (dealName !== null && dealName !== undefined) properties.dealname = dealName;
            if (dealStage !== null && dealStage !== undefined) properties.dealstage = dealStage;
            if (pipelineId !== null && pipelineId !== undefined) properties.pipeline = pipelineId;
            if (amount !== null && amount !== undefined) properties.amount = parseFloat(amount.toString());
            if (closeDate !== null && closeDate !== undefined) {
            if (dealType !== null && dealType !== undefined) properties.dealtype = dealType;
            if (priority !== null && priority !== undefined) properties.hs_priority = priority;
            if (description !== null && description !== undefined) properties.description = description;
            if (closedWonReason !== null && closedWonReason !== undefined) properties.closed_won_reason = closedWonReason;
            if (closedLostReason !== null && closedLostReason !== undefined) properties.closed_lost_reason = closedLostReason;
                    ResultCode: 'NO_UPDATES',
            // Update the deal
            const updatedDeal = await this.makeHubSpotRequest<any>(
            const dealDetails = this.mapHubSpotProperties(updatedDeal);
            const previousDetails = this.mapHubSpotProperties(currentDeal);
                const fieldName = this.getHubSpotFieldDisplayName(key);
                const newValue = properties[key];
                        oldValue: oldValue,
                        newValue: newValue
                totalChanges: changes.length
                Message: `Successfully updated deal "${dealDetails.dealname}" with ${changes.length} changes`,
                Message: `Error updating deal: ${errorMessage}`,
     * Get display name for HubSpot field
    private getHubSpotFieldDisplayName(fieldKey: string): string {
        const fieldMap: Record<string, string> = {
            'dealname': 'Deal Name',
            'dealstage': 'Deal Stage',
            'pipeline': 'Pipeline',
            'amount': 'Amount',
            'closedate': 'Close Date',
            'dealtype': 'Deal Type',
            'hs_priority': 'Priority',
            'description': 'Description',
            'closed_won_reason': 'Closed Won Reason',
            'closed_lost_reason': 'Closed Lost Reason'
        return fieldMap[fieldKey] || fieldKey;
                Name: 'ClosedWonReason',
                Name: 'ClosedLostReason',
        return 'Updates an existing deal in HubSpot with change tracking';
