import { MJListDetailEntity } from "@memberjunction/core-entities";
 * Action to remove records from a list.
 * // Remove specific records
 *   ActionName: 'Remove Records from List',
 *     { Name: 'RecordIDs', Value: ['rec1', 'rec2'] }
 * // Remove all records with a specific status
 *     { Name: 'FilterByStatus', Value: 'Complete' }
 * // Remove all records from list
 *     { Name: 'RemoveAll', Value: true }
@RegisterClass(BaseAction, "Remove Records from List")
export class RemoveRecordsFromListAction extends BaseAction {
      const removeAll = this.getBooleanParam(params, 'RemoveAll', false);
      // Must specify recordIds, filterByStatus, or removeAll
      if (!recordIds?.length && !filterByStatus && !removeAll) {
          Message: 'Must specify RecordIDs, FilterByStatus, or RemoveAll'
      if (recordIds && recordIds.length > 0) {
        filter += ` AND RecordID IN (${recordIdFilter})`;
      // Find records to delete
          Message: `Failed to query list details: ${detailsResult.ErrorMessage}`
        this.addOutputParam(params, 'Removed', 0);
        this.addOutputParam(params, 'Failed', 0);
          ResultCode: 'NO_RECORDS',
          Message: 'No matching records found to remove'
      // Delete records
      let removedCount = 0;
          const deleteResult = await detail.Delete();
            removedCount++;
            errors.push(`Failed to remove record '${detail.RecordID}'`);
          errors.push(`Error removing record '${detail.RecordID}': ${errorMessage}`);
      this.addOutputParam(params, 'Removed', removedCount);
      this.addOutputParam(params, 'Failed', failedCount);
      this.addOutputParam(params, 'Errors', errors);
          ? `Successfully removed ${removedCount} record(s) from list`
          : `Removed ${removedCount}, failed ${failedCount}. Errors: ${errors.join('; ')}`
        Message: `Failed to remove records from list: ${errorMessage}`
