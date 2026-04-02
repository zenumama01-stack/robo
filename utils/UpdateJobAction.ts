 * Update Scheduled Job action.
 * Updates an existing scheduled job with new values for specified fields.
 * Only provided fields will be updated; omitted fields remain unchanged.
 *   ActionName: 'Update Scheduled Job',
 *     { Name: 'IsActive', Value: 'false' },
 *     { Name: 'Status', Value: 'Disabled' }
@RegisterClass(BaseAction, 'Update Scheduled Job')
export class UpdateScheduledJobAction extends BaseJobAction {
     * Updates an existing scheduled job
     *   - JobID (required): ID of the job to update
     *   - Name (optional): New job name
     *   - CronExpression (optional): New cron expression
     *   - IsActive (optional): New active flag
     *   - Status (optional): New status (Active, Disabled)
     *   - IntervalMinutes (optional): New interval in minutes
     *   - IntervalDays (optional): New interval in days
     *   - Description (optional): New description
     *   - Success: true if job was updated successfully
     *   - ResultCode: SUCCESS, VALIDATION_ERROR, NOT_FOUND, NO_CHANGES, or FAILED
     *   - Message: Details about what was updated
            // Load the existing job
            // Track what changed
            // Update Name if provided
            if (name && name !== job.Name) {
                updatedFields.push('Name');
            // Update CronExpression if provided
            if (cronExpression && cronExpression !== job.CronExpression) {
                updatedFields.push('CronExpression');
            // Update Status if provided
            if (status && status !== job.Status) {
                if (!validStatuses.includes(status)) {
                        Message: `Invalid status: ${status}. Valid values are: ${validStatuses.join(', ')}`
                updatedFields.push('Status');
            // Update Description if provided
            if (description !== undefined && description !== job.Description) {
                job.Description = description || null;
                updatedFields.push('Description');
            // Check if any changes were made
            if (updatedFields.length === 0) {
            // Save changes
                    Message: `Failed to update scheduled job: ${job.LatestResult?.Message || 'Unknown error'}`
                Message: `Updated scheduled job '${job.Name}'. Changed fields: ${updatedFields.join(', ')}`
                Message: `Error updating scheduled job: ${error instanceof Error ? error.message : String(error)}`
