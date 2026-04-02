 * Execute Scheduled Job Now action.
 * Triggers immediate execution of a scheduled job, bypassing its normal schedule.
 * Creates a job run record with "Pending" status for the scheduling engine to pick up.
 *   ActionName: 'Execute Scheduled Job Now',
@RegisterClass(BaseAction, 'Execute Scheduled Job Now')
export class ExecuteScheduledJobNowAction extends BaseJobAction {
     * Triggers immediate execution of a scheduled job
     *   - JobID (required): ID of the job to execute
     *   - Success: true if job execution was queued successfully
     *   - ResultCode: SUCCESS, VALIDATION_ERROR, NOT_FOUND, or FAILED
     *   - Params: Output parameter 'RunID' contains the ID of the created job run
            // Load the job to verify it exists
            // Check if job status allows execution
            if (job.Status !== 'Active') {
                    Message: `Cannot execute job '${job.Name}' with status '${job.Status}'. Job must be Active.`
            // Create a job run record
            const jobRun = await md.GetEntityObject<MJScheduledJobRunEntity>('MJ: Scheduled Job Runs', params.ContextUser);
            jobRun.NewRecord();
            jobRun.ScheduledJobID = job.ID;
            jobRun.Status = 'Running';
            jobRun.StartedAt = new Date();
            jobRun.QueuedAt = new Date();
            // Save the job run
            const saveResult = await jobRun.Save();
                    Message: `Failed to create job run: ${jobRun.LatestResult?.Message || 'Unknown error'}`
            // Add output parameter with run ID
            this.addOutputParam(params, 'RunID', jobRun.ID);
                Message: `Scheduled job '${job.Name}' queued for immediate execution (Run ID: ${jobRun.ID})`,
                Message: `Error executing scheduled job: ${error instanceof Error ? error.message : String(error)}`
