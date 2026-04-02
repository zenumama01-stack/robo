 * Action that iterates over collections executing actions for each item
 * // Process array of items
 *   ActionName: 'Loop',
 *     Name: 'Items',
 *       { name: 'John', email: 'john@example.com' },
 *       { name: 'Jane', email: 'jane@example.com' }
 *     Name: 'ItemVariableName',
 *     Value: 'user'
 *     Name: 'Action',
 *         To: '{{user.email}}',
 *         Subject: 'Welcome {{user.name}}'
 * // Parallel processing with limit
 *     Value: fileUrls
 *     Name: 'Parallel',
 *       ActionName: 'Web Page Content',
 *       Params: { URL: '{{item}}' }
@RegisterClass(BaseAction, "Loop")
export class LoopAction extends BaseAction {
     * Iterates over collections executing actions
     *   - Items: Array to iterate over (required)
     *   - ItemVariableName: Variable name for current item (default: 'item')
     *   - IndexVariableName: Variable name for current index (default: 'index')
     *   - Action: Action configuration to execute per item (required)
     *   - Parallel: Boolean - run iterations in parallel (default: false)
     *   - MaxConcurrent: Max parallel executions (default: 10)
     *   - ContinueOnError: Boolean - continue if an iteration fails (default: false)
     *   - IncludeContext: Additional context to pass to each action (optional)
     * @returns Array of results from each iteration
            const items = JSONParamHelper.getJSONParam(params, 'items');
            const itemVariableName = this.getParamValue(params, 'itemvariablename') || 'item';
            const indexVariableName = this.getParamValue(params, 'indexvariablename') || 'index';
            const actionConfig = JSONParamHelper.getJSONParam(params, 'action');
            const parallel = this.getBooleanParam(params, 'parallel', false);
            const maxConcurrent = this.getNumericParam(params, 'maxconcurrent', 10);
            const continueOnError = this.getBooleanParam(params, 'continueonerror', false);
            const includeContext = JSONParamHelper.getJSONParam(params, 'includecontext') || {};
            if (!items) {
                    Message: "Items parameter is required",
                    ResultCode: "MISSING_ITEMS"
            if (!Array.isArray(items)) {
                    Message: "Items must be an array",
                    ResultCode: "INVALID_ITEMS"
            if (!actionConfig) {
                    Message: "Action parameter is required",
                    ResultCode: "MISSING_ACTION"
                    Message: "Action configuration must include ActionName",
                    ResultCode: "INVALID_ACTION"
            // Track results
            let errorCount = 0;
            // Execute iterations
            if (parallel) {
                // Parallel execution with concurrency limit
                const chunks = this.chunkArray(items, maxConcurrent);
                    const chunkPromises = chunk.map((item, chunkIndex) => {
                        const globalIndex = chunks.indexOf(chunk) * maxConcurrent + chunkIndex;
                        return this.executeIteration(
                            globalIndex, 
                            itemVariableName, 
                            indexVariableName, 
                            actionConfig, 
                            includeContext,
                            params.ContextUser
                    const chunkResults = await Promise.allSettled(chunkPromises);
                    for (let i = 0; i < chunkResults.length; i++) {
                        const result = chunkResults[i];
                        const globalIndex = chunks.indexOf(chunk) * maxConcurrent + i;
                            results[globalIndex] = result.value;
                            if (result.value.Success) {
                                errorCount++;
                                errors.push({ index: globalIndex, error: result.value.Message });
                                if (!continueOnError) break;
                            const errorResult = {
                                Message: result.reason?.message || String(result.reason),
                                ResultCode: "ITERATION_ERROR"
                            results[globalIndex] = errorResult;
                            errors.push({ index: globalIndex, error: result.reason });
                    if (!continueOnError && errorCount > 0) break;
                // Sequential execution
                for (let i = 0; i < items.length; i++) {
                        const result = await this.executeIteration(
                            items[i], 
                            errors.push({ index: i, error: result.Message });
                        results.push(errorResult);
                        errors.push({ index: i, error: error });
                Value: results
                Name: 'SuccessCount',
                Value: successCount
                Name: 'ErrorCount',
                Value: errorCount
                    Value: errors
            const allSuccessful = errorCount === 0;
            const someSuccessful = successCount > 0;
                Success: continueOnError ? someSuccessful : allSuccessful,
                ResultCode: allSuccessful ? "ALL_SUCCESS" : (someSuccessful ? "PARTIAL_SUCCESS" : "ALL_FAILED"),
                    message: `Loop completed: ${successCount} successful, ${errorCount} failed`,
                    totalItems: items.length,
                    errorCount: errorCount,
                    parallel: parallel,
                    continueOnError: continueOnError,
                Message: `Loop action failed: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "LOOP_FAILED"
     * Execute a single iteration
    private async executeIteration(
        itemVariableName: string, 
        indexVariableName: string,
        actionConfig: any,
        includeContext: any,
        // Build context for this iteration
        const iterationContext = {
            ...includeContext,
            [itemVariableName]: item,
            [indexVariableName]: index
        // Prepare action configuration with context
        const preparedConfig = this.prepareActionConfig(actionConfig, iterationContext);
        const result = await engine.RunAction(preparedConfig);
        // Convert ActionResult to ActionResultSimple
            ResultCode: result.Result?.ResultCode || (result.Success ? "SUCCESS" : "FAILED"),
            Message: result.Message,
            Params: result.Params
     * Prepare action configuration with template replacement
    private prepareActionConfig(actionConfig: any, context: any): any {
        // Deep clone the config
        const preparedConfig = JSON.parse(JSON.stringify(actionConfig));
        // Replace templates in the configuration
        this.replaceTemplates(preparedConfig, context);
     * Replace template variables in object recursively
    private replaceTemplates(obj: any, context: any): void {
        if (typeof obj === 'string') {
            // Can't modify string in place, would need parent reference
                const value = obj[key];
                    // Replace template variables
                    obj[key] = this.replaceTemplateString(value, context);
                    // Recurse into objects and arrays
                    this.replaceTemplates(value, context);
     * Replace template variables in a string
    private replaceTemplateString(template: string, context: any): string {
        // Simple template replacement for {{variable}} and {{object.property}}
        return template.replace(/\{\{([^}]+)\}\}/g, (match, path) => {
            const trimmedPath = path.trim();
            const value = this.getValueByPath(context, trimmedPath);
            return value !== undefined ? String(value) : match;
     * Get value from object by dot-notation path
    private getValueByPath(obj: any, path: string): any {
