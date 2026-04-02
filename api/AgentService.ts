import { AgentRunner } from '@memberjunction/ai-agents';
import { ExecuteAgentResult, AgentExecutionProgressCallback, AIAgentEntityExtended } from '@memberjunction/ai-core-plus';
import { AgentInfo, ExecutionResult } from '../lib/output-formatter';
import { ConsoleManager } from '../lib/console-manager';
export interface AgentExecutionOptions {
  conversationMessages?: Array<{ role: 'user' | 'assistant'; content: string }>;
export class AgentService {
  private metadata?: Metadata;
      throw new Error(`Failed to initialize Agent Service: ${error?.message || 'Unknown error'}`);
  async listAgents(): Promise<AgentInfo[]> {
      const result = await rv.RunView<AIAgentEntityExtended>({
        EntityName: 'MJ: AI Agents',
        throw new Error(`Failed to load agents: ${result.ErrorMessage}`);
      const agents = result.Results || [];
      return agents
        .filter(agent => agent.Name) // Filter out agents without names
        .map(agent => ({
          name: agent.Name!,
          description: agent.Description || undefined,
          status: 'available' as const, // For now, assume all agents are available
          lastUsed: undefined // We could track this in the future
      throw new Error(`❌ Failed to list agents
Context: Loading AI agents from database
1. Verify AI Agents entity exists in your database
2. Check user permissions to access AI Agents
3. Ensure @memberjunction/ai-agents package is built
For help with agent configuration, see the MJ documentation.`);
  async findAgent(agentName: string): Promise<AIAgentEntityExtended | null> {
        ExtraFilter: `Name = '${agentName.replace(/'/g, "''")}'`,
      throw new Error(`Failed to find agent "${agentName}": ${error?.message || 'Unknown error'}`);
  async executeAgent(
    agentName: string, 
    prompt: string, 
    options: AgentExecutionOptions = {}
    const logger = new ExecutionLogger(`agents:run`, agentName, undefined, prompt);
      logger.logStep('INFO', 'SYSTEM', 'Finding agent', { agentName });
      const agent = await this.findAgent(agentName);
        const suggestions = await this.getSimilarAgentNames(agentName);
        throw new Error(`❌ Agent not found: "${agentName}"
Problem: No agent exists with the specified name
Available agents: Use 'mj-ai agents:list' to see all agents${suggestionText}
1. Check the agent name spelling
2. Use 'mj-ai agents:list' to see available agents
3. Verify the agent is deployed and enabled`);
      logger.logStep('SUCCESS', 'SYSTEM', 'Agent found', { 
        agentId: agent.ID, 
        agentName: agent.Name 
      logger.logStep('INFO', 'AGENT', 'Starting agent execution', { 
        prompt: prompt.substring(0, 100) + (prompt.length > 100 ? '...' : '')
      // Build conversation messages - always include the conversation history plus current message
      let conversationMessages: Array<{ role: 'user' | 'assistant'; content: string }>;
      if (options.conversationMessages) {
        // We have conversation history - clone it and append current message
        conversationMessages = [...options.conversationMessages, {
          role: 'user' as const,
          content: prompt
        // No conversation history - create new conversation with just current message
        conversationMessages = [{
      // Prepare progress callbacks
      let lastProgressOutput = '';
      const callbacks = {
        onProgress: ((progress) => {
          const stepIcons: Record<string, string> = {
            'initialization': '🚀',
            'validation': '✓',
            'prompt_execution': '💭',
            'action_execution': '⚙️',
            'subagent_execution': '🤖',
            'decision_processing': '🧠',
            'finalization': '✨'
          const icon = stepIcons[progress.step] || '→';
          // Use stepCount from metadata if available, otherwise fall back to percentage (deprecated)
          let progressIndicator: string;
          if (progress.metadata?.stepCount != null) {
            progressIndicator = `Step ${progress.metadata.stepCount}`.padStart(7, ' ');
          } else if (progress.percentage != null) {
            progressIndicator = `${progress.percentage.toFixed(0).padStart(3, ' ')}%`;
            progressIndicator = '   ';
          if (options.verbose) {
            // In verbose mode, show full progress updates
            console.log(
              chalk.blue(`\n  ${icon} [${progressIndicator}]`),
              chalk.bold(progress.step.replace(/_/g, ' ')),
              chalk.dim(`- ${progress.message}`)
            if (progress.metadata && Object.keys(progress.metadata).length > 0) {
              console.log(chalk.dim(`     ${JSON.stringify(progress.metadata)}`));
            // In non-verbose mode, show truncated progress on the same line
            const display = `${icon} [${progressIndicator}] ${progress.step.replace(/_/g, ' ')}: ${progress.message}`;
            const truncated = display.substring(0, 80);
            const finalDisplay = truncated + (display.length > 80 ? '...' : '');
            // Clear previous line and write new content
            if (lastProgressOutput) {
              process.stdout.write('\r' + ' '.repeat(lastProgressOutput.length) + '\r');
            process.stdout.write(finalDisplay);
            lastProgressOutput = finalDisplay;
            // Add newline when finalizing
            if (progress.step === 'finalization' && lastProgressOutput) {
              process.stdout.write('\n');
              lastProgressOutput = '';
        }) as AgentExecutionProgressCallback
      // Suppress console output during agent execution unless verbose
      let executionResult: ExecuteAgentResult;
        executionResult = await agentRunner.RunAgent({
          agent: agent,
          conversationMessages,
          contextUser: this.contextUser!,
          onProgress: callbacks.onProgress
        executionResult = await ConsoleManager.withSuppressedOutput(async () => {
          return await agentRunner.RunAgent({
      // Clear any remaining progress output
      if (lastProgressOutput && !options.verbose) {
      if (executionResult && executionResult.success) {
        // Get the result from Message field first, then FinalPayload if Message is empty
        let resultContent: any = executionResult.agentRun.Message;
        if (!resultContent && executionResult.agentRun.FinalPayload) {
          // Parse FinalPayload if Message is not available
            const finalPayload = JSON.parse(executionResult.agentRun.FinalPayload);
            resultContent = finalPayload;
            resultContent = executionResult.agentRun.FinalPayload;
        // Use payload if neither Message nor FinalPayload has content
        if (!resultContent && executionResult.payload) {
          resultContent = executionResult.payload;
        logger.logStep('SUCCESS', 'AGENT', 'Agent execution completed', {
          result: typeof resultContent === 'string' 
            ? resultContent.substring(0, 200) + (resultContent.length > 200 ? '...' : '')
            : resultContent
          entityName: agentName,
          result: resultContent,
        logger.finalize('SUCCESS', resultContent);
        const errorMessage = executionResult?.agentRun?.ErrorMessage || 'Unknown execution error';
      // Include stack trace in verbose mode
      const errorDetails = options.verbose && error?.stack 
        ? `${errorMessage}\n\nStack trace:\n${error.stack}`
        : errorMessage;
        error: errorDetails,
        const stackInfo = options.verbose && error?.stack
          ? `\n\nStack trace:\n${error.stack}`
        throw new Error(`❌ Agent execution failed
Agent: ${agentName}
Context: Running agent with user prompt
1. Check the agent configuration and parameters
2. Verify required AI models are available
4. Try with a simpler prompt to test basic functionality
Log file: ${logger.getLogFilePath()}${stackInfo}`);
  private async getSimilarAgentNames(searchName: string): Promise<string[]> {
      const agents = await this.listAgents();
        .filter(agent => 
          agent.name.toLowerCase().includes(searchLower) ||
          searchLower.includes(agent.name.toLowerCase()) ||
          this.calculateSimilarity(agent.name.toLowerCase(), searchLower) > 0.6
        .map(agent => agent.name)
    // For CLI usage, we'll use the first available user
    // In a real application, you might want to configure which user to use
    const user = UserCache.Users[0];
