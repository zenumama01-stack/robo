    LogError, Metadata, RunView, RunViewParams, 
    RunViewResult, UserInfo 
 * View Operations Implementation for REST endpoints
 * These functions handle running views through the REST API
export class ViewOperationsHandler {
     * Run a view and return results
    static async runView(params: RunViewParams, user: UserInfo): Promise<{ success: boolean, result?: RunViewResult, error?: string }> {
            // Validate entity exists
            const entity = md.Entities.find(e => e.Name === params.EntityName);
                    error: `Entity '${params.EntityName}' not found` 
            // Check read permission
                    error: `User ${user.Name} does not have permission to read ${params.EntityName} records` 
            // Sanitize and validate parameters
            this.sanitizeRunViewParams(params);
            // Execute the view
            const runView = new RunView();
            const result = await runView.RunView(params, user);
            return { success: false, error: (error as Error)?.message || 'Unknown error' };
    static async runViews(paramsArray: RunViewParams[], user: UserInfo): Promise<{ success: boolean, results?: RunViewResult[], error?: string }> {
            // Validate and sanitize each set of parameters
            for (const params of paramsArray) {
                // Sanitize parameters
            // Execute the views
            const results = await runView.RunViews(paramsArray, user);
            return { success: true, results };
     * List entities with optional filtering
    static async listEntities(params: RunViewParams, user: UserInfo): Promise<RunViewResult> {
            // Check entity exists and user has permission
                throw new Error(`Entity '${params.EntityName}' not found`);
                throw new Error(`User ${user.Name} does not have permission to read ${params.EntityName} records`);
            return await runView.RunView(params, user);
     * Get available views for an entity
    static async getEntityViews(entityName: string, user: UserInfo): Promise<{ success: boolean, views?: any[], error?: string }> {
                    error: `Entity '${entityName}' not found` 
            // Run a view to get the available views
                ExtraFilter: `Entity = '${entityName}'`
                    error: result.ErrorMessage || 'Failed to retrieve views' 
            // Format the view data
            const views = result.Results.map(view => ({
                ID: view.ID,
                Name: view.Name,
                Description: view.Description,
                IsShared: view.IsShared,
                CreatedAt: view.CreatedAt
            return { success: true, views };
     * Sanitize and validate RunViewParams
    private static sanitizeRunViewParams(params: RunViewParams): void {
        // Ensure EntityName is provided
            throw new Error('EntityName is required');
        // Convert string arrays if they came in as comma-separated strings
        if (params.Fields && typeof params.Fields === 'string') {
            params.Fields = (params.Fields as string).split(',');
        // Sanitize numeric values
        if (params.MaxRows !== undefined) {
            params.MaxRows = typeof params.MaxRows === 'string' 
                ? parseInt(params.MaxRows as string) 
                : params.MaxRows;
        if (params.StartRow !== undefined) {
            params.StartRow = typeof params.StartRow === 'string' 
                ? parseInt(params.StartRow as string) 
                : params.StartRow;
        // Default ResultType if not provided
        if (!params.ResultType) {
            params.ResultType = 'simple';
