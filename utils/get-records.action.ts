 * Generic action for retrieving multiple records from any entity with filtering and sorting capabilities.
 * This action provides a flexible way to fetch filtered and sorted datasets for any entity type
 * by accepting the entity name, optional filter clause, and ordering parameters.
 * // Get all active events sorted by start date
 *   ActionName: 'Get Records',
 *       Value: 'Events'
 *       Name: 'Filter',
 *       Value: 'Status = 'Active' AND StartDate >= GETDATE()'
 *       Name: 'OrderBy',
 *       Value: 'StartDate ASC'
 *       Name: 'MaxRows',
 *       Value: 100
 * // Get recent submissions for a specific event
 *       Value: 'Submissions'
 *       Value: 'EventID = '123e4567-e89b-12d3-a456-426614174000' AND CreatedAt >= DATEADD(day, -30, GETDATE())'
 *       Value: 'CreatedAt DESC'
@RegisterClass(BaseAction, "GetRecordsAction")
export class GetRecordsAction extends BaseAction {
            const entityName = this.getStringParam(params, "EntityName");
                    ResultCode: "MISSING_ENTITY_NAME",
                    Message: "EntityName parameter is required"
            const filter = this.getStringParam(params, "Filter");
            const orderBy = this.getStringParam(params, "OrderBy");
            const maxRows = this.getNumericParam(params, "MaxRows", 100);
            const includeCount = this.getBooleanParam(params, "IncludeCount", true);
            if (maxRows && (maxRows < 1 || maxRows > 10000)) {
                    ResultCode: "INVALID_MAX_ROWS",
                    Message: "MaxRows must be between 1 and 10000"
            // Use RunView to execute the query
            const runViewParams: any = {
            // Add filter if provided
            if (filter) {
                runViewParams.Filter = filter;
            // Add ordering if provided
                runViewParams.OrderBy = orderBy;
            const result = await rv.RunView(runViewParams, params.ContextUser);
                    ResultCode: "QUERY_FAILED",
                    Message: `Failed to retrieve records: ${result.ErrorMessage}`
            const records = result.Results || [];
            const totalCount = result.TotalRowCount || records.length;
            // Build success message
            let message = `Successfully retrieved ${records.length} records`;
            if (includeCount && totalCount > records.length) {
                message += ` (showing ${records.length} of ${totalCount} total)`;
                message += ` with filter: ${filter}`;
                message += ` ordered by: ${orderBy}`;
                        Name: 'Records',
                        Value: records
                        Name: 'EntityName',
                        Value: entityName
                        Value: filter
                        Value: orderBy
                ResultCode: "ERROR",
                Message: `Error retrieving records: ${errorMessage}`
    public get Params(): any[] {
                Name: 'MaxRows',
                Name: 'IncludeCount',
    public get Returns(): any[] {
    private getStringParam(params: RunActionParams, paramName: string): string | undefined {
        const param = params.Params.find(p =>
            p.Name.toLowerCase() === paramName.toLowerCase() &&
            p.Type === 'Input'
        return param?.Value ? String(param.Value) : undefined;
        if (param?.Value != null) {
            const num = Number(param.Value);
            const val = String(param.Value).toLowerCase();
            if (val === 'true' || val === '1' || val === 'yes') return true;
            if (val === 'false' || val === '0' || val === 'no') return false;
