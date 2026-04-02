 * Action that executes multiple actions in parallel
 * // Execute multiple actions in parallel
 *   ActionName: 'Parallel Execute',
 *     Name: 'Actions',
 *         ActionName: 'Web Search',
 *         Params: { Query: 'latest news' }
 *         ActionName: 'Get Weather',
 *         Params: { Location: 'New York' }
 *         ActionName: 'Get Stock Price',
 *         Params: { Symbol: 'AAPL' }
 * // Wait for first result only
 *     Value: [api1Action, api2Action, api3Action]
 *     Name: 'WaitForAll',
 * // With timeout and error handling
 *     Value: actions
 *     Name: 'ContinueOnError',
@RegisterClass(BaseAction, "Parallel Execute")
export class ParallelExecuteAction extends BaseAction {
     * Executes multiple actions in parallel
     *   - Actions: Array of action configurations to execute (required)
     *   - WaitForAll: Boolean - wait for all vs first (default: true)
     *   - ContinueOnError: Boolean - continue if actions fail (default: false)
     *   - MaxConcurrent: Max parallel executions (default: unlimited)
     *   - Timeout: Overall timeout in milliseconds (optional)
     *   - IncludeContext: Additional context to pass to all actions (optional)
     * @returns Array of results or first result based on WaitForAll
            const actions = JSONParamHelper.getJSONParam(params, 'actions');
            const waitForAll = this.getBooleanParam(params, 'waitforall', true);
            const maxConcurrent = this.getNumericParam(params, 'maxconcurrent', 0); // 0 = unlimited
            const timeout = this.getNumericParam(params, 'timeout', 0); // 0 = no timeout
            // Validate actions
            if (!actions || !Array.isArray(actions) || actions.length === 0) {
                    Message: "Actions parameter must be a non-empty array",
                    ResultCode: "INVALID_ACTIONS"
            // Validate action configurations
            for (let i = 0; i < actions.length; i++) {
                if (!actions[i] || !actions[i].ActionName) {
                        Message: `Action at index ${i} must have ActionName`,
                        ResultCode: "INVALID_ACTION_CONFIG"
            // Prepare actions with context
            const preparedActions = actions.map(action => 
                this.prepareActionConfig(action, includeContext)
            // Execute based on mode
            if (waitForAll) {
                return await this.executeAllActions(
                    preparedActions, 
                    continueOnError, 
                return await this.executeFirstAction(
                Message: `Parallel execute failed: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "PARALLEL_FAILED"
     * Execute all actions and wait for completion
    private async executeAllActions(
        actions: any[], 
        continueOnError: boolean, 
        maxConcurrent: number,
        timeout: number,
        contextUser: any,
        // Create execution function
        const executeAction = async (action: any, index: number) => {
                // Check timeout
                if (timeout > 0 && Date.now() - startTime > timeout) {
                    throw new Error('Timeout exceeded');
                const result = await engine.RunAction(action);
                results[index] = {
                    actionName: action.ActionName,
                    duration: Date.now() - startTime
                results[index] = errorResult;
        // Execute with or without concurrency limit
        if (maxConcurrent > 0 && maxConcurrent < actions.length) {
            // Execute with concurrency limit
            for (let i = 0; i < actions.length; i += maxConcurrent) {
                const batch = actions.slice(i, i + maxConcurrent);
                const batchPromises = batch.map((action, batchIndex) => 
                    executeAction(action, i + batchIndex)
                    await Promise.all(batchPromises);
            // Execute all at once
            const promises = actions.map((action, index) => executeAction(action, index));
            if (continueOnError) {
                await Promise.allSettled(promises);
                await Promise.all(promises);
            Name: 'TotalDuration',
            Value: Date.now() - startTime
                message: `Parallel execution completed: ${successCount} successful, ${errorCount} failed`,
                totalActions: actions.length,
                totalDuration: Date.now() - startTime,
     * Execute actions and return first result
    private async executeFirstAction(
        return new Promise<ActionResultSimple>((resolve) => {
            let resolved = false;
            let completedCount = 0;
                    // Check if already resolved
                    if (resolved) return;
                            resolved = true;
                                Name: 'FirstResult',
                                Value: result
                                Name: 'FirstActionName',
                                Value: action.ActionName
                                Name: 'FirstActionIndex',
                                Value: index
                                ResultCode: "FIRST_SUCCESS",
                                    message: "First successful action completed",
                                    actionIndex: index,
                                    duration: Date.now() - startTime,
                                    result: result
                                error: result.Message
                            completedCount++;
                            // Check if all failed
                            if (completedCount === actions.length) {
                                    Name: 'AllResults',
                                    ResultCode: "ALL_FAILED",
                                        message: "All actions failed",
                                        results: results,
                let currentIndex = 0;
                const executeNext = () => {
                    if (currentIndex < actions.length && !resolved) {
                        const action = actions[currentIndex];
                        const index = currentIndex;
                        currentIndex++;
                        executeAction(action, index).then(() => executeNext());
                // Start initial batch
                for (let i = 0; i < Math.min(maxConcurrent, actions.length); i++) {
                    executeNext();
                actions.forEach((action, index) => executeAction(action, index));
            // Handle timeout
            if (timeout > 0) {
                            ResultCode: "TIMEOUT",
                            Message: `Parallel execution timed out after ${timeout}ms`
                }, timeout);
     * Prepare action configuration with context
        if (!context || Object.keys(context).length === 0) {
            return actionConfig;
        // Merge context into params
        // Merge context (existing params take precedence)
        preparedConfig.Params = { ...context, ...preparedConfig.Params };
