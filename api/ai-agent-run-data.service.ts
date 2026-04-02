import { MJAIAgentRunEntity, MJAIAgentRunStepEntity, MJActionExecutionLogEntity, MJAIPromptRunEntity } from '@memberjunction/core-entities';
export interface AgentRunData {
  steps: MJAIAgentRunStepEntity[];
  subRuns: MJAIAgentRunEntity[];
  actionLogs: MJActionExecutionLogEntity[];
  promptRuns: MJAIPromptRunEntity[];
 * Helper class for managing AI Agent Run data per component instance
 * No longer a singleton Angular service - instantiated per component
export class AIAgentRunDataHelper {
  // Data subjects
  private stepsSubject$ = new BehaviorSubject<MJAIAgentRunStepEntity[]>([]);
  private subRunsSubject$ = new BehaviorSubject<MJAIAgentRunEntity[]>([]);
  private actionLogsSubject$ = new BehaviorSubject<MJActionExecutionLogEntity[]>([]);
  private promptRunsSubject$ = new BehaviorSubject<MJAIPromptRunEntity[]>([]);
  private loadingSubject$ = new BehaviorSubject<boolean>(false);
  private errorSubject$ = new BehaviorSubject<string | null>(null);
  steps$ = this.stepsSubject$.asObservable();
  subRuns$ = this.subRunsSubject$.asObservable();
  actionLogs$ = this.actionLogsSubject$.asObservable();
  promptRuns$ = this.promptRunsSubject$.asObservable();
  loading$ = this.loadingSubject$.asObservable();
  error$ = this.errorSubject$.asObservable();
  // Cache for sub-agent data with size limit
  private readonly MAX_CACHE_SIZE = 100; // Maximum 100 sub-agent entries
  private readonly CACHE_TTL_MS = 15 * 60 * 1000; // 15 minute TTL
  private subAgentDataCache = new Map<string, {
    timestamp: number;
  // Track cache access order for LRU eviction
  private cacheAccessOrder: string[] = [];
  private currentAgentRunId: string | null = null;
   * Load all data for an agent run
  async loadAgentRunData(agentRunId: string, forceReload = false): Promise<void> {
    if (!agentRunId) {
      this.errorSubject$.next('No agent run ID provided');
    // Skip cache check when force reloading
    if (!forceReload && this.currentAgentRunId === agentRunId && this.stepsSubject$.value.length > 0) {
    this.currentAgentRunId = agentRunId;
    this.loadingSubject$.next(true);
    this.errorSubject$.next(null);
    // Clear cache when loading new run
    this.subAgentDataCache.clear();
      await this.loadStepsAndSubRuns(agentRunId);
      this.errorSubject$.next('Failed to load agent run data');
      console.error('Error loading agent run data:', error);
      this.loadingSubject$.next(false);
  private async loadStepsAndSubRuns(agentRunId: string) {
    // First, get all steps to determine what additional data we need
      ExtraFilter: `AgentRunID='${agentRunId}'`,
      OrderBy: '__mj_CreatedAt, StepNumber'
      throw new Error('Failed to load agent run steps');
    const steps = stepsResult.Results as MJAIAgentRunStepEntity[] || [];
    // Build filters for batch loading
    const actionLogIds = steps
      .filter(s => s.StepType === 'Actions' && s.TargetLogID)
    const promptRunIds = steps
      .filter(s => s.StepType === 'Prompt' && s.TargetLogID)
    // Build batch queries array
    const batchQueries: any[] = [
      // Sub-runs query
        ExtraFilter: `ParentRunID='${agentRunId}'`,
        OrderBy: 'StartedAt'
      // Current run query
        ExtraFilter: `ID='${agentRunId}'`
    // Add action logs query if needed
      batchQueries.push({
        ExtraFilter: `ID IN ('${actionLogIds.join("','")}')`,
    // Add prompt runs query if needed
    if (promptRunIds.length > 0) {
        OrderBy: '__mj_CreatedAt'
    // Execute all queries in one batch
    const batchResults = await rv.RunViews(batchQueries);
    let resultIndex = 0;
    // Sub-runs
    const subRuns = batchResults[resultIndex].Success 
      ? (batchResults[resultIndex].Results as MJAIAgentRunEntity[] || [])
    resultIndex++;
    // Skip current run result
    // Action logs
    const actionLogs = actionLogIds.length > 0 && batchResults[resultIndex]?.Success
      ? (batchResults[resultIndex].Results as MJActionExecutionLogEntity[] || [])
    if (actionLogIds.length > 0) resultIndex++;
    // Prompt runs
    const promptRuns = promptRunIds.length > 0 && batchResults[resultIndex]?.Success
      ? (batchResults[resultIndex].Results as MJAIPromptRunEntity[] || [])
    // Update all subjects
    this.stepsSubject$.next(steps);
    this.subRunsSubject$.next(subRuns);
    this.actionLogsSubject$.next(actionLogs);
    this.promptRunsSubject$.next(promptRuns);
   * Load sub-agent data (for expanding sub-agent nodes)
  async loadSubAgentData(subAgentRunId: string): Promise<{ steps: MJAIAgentRunStepEntity[], promptRuns: MJAIPromptRunEntity[] }> {
    const cachedData = this.subAgentDataCache.get(subAgentRunId);
    if (cachedData) {
      // Check if cache is still valid
      if (now - cachedData.timestamp < this.CACHE_TTL_MS) {
        // Update access order for LRU
        this.updateCacheAccessOrder(subAgentRunId);
        return { steps: cachedData.steps, promptRuns: cachedData.promptRuns };
        // Cache expired, remove it
        this.removeCacheEntry(subAgentRunId);
    // Load steps first to determine what else we need
      ExtraFilter: `AgentRunID = '${subAgentRunId}'`,
    if (!stepsResult.Success || !stepsResult.Results) {
      return { steps: [], promptRuns: [] };
    const steps = stepsResult.Results;
    // Get prompt run IDs
    let promptRuns: MJAIPromptRunEntity[] = [];
    // Load prompt runs if needed
      const promptResult = await rv.RunView<MJAIPromptRunEntity>({
      if (promptResult.Success) {
        promptRuns = promptResult.Results || [];
    // Cache the data with timestamp
    const data = { steps, promptRuns, timestamp: Date.now() };
    // Enforce cache size limit
    if (this.subAgentDataCache.size >= this.MAX_CACHE_SIZE) {
      // Remove least recently used entry
      const lruKey = this.cacheAccessOrder[0];
      if (lruKey && lruKey !== subAgentRunId) {
        this.removeCacheEntry(lruKey);
    this.subAgentDataCache.set(subAgentRunId, data);
   * Clear all data
  clearData() {
    this.stepsSubject$.next([]);
    this.subRunsSubject$.next([]);
    this.actionLogsSubject$.next([]);
    this.promptRunsSubject$.next([]);
    this.clearCache();
    this.currentAgentRunId = null;
   * Clear just the cache for the current agent run
  clearCurrentRunCache() {
    // Clear all cache entries related to current run
    if (this.currentAgentRunId) {
      const keysToRemove: string[] = [];
      for (const [key, value] of this.subAgentDataCache.entries()) {
        // You might want to add logic here to identify related entries
        keysToRemove.push(key);
      keysToRemove.forEach(key => this.removeCacheEntry(key));
   * Clear entire cache
  private clearCache() {
    this.cacheAccessOrder = [];
   * Update cache access order for LRU eviction
  private updateCacheAccessOrder(key: string) {
    const index = this.cacheAccessOrder.indexOf(key);
      this.cacheAccessOrder.splice(index, 1);
    this.cacheAccessOrder.push(key);
   * Remove a cache entry
  private removeCacheEntry(key: string) {
    this.subAgentDataCache.delete(key);
   * Get cache statistics for monitoring
  getCacheStats() {
      size: this.subAgentDataCache.size,
      maxSize: this.MAX_CACHE_SIZE,
      ttlMs: this.CACHE_TTL_MS,
      accessOrder: [...this.cacheAccessOrder]
   * Get current data snapshot
  getCurrentData(): AgentRunData {
      steps: this.stepsSubject$.value,
      subRuns: this.subRunsSubject$.value,
      actionLogs: this.actionLogsSubject$.value,
      promptRuns: this.promptRunsSubject$.value
