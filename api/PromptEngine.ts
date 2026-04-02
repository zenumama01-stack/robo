 * Prompt execution engine combining Nunjucks templating with AI/Core
import nunjucks from 'nunjucks';
import { PromptFileLoader } from './PromptFileLoader.js';
import { PromptExecutionResult } from '../types/prompts.js';
import { CleanAndParseJSON } from '@memberjunction/global';
export type GuardrailCheckFn = () => { canContinue: boolean; reason?: string };
export class PromptEngine {
  private guardrailCheck?: GuardrailCheckFn;
    private config: AIConfig,
    promptsDir: string
    // Initialize Nunjucks with custom loader
    const loader = new PromptFileLoader(promptsDir);
    this.nunjucksEnv = new nunjucks.Environment(loader, {
      autoescape: false,
      dev: true
    // Add custom filters
    this.addCustomFilters();
    // Initialize AI/Core LLM
    this.llm = this.createLLM();
   * Set guardrail checking callback
   * This will be called before every LLM execution to check if we should continue
  public setGuardrailCheck(checkFn: GuardrailCheckFn): void {
    this.guardrailCheck = checkFn;
   * Initialize the prompt loader
    const env = this.nunjucksEnv as { loaders?: PromptFileLoader[] };
    const loader = env.loaders?.[0];
    if (loader) {
      await loader.loadAll();
   * Create appropriate LLM based on provider configuration
   * Uses MJ ClassFactory pattern for BaseLLM instantiation
  private createLLM(): BaseLLM {
    // Use shared factory (DRY principle)
    return createLLMInstance(this.config.provider, this.config.apiKey);
   * Add custom Nunjucks filters
   * Pattern from Templates package
  private addCustomFilters(): void {
    // JSON formatting with indentation
    this.nunjucksEnv.addFilter('json', (obj: any, indent: number = 2) => {
        return `[Error serializing to JSON: ${(error as Error).message}]`;
    // Compact JSON
    this.nunjucksEnv.addFilter('jsoninline', (obj: any) => {
        return JSON.stringify(obj);
    // Parse JSON strings
    this.nunjucksEnv.addFilter('jsonparse', (str: string) => {
    // Array join filter
    this.nunjucksEnv.addFilter('join', (arr: any[], separator: string = ', ') => {
      return Array.isArray(arr) ? arr.join(separator) : arr;
   * Render a Nunjucks template with context
   * Pattern from Templates.renderTemplateAsync()
  private async renderTemplate(promptName: string, context: any): Promise<string> {
      this.nunjucksEnv.render(promptName, context, (err, result) => {
          console.error(`[PromptEngine] FATAL: Template rendering failed for ${promptName}`);
          console.error(`[PromptEngine] Error:`, err.message);
          console.error(`[PromptEngine] Context keys:`, Object.keys(context));
          console.error(`[PromptEngine] Terminating process due to template rendering failure`);
        resolve(result || '');
   * Execute a prompt with AI/Core
   * Main entry point for DBAutoDoc analysis
  public async executePrompt<T>(
    promptName: string,
    context: any,
      responseFormat?: 'JSON' | 'Text';
  ): Promise<PromptExecutionResult<T>> {
      // Check guardrails before executing
      if (this.guardrailCheck) {
        const check = this.guardrailCheck();
        if (!check.canContinue) {
            errorMessage: `Guardrail limit exceeded: ${check.reason}`,
            guardrailExceeded: true
      // 1. Render Nunjucks template with context
      const renderedPrompt = await this.renderTemplate(promptName, context);
      // 2. Build ChatParams for AI/Core
      const messages = [];
      // Add system prompt if provided
      if (options?.systemPrompt) {
          content: options.systemPrompt
      // Add rendered user prompt
        content: renderedPrompt
        model: this.config.model,
        maxOutputTokens: options?.maxTokens ?? this.config.maxTokens,
        responseFormat: options?.responseFormat ?? 'JSON',
        ...(options?.temperature != null && { temperature: options.temperature }),
        ...(this.config.temperature != null && options?.temperature == null && { temperature: this.config.temperature }),
        ...(this.config.effortLevel != null && { effortLevel: this.config.effortLevel.toString() }) // Optional 1-100, BaseLLM drivers handle if supported
      // 3. Execute with AI/Core (follows RunView pattern - doesn't throw)
      // 4. Check success (IMPORTANT: like RunView, check .success property)
          errorMessage: chatResult.errorMessage || 'Unknown error',
      // 5. Extract result and parse JSON if needed
      let parsedResult: T;
      if (options?.responseFormat === 'JSON') {
        // Use shared MJGlobal utility for JSON cleaning (handles markdown fences, double-escaping, etc.)
        const parsed = CleanAndParseJSON<T>(content, true);
        if (parsed === null) {
            errorMessage: `Failed to parse JSON response\n\nRaw content:\n${content}`,
            tokensUsed: usage?.totalTokens || 0,
            promptInput: renderedPrompt,
            promptOutput: content
        parsedResult = content as unknown as T;
        result: parsedResult,
        cost: usage?.cost,
        errorMessage: `Prompt execution failed: ${(error as Error).message}`,
   * Execute multiple prompts in parallel
   * Uses AI/Core's ChatCompletions for efficiency
  public async executePromptsParallel<T>(
    requests: Array<{
      context: any;
  ): Promise<Array<PromptExecutionResult<T>>> {
      // Render all templates first
      const renderedPrompts = await Promise.all(
        requests.map(req => this.renderTemplate(req.promptName, req.context))
      // Build ChatParams array
      const paramsArray: ChatParams[] = renderedPrompts.map((prompt, i) => {
          messages: [{ role: 'user', content: prompt }],
          maxOutputTokens: requests[i].options?.maxTokens ?? this.config.maxTokens,
          responseFormat: requests[i].options?.responseFormat ?? 'JSON'
        // Only add temperature if explicitly provided (don't default to 0.1)
        if (requests[i].options?.temperature != null) {
          params.temperature = requests[i].options!.temperature;
        } else if (this.config.temperature != null) {
          params.temperature = this.config.temperature;
      // Execute in parallel using AI/Core
      const results = await this.llm.ChatCompletions(paramsArray);
      // Parse and return
      return results.map((chatResult, i) => {
            errorMessage: chatResult.errorMessage,
            errorMessage: `JSON parse error`,
          result: parsed,
          cost: usage?.cost
      // If rendering or execution fails completely, return array of errors
      return requests.map(() => ({
        errorMessage: `Parallel execution failed: ${(error as Error).message}`,
