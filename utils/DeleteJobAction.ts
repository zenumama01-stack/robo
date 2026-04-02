 * Delete Scheduled Job action.
 * Deletes an existing scheduled job from the system.
 * Will fail if the job has related records that prevent deletion.
 *   ActionName: 'Delete Scheduled Job',
 *     { Name: 'JobID', Value: 'F3C4A5B6-...' }
@RegisterClass(BaseAction, 'Delete Scheduled Job')
export class DeleteScheduledJobAction extends BaseJobAction {
     * Deletes a scheduled job record
     *   - JobID (required): ID of the job to delete
     *   - Success: true if job was deleted successfully
     *   - ResultCode: SUCCESS, VALIDATION_ERROR, NOT_FOUND, REFERENCE_CONSTRAINT, or FAILED
     *   - Message: Description of the result
            const jobId = this.getParamValue(params, 'JobID');
            // Load the job
            const loadResult = await this.loadJob(jobId, params.ContextUser);
            if (loadResult.error) {
                return loadResult.error;
            const job = loadResult.job!;
            const jobName = job.Name;
            // Delete the job
            const deleteResult = await job.Delete();
                const errorMsg = job.LatestResult?.Message || 'Unknown error';
                // Check for cascade/reference errors
                if (errorMsg.toLowerCase().includes('reference') ||
                    errorMsg.toLowerCase().includes('constraint') ||
                    errorMsg.toLowerCase().includes('foreign key')) {
                        ResultCode: 'REFERENCE_CONSTRAINT',
                        Message: `Cannot delete scheduled job '${jobName}': It is referenced by other records (such as job runs)`
                    Message: `Failed to delete scheduled job: ${errorMsg}`
                Message: `Successfully deleted scheduled job '${jobName}' (ID: ${jobId})`
                Message: `Error deleting scheduled job: ${error instanceof Error ? error.message : String(error)}`
