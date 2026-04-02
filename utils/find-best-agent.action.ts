import { AIAgentPermissionHelper } from "@memberjunction/ai-engine-base";
 * Action that finds the best-matching AI agents for a given task using embedding-based semantic search.
 * This action uses local embeddings to perform fast similarity search across all available agents,
 * returning the most relevant agents based on their descriptions and capabilities.
 * // Find agents for a research task
 *   ActionName: 'Find Best Agent',
 *     Value: 'Research market trends and compile a comprehensive report'
@RegisterClass(BaseAction, "Find Best Agent")
export class FindBestAgentAction extends BaseAction {
    // Singleton initialization removed - AIEngine handles embedding lifecycle
     * Executes the Find Best Agent action.
     *   - TaskDescription: Description of the task to find agents for (required)
     *   - MaxResults: Maximum number of agents to return (optional, default: 5)
     *   - IncludeInactive: Include inactive agents (optional, default: false)
     * @returns Action result with matched agents
            const maxResults = parseInt(this.getParamValue(params, 'maxresults') || '5');
            const includeInactive = this.getBooleanParam(params, 'includeinactive', false);
            if (maxResults < 1 || maxResults > 20) {
                    Message: 'MaxResults must be between 1 and 20'
            // Validate contextUser is provided for permission filtering
                    Message: 'User context required for permission filtering'
            // Find similar agents using AIEngine's built-in method - no database round trip!
            const matchedAgents = await AIEngine.Instance.FindSimilarAgents(
                maxResults * 3, // Get 3x results to account for filtering
            // Filter by user permissions - user must have 'run' permission
            const accessibleAgents = await AIAgentPermissionHelper.GetAccessibleAgents(
                params.ContextUser,
            const accessibleAgentIds = new Set(accessibleAgents.map(a => a.ID));
            // Filter matched agents by permissions
            let permissionFilteredAgents = matchedAgents.filter(a => accessibleAgentIds.has(a.agentId));
            // Filter by status if not including inactive
                permissionFilteredAgents = permissionFilteredAgents.filter(a => a.status === 'Active');
            // Filter by invocation mode - exclude Sub-Agent agents (only show Any or Top-Level)
            // Sub-Agents are meant to be called by other agents, not discovered by users/tools
            const invocationFilteredAgents = permissionFilteredAgents.filter(a =>
                a.invocationMode !== 'Sub-Agent'
            // Limit to maxResults after all filtering
            const filteredAgents = invocationFilteredAgents.slice(0, maxResults);
            if (filteredAgents.length === 0) {
                    ResultCode: 'NO_AGENTS_FOUND',
                    Message: `No accessible agents found matching the criteria (minimum similarity: ${minimumSimilarityScore}). You may not have permission to run the matching agents.`
            // AIEngine already loaded above - use it to get agent actions
            // Create map of agentId -> action names
            const agentActionsMap = new Map<string, string[]>();
            for (const agent of filteredAgents) {
                const agentActions = AIEngine.Instance.AgentActions
                    .filter(aa => aa.AgentID === agent.agentId && aa.Status === 'Active')
                    .map(aa => aa.Action);  // Get action name
                agentActionsMap.set(agent.agentId, agentActions);
                Name: 'MatchedAgents',
                Value: filteredAgents
                Value: filteredAgents.length
            // Build response message with full descriptions and actions
                message: `Found ${filteredAgents.length} accessible agent(s)`,
                matchCount: filteredAgents.length,
                allMatches: filteredAgents.map(a => ({
                    agentId: a.agentId,  // Include agent ID for direct use with Load Agent Spec
                    agentName: a.agentName,
                    similarityScore: Math.round(a.similarityScore * 100) / 100, // Round to 2 decimal places
                    description: a.description,  // Full description, no truncation
                    actions: agentActionsMap.get(a.agentId) || []
                Message: `Failed to find best agent: ${error instanceof Error ? error.message : String(error)}`
