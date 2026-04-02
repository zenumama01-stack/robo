 * Action to merge duplicate contacts in HubSpot
@RegisterClass(BaseAction, 'MergeContactsAction')
export class MergeContactsAction extends HubSpotBaseAction {
     * Merge duplicate contacts
            const primaryContactId = this.getParamValue(Params, 'PrimaryContactId');
            const secondaryContactIds = this.getParamValue(Params, 'SecondaryContactIds');
            const primaryEmail = this.getParamValue(Params, 'PrimaryEmail');
            const secondaryEmails = this.getParamValue(Params, 'SecondaryEmails');
            const mergeStrategy = this.getParamValue(Params, 'MergeStrategy') || 'NEWEST';
            if (!primaryContactId && !primaryEmail) {
                    Message: 'Either PrimaryContactId or PrimaryEmail is required',
            if (!secondaryContactIds && !secondaryEmails) {
                    Message: 'Either SecondaryContactIds or SecondaryEmails is required',
            // Resolve contact IDs if emails provided
            let primaryId = primaryContactId;
            let secondaryIds = secondaryContactIds || [];
            // Get primary contact ID from email if needed
            if (!primaryId && primaryEmail) {
                const primarySearch = await this.searchHubSpotObjects<any>(
                        value: primaryEmail
                    ['email', 'firstname', 'lastname'],
                if (primarySearch.length === 0) {
                        ResultCode: 'PRIMARY_NOT_FOUND',
                        Message: `Primary contact with email ${primaryEmail} not found`,
                primaryId = primarySearch[0].id;
            // Get secondary contact IDs from emails if needed
            if (secondaryEmails && (!secondaryIds || secondaryIds.length === 0)) {
                const emailList = Array.isArray(secondaryEmails) ? secondaryEmails : [secondaryEmails];
                secondaryIds = [];
                for (const email of emailList) {
                    const secondarySearch = await this.searchHubSpotObjects<any>(
                    if (secondarySearch.length > 0) {
                        secondaryIds.push(secondarySearch[0].id);
                if (secondaryIds.length === 0) {
                        ResultCode: 'SECONDARY_NOT_FOUND',
                        Message: 'No secondary contacts found with provided emails',
            // Ensure secondaryIds is an array
            if (!Array.isArray(secondaryIds)) {
                secondaryIds = [secondaryIds];
            // Get details of all contacts before merge
            const primaryBefore = await this.makeHubSpotRequest<any>(
                `objects/contacts/${primaryId}`,
            const secondaryDetailsBefore = [];
            for (const secondaryId of secondaryIds) {
                        `objects/contacts/${secondaryId}`,
                    secondaryDetailsBefore.push(this.mapHubSpotProperties(contact));
                    // Contact might not exist, skip
            // Perform the merge
            const mergeBody = {
                objectIdToMerge: secondaryIds[0], // HubSpot v3 API merges one at a time
                mergeProperties: mergeStrategy === 'NEWEST'
            // Track merge results
            const mergeResults = [];
            const failedMerges = [];
            // Merge each secondary contact
                        `objects/contacts/merge`,
                            primaryObjectId: primaryId,
                            objectIdToMerge: secondaryId
                    mergeResults.push({
                        secondaryId: secondaryId,
                        message: 'Successfully merged'
                } catch (mergeError) {
                    const errorMessage = mergeError instanceof Error ? mergeError.message : 'Unknown error';
                    failedMerges.push({
            // Get the merged contact details
            const primaryAfter = await this.makeHubSpotRequest<any>(
            const mergedContactDetails = this.mapHubSpotProperties(primaryAfter);
                primaryContactId: primaryId,
                mergedContactEmail: mergedContactDetails.email,
                mergedContactName: `${mergedContactDetails.firstname || ''} ${mergedContactDetails.lastname || ''}`.trim(),
                totalContactsMerged: mergeResults.length + 1, // Including primary
                successfulMerges: mergeResults.length,
                failedMerges: failedMerges.length,
                mergeStrategy: mergeStrategy,
                portalUrl: `https://app.hubspot.com/contacts/${primaryId}`
            const primaryBeforeParam = outputParams.find(p => p.Name === 'PrimaryContactBefore');
            if (primaryBeforeParam) primaryBeforeParam.Value = this.mapHubSpotProperties(primaryBefore);
            const secondaryContactsParam = outputParams.find(p => p.Name === 'SecondaryContactsBefore');
            if (secondaryContactsParam) secondaryContactsParam.Value = secondaryDetailsBefore;
            const mergedContactParam = outputParams.find(p => p.Name === 'MergedContact');
            if (mergedContactParam) mergedContactParam.Value = mergedContactDetails;
            const mergeResultsParam = outputParams.find(p => p.Name === 'MergeResults');
            if (mergeResultsParam) mergeResultsParam.Value = mergeResults;
            const failedMergesParam = outputParams.find(p => p.Name === 'FailedMerges');
            if (failedMergesParam) failedMergesParam.Value = failedMerges;
            if (failedMerges.length > 0 && mergeResults.length === 0) {
                    ResultCode: 'MERGE_FAILED',
                    Message: 'All merge operations failed',
            const resultMessage = failedMerges.length > 0
                ? `Merged ${mergeResults.length} of ${secondaryIds.length} contacts into primary contact`
                : `Successfully merged ${mergeResults.length} contacts into primary contact`;
                ResultCode: failedMerges.length > 0 ? 'PARTIAL_SUCCESS' : 'SUCCESS',
                Message: resultMessage,
                    Message: 'One or more contacts not found',
            if (errorMessage.includes('409') || errorMessage.includes('conflict')) {
                    ResultCode: 'MERGE_CONFLICT',
                    Message: 'Merge conflict - contacts may have already been merged',
                Message: `Error merging contacts: ${errorMessage}`,
                Name: 'PrimaryContactId',
                Name: 'SecondaryContactIds',
                Name: 'PrimaryEmail',
                Name: 'SecondaryEmails',
                Name: 'MergeStrategy',
                Value: 'NEWEST'
                Name: 'PrimaryContactBefore',
                Name: 'SecondaryContactsBefore',
                Name: 'MergedContact',
                Name: 'MergeResults',
                Name: 'FailedMerges',
        return 'Merges duplicate contacts in HubSpot, combining their data and associations';
