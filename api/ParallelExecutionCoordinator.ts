import { LogError, LogStatus, Metadata } from '@memberjunction/core';
import { BaseLLM, ChatParams, ChatResult, ChatMessageRole, ChatMessage, GetAIAPIKey } from '@memberjunction/ai';
import { AIPromptEntityExtended, AIPromptRunEntityExtended } from '@memberjunction/ai-core-plus';
  ExecutionTask,
  ExecutionTaskResult,
  ExecutionGroup,
  ParallelExecutionConfig,
  ParallelExecutionResult,
  ResultSelectionConfig,
  ParallelExecutionProgress,
  TokenUsageUpdate,
} from './ParallelExecution';
 * Interface for progress callbacks to avoid circular dependency issues
interface ProgressCallbacksInterface {
  getStreamingConfig?: () => {
    enabled?: boolean;
    callbacks?: {
 * Helper class for tracking parallel execution progress
class ParallelProgressTracker {
  private totalTasks: number;
  private completedTasks: number = 0;
  private successfulTasks: number = 0;
  private failedTasks: number = 0;
  private cancelledTasks: number = 0;
  private currentGroup: number = 0;
  private totalGroups: number;
  private activeTasks: string[] = [];
  private totalTokenUsage: TokenUsageUpdate = {};
  private startTime: Date;
  private progressCallbacks: ProgressCallbacksInterface | null;
  constructor(totalTasks: number, totalGroups: number, progressCallbacks: ProgressCallbacksInterface | null) {
    this.totalTasks = totalTasks;
    this.totalGroups = totalGroups;
    this.startTime = new Date();
    this.progressCallbacks = progressCallbacks;
  updateProgress(currentGroup: number): void {
    this.currentGroup = currentGroup;
    this.sendProgressUpdate();
  addActiveTask(taskId: string): void {
    if (!this.activeTasks.includes(taskId)) {
      this.activeTasks.push(taskId);
  removeActiveTask(taskId: string): void {
    const index = this.activeTasks.indexOf(taskId);
    if (index > -1) {
      this.activeTasks.splice(index, 1);
  taskCompleted(result: ExecutionTaskResult): void {
    this.completedTasks++;
    this.removeActiveTask(result.task.taskId);
      this.successfulTasks++;
      if (result.tokensUsed) {
        this.totalTokenUsage.totalTokens = (this.totalTokenUsage.totalTokens || 0) + result.tokensUsed;
    } else if (result.cancelled) {
      this.cancelledTasks++;
      this.failedTasks++;
    // Send task completion update if enabled
    if (this.progressCallbacks?.getStreamingConfig()?.enabled) {
      const config = this.progressCallbacks.getStreamingConfig();
      if (config.callbacks?.OnTaskComplete) {
        config.callbacks.OnTaskComplete(result, this.getCurrentProgress());
  private getCurrentProgress(): ParallelExecutionProgress {
      completedTasks: this.completedTasks,
      totalTasks: this.totalTasks,
      successfulTasks: this.successfulTasks,
      failedTasks: this.failedTasks,
      cancelledTasks: this.cancelledTasks,
      currentGroup: this.currentGroup,
      totalGroups: this.totalGroups,
      totalTokenUsage: this.totalTokenUsage,
      totalElapsedTimeMS: new Date().getTime() - this.startTime.getTime(),
      activeTasks: [...this.activeTasks],
  private sendProgressUpdate(): void {
      if (config.callbacks?.OnParallelProgress) {
        config.callbacks.OnParallelProgress(this.getCurrentProgress());
 * Coordinates parallel execution of AI prompt tasks across multiple models and execution groups.
 * This class manages the complex orchestration of:
 * - Grouping tasks by execution group
 * - Sequential execution of groups with parallel execution within groups
 * - Result aggregation and selection
 * - Error handling and partial result collection
 * - Performance monitoring and metrics collection
 * - Real-time progress tracking and streaming updates
export class ParallelExecutionCoordinator {
  private readonly _defaultConfig: ParallelExecutionConfig;
   * Creates a new parallel execution coordinator with default configuration.
    this._defaultConfig = {
      maxConcurrentExecutions: 5,
      taskTimeoutMS: 30000, // 30 seconds
      failFast: false,
      collectPartialResults: true,
      maxRetries: 3,
      retryDelayMS: 1000,
   * Executes multiple tasks in parallel according to their execution groups.
   * Tasks are grouped by their executionGroup property and executed sequentially by group.
   * Within each group, tasks are executed in parallel up to the concurrency limit.
   * @param tasks - Array of execution tasks to process
   * @param config - Optional configuration to override defaults
   * @param parentPromptRunId - Optional parent prompt run ID for hierarchical logging
   * @param cancellationToken - Optional cancellation token to abort execution
   * @param progressCallbacks - Optional callbacks for progress tracking
   * @param agentRunId - Optional agent run ID to link prompt executions to parent agent run
   * @returns Promise<ParallelExecutionResult> - Aggregated results from all executions
  public async executeTasksInParallel(
    tasks: ExecutionTask[],
    config?: Partial<ParallelExecutionConfig>,
    parentPromptRunId?: string,
    progressCallbacks?: ProgressCallbacksInterface, // Using interface to avoid circular dependency
    agentRunId?: string,
  ): Promise<ParallelExecutionResult> {
    const executionConfig = { ...this._defaultConfig, ...config };
      LogStatus('Parallel execution cancelled before starting');
        taskResults: [],
        groupResults: new Map(),
        totalExecutionTimeMS: 0,
        successCount: 0,
        failureCount: 0,
        cancelledCount: tasks.length,
        totalTokensUsed: 0,
        errors: ['Execution was cancelled before starting'],
        startTime,
    LogStatus(`Starting parallel execution of ${tasks.length} tasks with config: ${JSON.stringify(executionConfig)}`);
      // Propagate cancellation token to all tasks
        tasks.forEach((task) => {
          task.cancellationToken = cancellationToken;
      // Group tasks by execution group
      const executionGroups = this.groupTasksByExecutionGroup(tasks);
      // Initialize progress tracker
      const progressTracker = new ParallelProgressTracker(tasks.length, executionGroups.length, progressCallbacks);
      // Execute groups sequentially, tasks within groups in parallel
      const allResults = await this.executeGroupsSequentially(params, executionGroups, executionConfig, parentPromptRunId, cancellationToken, progressTracker, agentRunId);
      // Aggregate results and calculate metrics
      const result = this.aggregateResults(allResults, startTime, new Date());
        `Parallel execution completed: ${result.successCount}/${tasks.length} tasks successful, ${result.failureCount} failed, ${result.cancelledCount} cancelled`,
      LogError(`Parallel execution failed: ${error.message}`);
        totalExecutionTimeMS: new Date().getTime() - startTime.getTime(),
        failureCount: tasks.length,
        cancelledCount: 0,
        errors: [error.message],
   * Selects the best result from multiple parallel execution results.
   * Uses various strategies to determine which result should be considered
   * the "best" output from the parallel executions.
   * @param results - Array of successful execution results to choose from
   * @param config - Configuration for result selection method
   * @returns Promise<ExecutionTaskResult | null> - The selected best result, or null if none suitable
  public async selectBestResult(
    results: ExecutionTaskResult[],
    config: ResultSelectionConfig,
  ): Promise<ExecutionTaskResult | null> {
    if (results.length === 0) {
    if (results.length === 1) {
    LogStatus(`Selecting best result from ${results.length} candidates using method: ${config.method}`);
    switch (config.method) {
      case 'First':
        return this.selectFirstResult(results);
      case 'Random':
        return this.selectRandomResult(results);
      case 'PromptSelector':
        return await this.selectResultWithPrompt(results, config.selectorPromptId!, parentPromptRunId, cancellationToken);
      case 'Consensus':
        return this.selectConsensusResult(results);
        LogError(`Unknown result selection method: ${config.method}`);
   * Tasks with the same group number will be executed in parallel.
   * @param tasks - Array of tasks to group
   * @returns ExecutionGroup[] - Sorted array of execution groups
  private groupTasksByExecutionGroup(tasks: ExecutionTask[]): ExecutionGroup[] {
    const groupMap = new Map<number, ExecutionTask[]>();
    // Group tasks by execution group number
    for (const task of tasks) {
      const groupNumber = task.executionGroup;
      if (!groupMap.has(groupNumber)) {
        groupMap.set(groupNumber, []);
      groupMap.get(groupNumber)!.push(task);
    // Convert to ExecutionGroup array and sort by group number
    const groups: ExecutionGroup[] = [];
    for (const [groupNumber, groupTasks] of groupMap) {
      // Sort tasks within group by priority (higher priority first)
      groupTasks.sort((a, b) => b.priority - a.priority);
      groups.push({
        groupNumber,
        tasks: groupTasks,
    // Sort groups by group number (execute in ascending order)
    groups.sort((a, b) => a.groupNumber - b.groupNumber);
    LogStatus(`Grouped ${tasks.length} tasks into ${groups.length} execution groups`);
    return groups;
   * Executes execution groups sequentially, with parallel execution within each group.
   * @param groups - Array of execution groups to process
   * @param config - Execution configuration
   * @param parentPromptRunId - Optional parent prompt run ID for tracking
   * @param progressTracker - Progress tracker for monitoring execution
   * @returns Promise<ExecutionTaskResult[]> - All task results from all groups
  private async executeGroupsSequentially(
    groups: ExecutionGroup[],
    config: ParallelExecutionConfig,
    progressTracker?: ParallelProgressTracker,
  ): Promise<ExecutionTaskResult[]> {
    const allResults: ExecutionTaskResult[] = [];
    for (const group of groups) {
      // Check for cancellation before each group
        LogStatus(`Group execution cancelled at group ${group.groupNumber}`);
        // Create cancelled results for remaining tasks
        const cancelledResults = group.tasks.map((task) => ({
          task,
          errorMessage: 'Task cancelled',
          startTime: new Date(),
        allResults.push(...cancelledResults);
      LogStatus(`Executing group ${group.groupNumber} with ${group.tasks.length} tasks`);
      // Update progress tracker for current group
      progressTracker?.updateProgress(group.groupNumber);
      const groupResults = await this.executeGroupInParallel(params, group, config, parentPromptRunId, cancellationToken, progressTracker, agentRunId);
      allResults.push(...groupResults);
      // Check if we should fail fast
      if (config.failFast && groupResults.some((r) => !r.success)) {
        const failedTasks = groupResults.filter((r) => !r.success);
        LogError(`Failing fast due to ${failedTasks.length} failed tasks in group ${group.groupNumber}`);
    return allResults;
   * Executes all tasks within a single execution group in parallel.
   * @param group - The execution group to process
   * @returns Promise<ExecutionTaskResult[]> - Results from all tasks in the group
  private async executeGroupInParallel(
    group: ExecutionGroup,
    const maxConcurrent = Math.min(config.maxConcurrentExecutions, group.tasks.length);
    const results: ExecutionTaskResult[] = [];
    const executing: Promise<ExecutionTaskResult>[] = [];
    let taskIndex = 0;
    let executionOrder = 0;
    // Process tasks with concurrency limit
    while (taskIndex < group.tasks.length || executing.length > 0) {
        LogStatus(`Task execution cancelled in group ${group.groupNumber}`);
        // Cancel remaining tasks
        while (taskIndex < group.tasks.length) {
          const task = group.tasks[taskIndex++];
          results.push({
      // Start new tasks up to concurrency limit
      while (executing.length < maxConcurrent && taskIndex < group.tasks.length) {
        progressTracker?.addActiveTask(task.taskId);
        const execution = this.executeTask(params, task, config, parentPromptRunId, executionOrder++, agentRunId);
        executing.push(execution);
      // Wait for at least one task to complete
      if (executing.length > 0) {
        const result = await Promise.race(executing);
        // Notify progress tracker of task completion
        progressTracker?.taskCompleted(result);
        // Remove completed task from executing array
        const completedIndex = executing.findIndex((p) => p === Promise.resolve(result));
        if (completedIndex !== -1) {
          executing.splice(completedIndex, 1);
    const successfulResults = results.filter((r) => r.success);
    const cancelledResults = results.filter((r) => r.cancelled);
    LogStatus(`Group ${group.groupNumber} completed: ${successfulResults.length}/${results.length} successful, ${cancelledResults.length} cancelled`);
   * Executes a single task with retry logic and timeout handling.
   * @param task - The execution task to process
   * @param executionOrder - Execution order for this task
   * @returns Promise<ExecutionTaskResult> - Result of the task execution
  private async executeTask(
    task: ExecutionTask,
    executionOrder?: number,
  ): Promise<ExecutionTaskResult> {
    // Attempt execution with retries
    for (let attempt = 0; attempt <= config.maxRetries; attempt++) {
        if (task.cancellationToken?.aborted) {
          LogStatus(`Task ${task.taskId} cancelled before attempt ${attempt + 1}`);
            executionTimeMS: endTime.getTime() - startTime.getTime(),
            endTime,
          // Apply retry delay
          await this.delay(config.retryDelayMS * attempt);
          LogStatus(`Retrying task ${task.taskId}, attempt ${attempt + 1}/${config.maxRetries + 1}`);
        const result = await this.executeSingleTask(params, task, config.taskTimeoutMS, parentPromptRunId, executionOrder, agentRunId);
        result.endTime = new Date();
        LogError(`Task ${task.taskId} attempt ${attempt + 1} failed: ${error.message}`);
        // Don't retry if this was the last attempt
        if (attempt === config.maxRetries) {
    // All attempts failed
   * Executes a single task without retry logic.
   * @param timeoutMS - Timeout for the execution in milliseconds
  private async executeSingleTask(params: AIPromptParams, task: ExecutionTask, timeoutMS: number, parentPromptRunId?: string, executionOrder?: number, agentRunId?: string): Promise<ExecutionTaskResult> {
    // TODO: This will need to integrate with AIPromptRunner's execution logic
    // For now, implementing a simplified version
    let childPromptRun: AIPromptRunEntityExtended | null = null;
      // Create child prompt run log if parent ID is provided
      if (parentPromptRunId) {
        childPromptRun = await this.createChildPromptRun(task, startTime, parentPromptRunId, executionOrder, agentRunId);
      // Create LLM instance using vendor-specific driver class if available
      const driverClass = task.vendorDriverClass || task.model.DriverClass;
      const apiName = task.vendorApiName || task.model.APIName;
      if (!driverClass) {
        throw new Error(`No driver class available for model ${task.model.Name}. Vendor selection may have failed.`);
      const apiKey = GetAIAPIKey(driverClass, params.apiKeys, params.verbose);
      const llm = MJGlobal.Instance.ClassFactory.CreateInstance<BaseLLM>(BaseLLM, driverClass, apiKey);
      const innerParams = new ChatParams();
      innerParams.model = apiName;
      innerParams.cancellationToken = task.cancellationToken;
      // Configure streaming if enabled in task
      if (task.streamingConfig?.enabled && task.streamingConfig.callbacks?.OnContent) {
        innerParams.streaming = true;
        innerParams.streamingCallbacks = {
          OnContent: task.streamingConfig.callbacks.OnContent,
          OnComplete: task.streamingConfig.callbacks.OnComplete,
          OnError: task.streamingConfig.callbacks.OnError,
      innerParams.messages = this.buildMessageArray(task.renderedPrompt, task.conversationMessages, task.templateMessageRole || 'system');
      // Apply model-specific parameters if available
      if (task.modelParameters) {
        Object.assign(params, task.modelParameters);
      // Execute with timeout and cancellation support
      const racePromises: Promise<ChatResult | never>[] = [llm.ChatCompletion(innerParams)];
      // Add timeout promise
      racePromises.push(new Promise<never>((_, reject) => setTimeout(() => reject(new Error('Task execution timeout')), timeoutMS)));
      // Add cancellation promise if cancellation token is available
      if (task.cancellationToken) {
        racePromises.push(
            if (task.cancellationToken!.aborted) {
              reject(new Error('Task execution cancelled'));
              task.cancellationToken!.addEventListener('abort', () => {
      const modelResult = (await Promise.race(racePromises)) as ChatResult;
      // Update child prompt run with results
      if (childPromptRun) {
        await this.updateChildPromptRun(childPromptRun, modelResult, endTime, executionTimeMS);
        success: modelResult.success,
        rawResult: modelResult.data?.choices?.[0]?.message?.content,
        tokensUsed: modelResult.data?.usage?.totalTokens,
        promptRun: childPromptRun, // Include the child prompt run in the result
      // Check if this was a cancellation error
      const isCancelled = error.message.includes('cancelled');
      // Update child prompt run with error if it exists
        childPromptRun.CompletedAt = endTime;
        childPromptRun.ExecutionTimeMS = executionTimeMS;
        childPromptRun.Success = false;
        childPromptRun.ErrorMessage = error.message;
        childPromptRun.Result = `ERROR: ${error.message}`;
        await childPromptRun.Save();
      // Return cancelled result instead of throwing if this was a cancellation
      if (isCancelled) {
          promptRun: childPromptRun,
      throw new Error(`Task execution failed: ${error.message}`);
   * Aggregates individual task results into a comprehensive parallel execution result.
   * @param taskResults - Array of all task execution results
   * @param startTime - Start time of the parallel execution
   * @param endTime - End time of the parallel execution
   * @returns ParallelExecutionResult - Aggregated results with metrics
  private aggregateResults(taskResults: ExecutionTaskResult[], startTime: Date, endTime: Date): ParallelExecutionResult {
    const groupResults = new Map<number, ExecutionTaskResult[]>();
    let totalTokensUsed = 0;
    let successCount = 0;
    let cancelledCount = 0;
    // Group results and calculate metrics
    for (const result of taskResults) {
      const groupNumber = result.task.executionGroup;
      if (!groupResults.has(groupNumber)) {
        groupResults.set(groupNumber, []);
      groupResults.get(groupNumber)!.push(result);
        successCount++;
        totalTokensUsed += result.tokensUsed || 0;
        if (result.cancelled) {
          cancelledCount++;
          errors.push(result.errorMessage);
      success: successCount > 0,
      taskResults,
      groupResults,
      totalExecutionTimeMS: endTime.getTime() - startTime.getTime(),
      successCount,
      failureCount: taskResults.length - successCount - cancelledCount,
      cancelledCount,
      totalTokensUsed,
   * Selects the first successful result from the array.
   * @param results - Array of successful results to choose from
   * @returns ExecutionTaskResult - The first result in the array
  private selectFirstResult(results: ExecutionTaskResult[]): ExecutionTaskResult {
   * Selects a random result from the array.
   * @returns ExecutionTaskResult - A randomly selected result
  private selectRandomResult(results: ExecutionTaskResult[]): ExecutionTaskResult {
    const randomIndex = Math.floor(Math.random() * results.length);
    return results[randomIndex];
   * Uses an AI prompt to select the best result from multiple options.
   * @param selectorPromptId - ID of the prompt to use for selection
   * @returns Promise<ExecutionTaskResult> - The AI-selected best result with all results ranked
  private async selectResultWithPrompt(
    selectorPromptId: string,
    _cancellationToken?: AbortSignal,
      // Import AIPromptRunner here to avoid circular dependency
      const { AIPromptRunner } = await import('./AIPromptRunner');
      // Load the judge prompt from AIEngine
      await AIEngine.Instance.Config(false);
      const judgePrompt = AIEngine.Instance.Prompts.find((p) => p.ID === selectorPromptId);
      if (!judgePrompt) {
        LogError(`Judge prompt with ID ${selectorPromptId} not found`);
        return results[0]; // Fallback to first result
      // Prepare the data for the judge prompt
      const judgeData = this.formatResultsForJudge(results);
      // Create conversation messages for the judge
      const conversationMessages: ChatMessage[] = [
          content:
            'You are an expert AI judge tasked with ranking multiple AI responses to determine the best one. Analyze each response for quality, accuracy, completeness, and relevance to the original prompt.',
          content: JSON.stringify(judgeData, null, 2),
      // Create a ResultSelector prompt run log entry if parent ID is provided
      let resultSelectorPromptRun: AIPromptRunEntityExtended | null = null;
        resultSelectorPromptRun = await this.createResultSelectorPromptRun(
          judgePrompt,
          judgeData,
          parentPromptRunId,
          results.length, // execution order after all parallel children
      // Execute the judge prompt
      const judgeRunner = new AIPromptRunner();
      const judgeStartTime = Date.now();
      const judgeResult = await judgeRunner.ExecutePrompt({
        prompt: judgePrompt,
        data: judgeData,
      const judgeEndTime = Date.now();
      const judgeExecutionTimeMS = judgeEndTime - judgeStartTime;
      // Update the result selector prompt run with the result
      if (resultSelectorPromptRun && judgeResult.promptRun) {
        resultSelectorPromptRun.CompletedAt = new Date(judgeEndTime);
        resultSelectorPromptRun.ExecutionTimeMS = judgeExecutionTimeMS;
        resultSelectorPromptRun.Success = judgeResult.success;
        resultSelectorPromptRun.Result = judgeResult.rawResult || '';
        if (judgeResult.tokensUsed) {
          resultSelectorPromptRun.TokensUsed = judgeResult.tokensUsed;
        await resultSelectorPromptRun.Save();
      if (!judgeResult.success || !judgeResult.rawResult) {
        LogError(`Judge prompt execution failed: ${judgeResult.errorMessage}`);
      // Parse the judge's decision
      const rankings = this.parseJudgeResult(judgeResult.rawResult);
      if (!rankings || rankings.length === 0) {
        LogError('Failed to parse judge rankings');
      // Apply rankings to results
      this.applyRankingsToResults(results, rankings);
      // Store judge metadata for later use
      const bestCandidateId = rankings.find((r) => r.rank === 1)?.candidateId;
      const bestResultIndex = results.findIndex((r) => r.task.taskId === bestCandidateId);
      const bestResult = bestResultIndex >= 0 ? results[bestResultIndex] : results[0];
      // Add judge metadata to the best result
      bestResult.judgeMetadata = {
        judgePromptId: selectorPromptId,
        judgeExecutionTimeMS,
        judgeTokensUsed: judgeResult.tokensUsed,
      LogStatus(`Judge selected candidate ${bestCandidateId} as the best result`);
      return bestResult;
      LogError(`Error in AI judge selection: ${error.message}`);
   * Formats execution results for the judge prompt.
   * @param results - Array of execution results to format
   * @returns Structured data for judge evaluation
  private formatResultsForJudge(results: ExecutionTaskResult[]): Record<string, unknown> {
      originalPrompt: results[0]?.task.renderedPrompt || 'Original prompt not available',
      candidates: results.map((result) => ({
        candidateId: result.task.taskId,
        vendorName: result.task.model.Vendor || 'Unknown',
        response: result.rawResult || '',
        tokensUsed: result.tokensUsed || 0,
      instructions: {
        task: 'Rank these AI responses from best to worst (1 = best)',
        format: "Return valid JSON with 'rankings' array containing objects with 'candidateId', 'rank', and 'rationale' fields",
        criteria: ['Quality', 'Accuracy', 'Completeness', 'Relevance', 'Clarity'],
   * Parses the judge's result to extract rankings.
   * @param judgeResult - Raw result from the judge prompt
   * @returns Array of ranking objects or null if parsing fails
  private parseJudgeResult(judgeResult: string): Array<{ candidateId: string; rank: number; rationale: string }> | null {
      // Try to extract JSON from the result (in case there's extra text)
      const jsonMatch = judgeResult.match(/\{[\s\S]*\}/);
      const jsonString = jsonMatch ? jsonMatch[0] : judgeResult;
      if (parsed.rankings && Array.isArray(parsed.rankings)) {
        return parsed.rankings.map((ranking: Record<string, unknown>) => ({
          candidateId: ranking.candidateId,
          rank: ranking.rank,
          rationale: ranking.rationale || 'No rationale provided',
      LogError('Judge result does not contain valid rankings array');
      LogError(`Failed to parse judge result: ${error.message}`);
   * Applies rankings to execution task results.
   * @param results - Array of execution results to rank
   * @param rankings - Rankings from the judge
  private applyRankingsToResults(results: ExecutionTaskResult[], rankings: Array<{ candidateId: string; rank: number; rationale: string }>): void {
      const ranking = rankings.find((r) => r.candidateId === result.task.taskId);
      if (ranking) {
        result.ranking = ranking.rank;
        result.judgeRationale = ranking.rationale;
   * Selects the result that appears most frequently (consensus).
   * @returns ExecutionTaskResult - The most common result
  private selectConsensusResult(results: ExecutionTaskResult[]): ExecutionTaskResult {
    // Group results by their content
    const resultGroups = new Map<string, ExecutionTaskResult[]>();
      const key = result.rawResult || '';
      if (!resultGroups.has(key)) {
        resultGroups.set(key, []);
      resultGroups.get(key)!.push(result);
    // Find the group with the most results
    let largestGroup: ExecutionTaskResult[] = [];
    for (const group of resultGroups.values()) {
      if (group.length > largestGroup.length) {
        largestGroup = group;
    // Return the first result from the largest group
    return largestGroup[0];
  private buildMessageArray(
    templateMessageRole: 'system' | 'user' | 'none' = 'system',
   * Creates a child AIPromptRun entity for individual parallel execution tracking.
   * @param task - The execution task
   * @param startTime - When the execution started
   * @param parentPromptRunId - ID of the parent prompt run
   * @param executionOrder - Execution order within the parallel group
   * @returns Promise<AIPromptRunEntityExtended> - The created child prompt run
  private async createChildPromptRun(task: ExecutionTask, startTime: Date, parentPromptRunId: string, executionOrder?: number, agentRunId?: string): Promise<AIPromptRunEntityExtended> {
      const promptRun = await this._metadata.GetEntityObject<AIPromptRunEntityExtended>('MJ: AI Prompt Runs', task.contextUser);
      promptRun.PromptID = task.prompt.ID;
      promptRun.ModelID = task.model.ID;
      promptRun.RunType = 'ParallelChild';
      promptRun.ParentID = parentPromptRunId;
      if (executionOrder !== undefined) {
        promptRun.ExecutionOrder = executionOrder;
      if (agentRunId) {
        promptRun.AgentRunID = agentRunId;
      // Set vendor ID from task if available (use task vendor ID which comes from vendor selection)
      if (task.vendorId) {
        promptRun.VendorID = task.vendorId;
      } else if (task.promptModel?.VendorID) {
        // Fallback to prompt model vendor if task vendor not set
        promptRun.VendorID = task.promptModel.VendorID;
      // Set configuration ID from task if available
      if (task.configurationId) {
        promptRun.ConfigurationID = task.configurationId;
      const messagesData = {
        renderedPrompt: task.renderedPrompt,
        conversationMessages: task.conversationMessages,
        modelParameters: task.modelParameters,
        taskId: task.taskId,
      promptRun.Messages = JSON.stringify(messagesData);
        const error = `Failed to save child AIPromptRun: ${promptRun.LatestResult?.Message || 'Unknown error'}`;
      LogError(`Error creating child prompt run record: ${error.message}`);
   * Updates a child AIPromptRun entity with execution results.
   * @param promptRun - The child prompt run entity to update
   * @param modelResult - The result from the AI model
   * @param endTime - When the execution completed
   * @param executionTimeMS - Total execution time in milliseconds
  private async updateChildPromptRun(promptRun: AIPromptRunEntityExtended, modelResult: ChatResult, endTime: Date, executionTimeMS: number): Promise<void> {
      promptRun.Success = modelResult.success;
      if (modelResult.success) {
        promptRun.Result = modelResult.data?.choices?.[0]?.message?.content || '';
        // Extract token usage if available
        promptRun.ErrorMessage = modelResult.errorMessage;
        promptRun.Result = `ERROR: ${modelResult.errorMessage}`;
        LogError(`Failed to update child AIPromptRun with results: ${promptRun.LatestResult?.Message || 'Unknown error'}`);
      LogError(`Error updating child prompt run: ${error.message}`);
   * Creates a ResultSelector AIPromptRun entity for judge execution tracking.
   * @param judgePrompt - The judge prompt being executed
   * @param judgeData - The data being sent to the judge
   * @returns Promise<AIPromptRunEntityExtended> - The created result selector prompt run
  private async createResultSelectorPromptRun(
    judgePrompt: AIPromptEntityExtended,
    judgeData: Record<string, unknown>,
    parentPromptRunId: string,
    executionOrder: number,
      const promptRun = await this._metadata.GetEntityObject<AIPromptRunEntityExtended>('MJ: AI Prompt Runs');
      promptRun.PromptID = judgePrompt.ID;
      // We don't have a specific model ID for the judge yet, it will be set by AIPromptRunner
      promptRun.RunAt = new Date();
      promptRun.RunType = 'ResultSelector';
      // Store the judge data as JSON in Messages field
        candidateCount: Array.isArray((judgeData as Record<string, unknown>).candidates)
          ? ((judgeData as Record<string, unknown>).candidates as unknown[]).length
        const error = `Failed to save ResultSelector AIPromptRun: ${promptRun.LatestResult?.Message || 'Unknown error'}`;
      LogError(`Error creating result selector prompt run record: ${error.message}`);
   * Creates a delay for the specified number of milliseconds.
   * @param ms - Number of milliseconds to delay
   * @returns Promise<void> - Promise that resolves after the delay
  private delay(ms: number): Promise<void> {
    return new Promise((resolve) => setTimeout(resolve, ms));
