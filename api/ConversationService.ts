import readline from 'readline';
import { AgentService } from './AgentService';
export interface ConversationOptions {
  historyLimit?: number;
export interface ConversationTurn {
  userMessage: string;
  agentResponse?: string;
export class ConversationService {
  private agentService = new AgentService();
  private conversationHistory: ConversationTurn[] = [];
  private conversationMessages: Array<{ role: 'user' | 'assistant'; content: string }> = [];
  private rl?: readline.Interface;
  async startChat(
    initialPrompt?: string, 
    options: ConversationOptions = {}
  ): Promise<void> {
    const logger = new ExecutionLogger(`agents:chat`, agentName, undefined, initialPrompt);
      await this.agentService.initialize();
      // Verify agent exists
      const agent = await this.agentService.findAgent(agentName);
Use 'mj-ai agents:list' to see available agents.`);
      console.log(chalk.cyan(`\n🤖 Starting conversation with: ${chalk.bold(agentName)}`));
      console.log(chalk.dim('Type "exit", "/exit", "quit", or press Ctrl+C to end the conversation\n'));
      this.rl = readline.createInterface({
        output: process.stdout,
        prompt: chalk.blue('You: ')
      // Handle initial prompt if provided
      if (initialPrompt) {
        console.log(chalk.blue('You: ') + initialPrompt);
        await this.processUserMessage(agentName, initialPrompt, logger, options);
      // Start interactive conversation loop
      await this.conversationLoop(agentName, logger, options);
      logger.finalize('FAILED', undefined, error.message);
        throw new Error(`❌ Failed to start conversation
Problem: ${error.message}
1. Verify the agent exists and is configured correctly
2. Check database connection and MJ infrastructure
3. Try with a different agent
  private async conversationLoop(
    logger: ExecutionLogger, 
    options: ConversationOptions
    return new Promise((resolve, reject) => {
      const handleInput = async (input: string) => {
        const trimmedInput = input.trim();
        // Check for exit commands
        if (this.isExitCommand(trimmedInput)) {
          console.log(chalk.yellow('\n👋 Goodbye!'));
          this.rl?.close();
          logger.finalize('SUCCESS', this.conversationHistory);
          resolve();
          // Force process exit
        // Ignore empty input
        if (!trimmedInput) {
          this.rl?.prompt();
          await this.processUserMessage(agentName, trimmedInput, logger, options);
          console.log(chalk.red(`\n❌ Error: ${error.message}\n`));
      this.rl?.on('line', handleInput);
      this.rl?.on('SIGINT', () => {
        logger.finalize('CANCELLED', this.conversationHistory);
      this.rl?.on('close', () => {
      this.rl?.on('error', (error) => {
        reject(error);
      // Start the conversation
  private async processUserMessage(
    userMessage: string,
      logger.logStep('INFO', 'USER', 'Processing user message', { 
        message: userMessage.substring(0, 100) + (userMessage.length > 100 ? '...' : '')
      // Show thinking indicator
      console.log(chalk.dim('\n🤔 Agent is thinking...'));
      // For the first message, conversationMessages will be empty, so the agent won't have context
      // We need to ensure the conversation history includes all messages up to this point
      // but NOT the current message (which will be added by AgentService)
      let result;
      const executionOptions = {
        conversationMessages: [...this.conversationMessages] // Pass current conversation history
        result = await this.agentService.executeAgent(agentName, userMessage, executionOptions);
        result = await ConsoleManager.withSuppressedOutput(async () => {
          return await this.agentService.executeAgent(agentName, userMessage, executionOptions);
        // Display agent response
        const agentMessage = this.extractAgentResponse(result.result);
        const formattedMessage = TextFormatter.formatText(agentMessage, {
          indent: 3,
          preserveParagraphs: true,
          highlightCode: true
        console.log(chalk.green(`\n🤖 ${agentName}: `));
        console.log(formattedMessage);
          console.log(chalk.dim(`\n⏱️  Response time: ${duration}ms`));
            console.log(chalk.dim(`📋 Execution ID: ${result.executionId}`));
        // Add messages to conversation context
        this.conversationMessages.push({ role: 'user', content: userMessage });
        this.conversationMessages.push({ role: 'assistant', content: agentMessage });
        // Add to conversation history
        this.conversationHistory.push({
          userMessage,
          agentResponse: agentMessage,
          success: true
        logger.logStep('SUCCESS', 'AGENT', 'User message processed successfully', {
          responseLength: result.result?.length || 0,
          duration
        console.log(chalk.red(`\n❌ ${agentName}: Error occurred`));
          console.log(chalk.red(result.error));
        // Add user message to conversation context even if agent failed
        // Add failed turn to history
          error: result.error
        logger.logStep('ERROR', 'AGENT', 'User message processing failed', {
          error: result.error,
      // Manage conversation history size
      if (options.historyLimit && this.conversationHistory.length > options.historyLimit) {
        this.conversationHistory = this.conversationHistory.slice(-options.historyLimit);
        // Also trim conversation messages (keep last N pairs, each pair = user + assistant)
        const maxMessages = options.historyLimit * 2;
        if (this.conversationMessages.length > maxMessages) {
          this.conversationMessages = this.conversationMessages.slice(-maxMessages);
      console.log(chalk.red(`\n❌ Failed to process message: ${error.message}`));
      // Add user message to conversation context even on system error
        error: error.message
      logger.logStep('ERROR', 'SYSTEM', 'Message processing error', {
        error: error.message,
  private isExitCommand(input: string): boolean {
    const exitCommands = ['exit', 'quit', 'bye', 'goodbye', 'stop', '/exit'];
    return exitCommands.includes(input.toLowerCase());
  public getConversationHistory(): ConversationTurn[] {
    return [...this.conversationHistory];
  public clearHistory(): void {
    this.conversationHistory = [];
  public async exportConversation(filePath?: string): Promise<string> {
    const exportData = {
      totalTurns: this.conversationHistory.length,
      successfulTurns: this.conversationHistory.filter(t => t.success).length,
      failedTurns: this.conversationHistory.filter(t => !t.success).length,
      conversation: this.conversationHistory
    const exportJson = JSON.stringify(exportData, null, 2);
    if (filePath) {
      const fs = await import('fs');
      fs.writeFileSync(filePath, exportJson, 'utf8');
    return exportJson;
  private extractAgentResponse(result: any): string {
      return 'No response from agent';
    if (typeof result === 'string') {
    } else if (typeof result === 'object') {
      // For chat mode, prioritize actual user-facing messages
      if (result.message) {
        return result.message;
      } else if (result.userMessage) {
        return result.userMessage;
      } else if (result.nextStep && result.nextStep.userMessage) {
        return result.nextStep.userMessage;
        // Fallback to JSON representation with nice formatting
        return TextFormatter.formatJSON(result);
    return String(result);
