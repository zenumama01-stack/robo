type ListItemStatus = 'Active' | 'Complete' | 'Disabled' | 'Error' | 'Other' | 'Pending' | 'Rejected';
const VALID_STATUSES: ListItemStatus[] = ['Active', 'Complete', 'Disabled', 'Error', 'Other', 'Pending', 'Rejected'];
 * Action to update the status of list items in bulk.
 * // Update specific records
 *   ActionName: 'Update List Item Status',
 *     { Name: 'RecordIDs', Value: ['rec1', 'rec2'] },
 *     { Name: 'NewStatus', Value: 'Complete' }
 * // Update all items with a specific current status
 *     { Name: 'CurrentStatus', Value: 'Pending' },
 *     { Name: 'NewStatus', Value: 'Active' }
@RegisterClass(BaseAction, "Update List Item Status")
export class UpdateListItemStatusAction extends BaseAction {
      const currentStatus = this.getStringParam(params, 'CurrentStatus');
      const newStatus = this.getStringParam(params, 'NewStatus');
      if (!newStatus) {
          Message: 'NewStatus is required'
      // Validate status value
      if (!VALID_STATUSES.includes(newStatus as ListItemStatus)) {
          ResultCode: 'INVALID_STATUS',
          Message: `NewStatus must be one of: ${VALID_STATUSES.join(', ')}`
      if (currentStatus && !VALID_STATUSES.includes(currentStatus as ListItemStatus)) {
          Message: `CurrentStatus must be one of: ${VALID_STATUSES.join(', ')}`
      // Must specify recordIds or currentStatus
      if (!recordIds?.length && !currentStatus) {
          Message: 'Must specify RecordIDs or CurrentStatus to filter items to update'
      if (currentStatus) {
        filter += ` AND Status = '${currentStatus}'`;
      // Find records to update
        this.addOutputParam(params, 'Updated', 0);
          Message: 'No matching records found to update'
      // Update status on all matching records
      let updatedCount = 0;
          detail.Status = newStatus as ListItemStatus;
          const saveResult = await detail.Save();
            updatedCount++;
            errors.push(`Failed to update record '${detail.RecordID}'`);
          errors.push(`Error updating record '${detail.RecordID}': ${errorMessage}`);
      this.addOutputParam(params, 'Updated', updatedCount);
          ? `Successfully updated ${updatedCount} record(s) to status '${newStatus}'`
          : `Updated ${updatedCount}, failed ${failedCount}. Errors: ${errors.join('; ')}`
        Message: `Failed to update list item status: ${errorMessage}`
