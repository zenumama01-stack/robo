import { UserInfo, Metadata } from '@memberjunction/core';
import { MJAIModelEntity } from '@memberjunction/core-entities';
import { ExecutionResult } from '../lib/output-formatter';
export interface PromptExecutionOptions {
  model?: string;
  temperature?: number;
  maxTokens?: number;
  systemPrompt?: string;
  configurationId?: string;
export interface PromptInfo {
export class PromptService {
      throw new Error(`Failed to initialize Prompt Service: ${error?.message || 'Unknown error'}`);
  async listPrompts(): Promise<PromptInfo[]> {
      // For now, return a simple list indicating prompts can be run dynamically
      // In the future, this could list saved prompts from the database
          name: 'Direct Prompt Execution',
          model: 'Default model from configuration',
          maxTokens: 4000
      throw new Error(`Failed to list prompts: ${error?.message || 'Unknown error'}`);
  async executePrompt(
    options: PromptExecutionOptions = {}
    const logger = new ExecutionLogger('prompts:run', 'Direct Prompt', undefined, prompt);
      logger.logStep('INFO', 'SYSTEM', 'Initializing prompt runner', {
        model: options.model,
        temperature: options.temperature,
        maxTokens: options.maxTokens
      // For direct prompt execution, we'll use the AI module directly
      const aiModule = await import('@memberjunction/ai');
      const AIEngine = (aiModule as any).AIEngine;
      // Build the messages array
      const messages: Array<{role: 'system' | 'user' | 'assistant', content: string}> = [];
      if (options.systemPrompt) {
        messages.push({ role: 'system', content: options.systemPrompt });
      messages.push({ role: 'user', content: prompt });
      logger.logStep('INFO', 'AI_MODEL', 'Executing prompt', {
        messageCount: messages.length,
        promptLength: prompt.length
      // Get the LLM instance
      const llm = AIEngine.Instance.LLM;
      // Execute the prompt using the LLM directly
      const result = await llm.ChatCompletion({
        messages,
        max_tokens: options.maxTokens
        logger.logStep('SUCCESS', 'AI_MODEL', 'Prompt execution completed', {
          modelUsed: result.model,
          tokensUsed: result.usage?.total_tokens,
        const executionResult: ExecutionResult = {
          entityName: 'Direct Prompt',
          result: result.result,
        // Include model and usage info in the result
        executionResult.result = {
          response: result.data,
            vendorUsed: 'OpenAI', // Default for now
            configurationUsed: options.configurationId
          usage: result.usage
        logger.finalize('SUCCESS', executionResult.result);
        return executionResult;
        const errorMessage = result.errorMessage || 'Unknown execution error';
        logger.logError(errorMessage, 'AI_MODEL');
        throw new Error(`❌ Prompt execution failed
Context: Executing direct prompt with AI model
1. Check that AI models are configured and available
2. Verify database connection is working
3. Try with a simpler prompt to test basic functionality
4. Check execution logs for detailed error information
  async listAvailableModels(): Promise<Array<{name: string, vendor: string, description?: string}>> {
      const models = AIEngine.Instance.Models;
      return models.map((model: MJAIModelEntity) => ({
        name: model.Name,
        vendor: model.Vendor,
        description: model.Description
      throw new Error(`Failed to list available models: ${error?.message || 'Unknown error'}`);
