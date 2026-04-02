import { Metadata, RunQuery } from '@memberjunction/core';
export interface AgentRunCostMetrics {
  totalPrompts: number;
  totalTokensInput: number;
  totalTokensOutput: number;
interface QueryResult {
  AgentRunID: string;
  TotalCost: number;
  TotalPrompts: number;
  TotalTokensInput: number;
  TotalTokensOutput: number;
export class AIAgentRunCostService {
  private costCache = new Map<string, { metrics: AgentRunCostMetrics; timestamp: number }>();
  private readonly CACHE_DURATION_MS = 30000; // 30 seconds
   * Get comprehensive cost metrics for an agent run including all nested sub-runs
   * Uses the high-performance CalculateAIAgentRunCost templated query
  async getAgentRunCostMetrics(agentRunId: string, useCache: boolean = true): Promise<AgentRunCostMetrics> {
    if (useCache) {
      const cached = this.getCachedMetrics(agentRunId);
    const metrics: AgentRunCostMetrics = {
      totalPrompts: 0,
      totalTokensInput: 0,
      totalTokensOutput: 0,
      // Use the high-performance templated query
      const queryResult = await rq.RunQuery({
        QueryName: 'CalculateRunCost',
          AIAgentRunID: agentRunId
      if (queryResult.Success && queryResult.Results && queryResult.Results.length > 0) {
        const result = queryResult.Results[0] as QueryResult;
        metrics.totalCost = result.TotalCost || 0;
        metrics.totalPrompts = result.TotalPrompts || 0;
        metrics.totalTokensInput = result.TotalTokensInput || 0;
        metrics.totalTokensOutput = result.TotalTokensOutput || 0;
        console.warn(`No cost data found for agent run ${agentRunId}:`, queryResult.ErrorMessage);
      metrics.isLoading = false;
      // Cache the results
        this.setCachedMetrics(agentRunId, metrics);
      console.error('Error calculating agent run cost metrics:', error);
        ...metrics,
        isLoading: false,
        error: 'Failed to calculate cost metrics'
   * Get just the total cost for display (simplified version)
  async getTotalCost(agentRunId: string, useCache: boolean = true): Promise<number> {
    const metrics = await this.getAgentRunCostMetrics(agentRunId, useCache);
    return metrics.totalCost;
   * Clear cache for specific agent run or all cached data
  clearCache(agentRunId?: string): void {
      this.costCache.delete(agentRunId);
      this.costCache.clear();
   * Get cached metrics if they exist and are still valid
  private getCachedMetrics(agentRunId: string): AgentRunCostMetrics | null {
    const cached = this.costCache.get(agentRunId);
    if (cached && (Date.now() - cached.timestamp) < this.CACHE_DURATION_MS) {
      return { ...cached.metrics }; // Return a copy
    // Remove stale cache entry
   * Cache metrics for future use
  private setCachedMetrics(agentRunId: string, metrics: AgentRunCostMetrics): void {
    this.costCache.set(agentRunId, {
      metrics: { ...metrics }, // Store a copy
      timestamp: Date.now()
