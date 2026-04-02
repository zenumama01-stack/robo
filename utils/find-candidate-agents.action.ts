 *   ActionName: 'Find Candidate Agents',
@RegisterClass(BaseAction, "Find Candidate Agents")
     * Executes the Find Candidate Agents action.
     *   - ExcludeSubAgents: Exclude agents with invocation mode 'Sub-Agent' (optional, default: true)
            const excludeSubAgents = this.getBooleanParam(params, 'excludesubagents', true);
            // Filter by invocation mode and parent relationship if excludeSubAgents is true
            // Sub-Agents and child agents are meant to be called by other agents, not typically discovered by users/tools
            let invocationFilteredAgents = permissionFilteredAgents;
            if (excludeSubAgents) {
                invocationFilteredAgents = permissionFilteredAgents.filter(a =>
                    a.invocationMode !== 'Sub-Agent' && !a.parentId
            // Create map of agentId -> sub-agents (name and description)
            const agentSubAgentsMap = new Map<string, Array<{name: string, description: string}>>();
                const subAgents: Array<{name: string, description: string}> = [];
                // Find child agents (ParentID = this agent)
                    a.ParentID === agent.agentId && a.Status === 'Active'
                subAgents.push(...childAgents.map(a => ({
                    name: a.Name,
                    description: a.Description || ''
                // Find related agents (via AgentRelationships)
                const relationships = AIEngine.Instance.AgentRelationships.filter(r =>
                    r.AgentID === agent.agentId && r.Status === 'Active'
                for (const rel of relationships) {
                    const relatedAgent = AIEngine.Instance.Agents.find(a => a.ID === rel.SubAgentID);
                    if (relatedAgent && relatedAgent.Status === 'Active') {
                        subAgents.push({
                            name: relatedAgent.Name,
                            description: relatedAgent.Description || ''
                agentSubAgentsMap.set(agent.agentId, subAgents);
            // Build response message with full descriptions, actions, and sub-agents
                    actions: agentActionsMap.get(a.agentId) || [],
                    subAgents: agentSubAgentsMap.get(a.agentId) || [],  // Sub-agents with name and description
                    defaultArtifactType: a.defaultArtifactType || null  // Artifact type this agent produces
                Message: `Failed to Find Candidate Agents: ${error instanceof Error ? error.message : String(error)}`
