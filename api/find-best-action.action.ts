 * Action that finds the best-matching actions for a given task using embedding-based semantic search.
 * This action uses local embeddings to perform fast similarity search across all available actions,
 * returning the most relevant actions based on their descriptions and capabilities.
 * // Find actions for a web search task
 *   ActionName: 'Find Best Action',
 *     Name: 'TaskDescription',
 *     Value: 'Search the internet for information on a topic'
 *     Name: 'MaxResults',
 *     Value: 5
 *     Name: 'MinimumSimilarityScore',
 *     Value: 0.6
@RegisterClass(BaseAction, "Find Best Action")
export class FindBestActionAction extends BaseAction {
     * Executes the Find Best Action action.
     *   - TaskDescription: Description of the task to find actions for (required)
     *   - MaxResults: Maximum number of actions to return (optional, default: 10)
     *   - MinimumSimilarityScore: Minimum similarity score 0-1 (optional, default: 0.5)
     *   - ExcludeAgentManagement: Exclude Agent Management actions (optional, default: true)
     * @returns Action result with matched actions
            // Extract and validate input parameters
            const taskDescription = this.getParamValue(params, 'taskdescription');
            const maxResults = parseInt(this.getParamValue(params, 'maxresults') || '10');
            const minimumSimilarityScore = parseFloat(this.getParamValue(params, 'minimumsimilarityscore') || '0.5');
            const excludeAgentManagement = this.getBooleanParam(params, 'excludeagentmanagement', true);
            // Validate required input
                    ResultCode: 'INVALID_INPUT',
                    Message: 'TaskDescription parameter is required and cannot be empty'
            // Validate numeric ranges
            if (maxResults < 1 || maxResults > 50) {
                    Message: 'MaxResults must be between 1 and 50'
            if (minimumSimilarityScore < 0 || minimumSimilarityScore > 1) {
                    Message: 'MinimumSimilarityScore must be between 0 and 1'
            // Validate contextUser is provided
                    ResultCode: 'MISSING_USER_CONTEXT',
                    Message: 'User context required'
            // Ensure AIEngine is loaded (embeddings computed during initialization)
            await AIEngine.Instance.Config(false, params.ContextUser);
            // Find similar actions using AIEngine's built-in method - no database round trip!
            const matchedActions = await AIEngine.Instance.FindSimilarActions(
                maxResults * 2, // Get 2x results to account for filtering
                minimumSimilarityScore
            // Filter out Agent Management actions if requested
            let filteredActions = matchedActions;
            if (excludeAgentManagement) {
                filteredActions = matchedActions.filter(a =>
                    a.categoryName !== 'Agent Management'
            // Limit to maxResults after filtering
            filteredActions = filteredActions.slice(0, maxResults);
            if (filteredActions.length === 0) {
                    ResultCode: 'NO_ACTIONS_FOUND',
                    Message: `No actions found matching the criteria (minimum similarity: ${minimumSimilarityScore}).`
            // Load action parameters for all matched actions
            const actionIds = filteredActions.map(a => a.actionId);
            const paramsResult = await rv.RunView<MJActionParamEntity>({
                ExtraFilter: `ActionID IN ('${actionIds.join("','")}')`,
                OrderBy: 'ActionID, Name',
            }, params.ContextUser);
            // Group parameters by action ID
            const paramsByActionId = new Map<string, MJActionParamEntity[]>();
            if (paramsResult.Success && paramsResult.Results) {
                for (const param of paramsResult.Results) {
                    const actionId = param.ActionID;
                    if (!paramsByActionId.has(actionId)) {
                        paramsByActionId.set(actionId, []);
                    paramsByActionId.get(actionId)!.push(param);
            // Build response with parameters included
            const actionsWithParams = filteredActions.map(a => ({
                actionId: a.actionId,
                actionName: a.actionName,
                similarityScore: Math.round(a.similarityScore * 100) / 100,
                description: a.description,
                categoryName: a.categoryName,
                status: a.status,
                driverClass: a.driverClass,
                parameters: (paramsByActionId.get(a.actionId) || []).map(p => ({
                    type: p.Type, // Input/Output
                    valueType: p.ValueType,
                    isArray: p.IsArray,
                Name: 'MatchedActions',
                Value: actionsWithParams
                Name: 'MatchCount',
                Value: filteredActions.length
            // Build response message with full descriptions and parameters
            const responseData = {
                message: `Found ${filteredActions.length} relevant action(s)`,
                taskDescription: taskDescription,
                matchCount: filteredActions.length,
                allMatches: actionsWithParams
                Message: `Failed to find best action: ${error instanceof Error ? error.message : String(error)}`
