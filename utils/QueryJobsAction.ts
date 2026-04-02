 * Query Scheduled Jobs action.
 * Searches for scheduled jobs based on filter criteria including Status, JobTypeID,
 * IsActive flag, and creation date ranges.
 *   ActionName: 'Query Scheduled Jobs',
 *     { Name: 'Status', Value: 'Active' },
 *     { Name: 'IsActive', Value: 'true' },
 *     { Name: 'MaxResults', Value: '50' }
@RegisterClass(BaseAction, 'Query Scheduled Jobs')
export class QueryScheduledJobsAction extends BaseJobAction {
     * Executes the scheduled job query with the provided filter parameters
     *   - Status (optional): Job status filter (Active, Disabled)
     *   - JobTypeID (optional): Filter by job type
     *   - IsActive (optional): Filter by active flag
     *   - CreatedAfter (optional): Filter jobs created after this date
     *   - CreatedBefore (optional): Filter jobs created before this date
     *   - MaxResults (optional): Limit number of results (default: 100)
     *   - Success: true if query executed successfully
     *   - Params: Output parameter 'Jobs' contains array of MJScheduledJobEntity records
            // Extract filter parameters
            const status = this.getParamValue(params, 'Status');
            const isActiveStr = this.getParamValue(params, 'IsActive');
            const createdAfter = this.getDateParam(params, 'CreatedAfter');
            const createdBefore = this.getDateParam(params, 'CreatedBefore');
            const maxResults = this.getNumericParam(params, 'MaxResults', 100);
            // Validate status if provided
            if (status && !this.isValidStatus(status)) {
            // Parse IsActive
            let isActive: boolean | undefined;
            if (isActiveStr !== undefined && isActiveStr !== null) {
                isActive = this.getBooleanParam(params, 'IsActive', true);
            // Build filter expression
            const filterExpression = this.buildJobFilter({
                jobTypeId,
                isActive,
                createdBefore
            // Execute query
                MaxRows: maxResults,
            // Check for query errors
                    Message: `Failed to query scheduled jobs: ${result.ErrorMessage}`
            const jobs = result.Results || [];
            // Add output parameter with the job array
            this.addOutputParam(params, 'Jobs', jobs);
                Message: `Found ${jobs.length} scheduled job(s)`,
                Message: `Error querying scheduled jobs: ${error instanceof Error ? error.message : String(error)}`
