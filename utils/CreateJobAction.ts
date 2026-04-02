import { BaseJobAction } from './BaseJobAction';
 * Create Scheduled Job action.
 * Creates a new scheduled job with the specified configuration.
 * await RunAction({
 *   ActionName: 'Create Scheduled Job',
 *     { Name: 'Name', Value: 'Daily Data Sync' },
 *     { Name: 'JobTypeID', Value: 'F3C4A5B6-...' },
 *     { Name: 'CronExpression', Value: '0 0 * * *' },
 *     { Name: 'IsActive', Value: 'true' }
@RegisterClass(BaseAction, 'Create Scheduled Job')
export class CreateScheduledJobAction extends BaseJobAction {
     * Creates a new scheduled job record
     *   - Name (required): Job name
     *   - JobTypeID (required): ID of the job type
     *   - CronExpression (required): Cron schedule expression
     *   - IsActive (optional): Whether job is active (default: true)
     *   - Status (optional): Job status (default: Active)
     *   - IntervalMinutes (optional): Polling interval in minutes
     *   - IntervalDays (optional): Polling interval in days
     *   - Description (optional): Job description
     *   - Success: true if job was created successfully
     *   - ResultCode: SUCCESS, VALIDATION_ERROR, or FAILED
     *   - Params: Output parameter 'JobID' contains the ID of the created job
            const name = this.getParamValue(params, 'Name');
            const jobTypeId = this.getParamValue(params, 'JobTypeID');
            const cronExpression = this.getParamValue(params, 'CronExpression');
                    Message: 'Name parameter is required'
            if (!jobTypeId) {
                    Message: 'JobTypeID parameter is required'
            if (!cronExpression) {
                    Message: 'CronExpression parameter is required'
            // Validate cron expression
            const cronValidation = this.validateCronExpression(cronExpression);
            if (!cronValidation.valid) {
                    Message: cronValidation.error || 'Invalid cron expression'
            const isActive = this.getBooleanParam(params, 'IsActive', true);
            const status = this.getParamValue(params, 'Status') || 'Active';
            const intervalMinutes = this.getNumericParam(params, 'IntervalMinutes', 0);
            const intervalDays = this.getNumericParam(params, 'IntervalDays', 0);
            const description = this.getParamValue(params, 'Description');
            if (!this.isValidStatus(status)) {
                    Message: `Invalid status: ${status}. Valid values are: Active, Disabled`
            // Create entity object
            const job = await md.GetEntityObject<MJScheduledJobEntity>('MJ: Scheduled Jobs', params.ContextUser);
            job.Name = name;
            job.JobTypeID = jobTypeId;
            job.CronExpression = cronExpression;
            job.Status = status as 'Active' | 'Disabled' | 'Expired' | 'Paused' | 'Pending';
            // Set optional fields - timezone defaults to UTC
            job.Timezone = 'UTC';
                job.Description = description;
            const saveResult = await job.Save();
                    Message: `Failed to create scheduled job: ${job.LatestResult?.Message || 'Unknown error'}`
            // Return the created job ID
            this.addOutputParam(params, 'JobID', job.ID);
                Message: `Created scheduled job '${name}' with ID: ${job.ID}`,
                Message: `Error creating scheduled job: ${error instanceof Error ? error.message : String(error)}`
