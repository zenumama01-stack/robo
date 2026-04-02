 * @fileoverview Driver for executing scheduled AI Agent jobs
    AgentJobConfiguration
 * Driver for executing scheduled AI Agent jobs
 *   AgentID: string,
 *   ConversationID?: string,
 *   StartingPayload?: any,
 *   InitialMessage?: string,
 *   ConfigurationID?: string,
 *   OverrideModelID?: string
 *   AgentRunID: string,
 *   TokensUsed: number,
 *   Cost: number,
 *   ConversationID?: string
@RegisterClass(BaseScheduledJob, 'AgentScheduledJobDriver')
export class AgentScheduledJobDriver extends BaseScheduledJob {
        // Parse agent-specific configuration
        const config = this.parseConfiguration<AgentJobConfiguration>(context.Schedule);
        const agent = await this.loadAgent(config.AgentID, context.ContextUser);
        this.log(`Executing agent: ${agent.Name}`);
        // Build conversation messages - if initial message provided, add it as user message
        const conversationMessages = config.InitialMessage
            ? [{ role: 'user' as const, content: config.InitialMessage }]
        const result = await runner.RunAgent({
            payload: config.StartingPayload,
            contextUser: context.ContextUser
        // Link agent run back to scheduled job run
        if (result.agentRun.ID && context.Run.ID) {
            result.agentRun.ScheduledJobRunID = context.Run.ID;
            await result.agentRun.Save();
        // Build result with agent-specific details
            ErrorMessage: result.agentRun.ErrorMessage || undefined,
                AgentRunID: result.agentRun.ID,
                TokensUsed: result.agentRun.TotalTokensUsed,
                Cost: result.agentRun.TotalCost,
                ConversationID: result.agentRun.ConversationID,
                Status: result.agentRun.Status
            const config = this.parseConfiguration<AgentJobConfiguration>(schedule);
            if (!config.AgentID) {
                    'Configuration.AgentID',
                    'AgentID is required',
                    config.AgentID,
            // Validate StartingPayload is valid JSON if provided
            if (config.StartingPayload) {
                    JSON.stringify(config.StartingPayload);
                        'Configuration.StartingPayload',
                        'StartingPayload must be valid JSON',
                        config.StartingPayload,
            ? `Scheduled Agent Completed: ${context.Schedule.Name}`
            : `Scheduled Agent Failed: ${context.Schedule.Name}`;
            ? `The scheduled agent "${context.Schedule.Name}" completed successfully.\n\n` +
              `Tokens Used: ${details?.TokensUsed || 'N/A'}\n` +
              `Cost: $${details?.Cost?.toFixed(6) || 'N/A'}\n` +
              `Agent Run ID: ${details?.AgentRunID}`
            : `The scheduled agent "${context.Schedule.Name}" failed.\n\n` +
              `Error: ${result.ErrorMessage}\n` +
              `Agent Run ID: ${details?.AgentRunID || 'N/A'}`;
                JobType: 'Agent',
                AgentID: config.AgentID,
                AgentRunID: details?.AgentRunID
    private async loadAgent(agentId: string, contextUser: UserInfo): Promise<AIAgentEntityExtended> {
        const loaded = await agent.Load(agentId);
