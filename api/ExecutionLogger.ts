 * @fileoverview Execution logging for MCP tool calls
 * Handles logging of all MCP tool executions to the database for
 * debugging, analytics, and audit purposes.
 * @module @memberjunction/ai-mcp-client/ExecutionLogger
import { MJMCPToolExecutionLogEntity } from '@memberjunction/core-entities';
import type { MCPLoggingConfig, MCPToolCallResult, MCPExecutionLogEntry } from './types.js';
 * ExecutionLogger handles logging of MCP tool calls to the database.
 * Features:
 * - Configurable logging per connection (inputs, outputs)
 * - Automatic output truncation for large responses
 * - Async logging to avoid blocking tool execution
 * - Error resilience (logging failures don't break tool calls)
 * const logger = new ExecutionLogger();
 * // Start logging before tool call
 * const logEntry = await logger.startLog(connectionId, toolName, params, contextUser);
 * // ... execute tool ...
 * // Complete log after tool call
 * await logger.completeLog(logEntry.id, result);
    /** Entity name for MCP Tool Execution Logs */
    private static readonly LOG_ENTITY_NAME = 'MJ: MCP Tool Execution Logs';
    /** Default max output log size (100KB) */
    private static readonly DEFAULT_MAX_OUTPUT_SIZE = 102400;
    /** Pending log writes for async completion */
    private readonly pendingLogs: Map<string, MCPExecutionLogEntry> = new Map();
     * Creates a new log entry at the start of tool execution
     * @param connectionId - MCP connection ID
     * @param toolId - MCP tool ID (if available)
     * @param toolName - Tool name
     * @param inputParams - Input parameters
     * @param config - Logging configuration
     * @returns Log entry ID for later completion
    async startLog(
        connectionId: string,
        toolId: string | undefined,
        toolName: string,
        inputParams: Record<string, unknown>,
        config: MCPLoggingConfig,
        // Skip if logging disabled
        if (!config.logToolCalls) {
            const logEntity = await md.GetEntityObject<MJMCPToolExecutionLogEntity>(
                ExecutionLogger.LOG_ENTITY_NAME,
            logEntity.NewRecord();
            logEntity.MCPServerConnectionID = connectionId;
            logEntity.MCPServerToolID = toolId ?? null;
            logEntity.ToolName = toolName;
            logEntity.UserID = contextUser.ID;
            logEntity.StartedAt = new Date();
            logEntity.Success = false; // Will be updated on completion
            logEntity.OutputTruncated = false;
            // Log input parameters if enabled
            if (config.logInputParameters) {
                logEntity.InputParameters = JSON.stringify(inputParams);
            const saved = await logEntity.Save();
                console.error('[MCPClient] Failed to create execution log entry:', logEntity.LatestResult?.Message);
            // Cache the entry for completion
            const entry: MCPExecutionLogEntry = {
                toolId,
                toolName,
                userId: contextUser.ID,
                startedAt: new Date(),
                outputTruncated: false
            this.pendingLogs.set(logEntity.ID, entry);
            return logEntity.ID;
            // Log error but don't fail the tool call
            console.error('[MCPClient] Error creating execution log:', error);
     * Completes a log entry after tool execution
     * @param logId - Log entry ID from startLog
     * @param result - Tool call result
    async completeLog(
        logId: string | null,
        result: MCPToolCallResult,
        if (!logId || !config.logToolCalls) {
            const loaded = await logEntity.Load(logId);
                console.error('[MCPClient] Failed to load execution log for completion:', logId);
            logEntity.EndedAt = new Date();
            logEntity.DurationMs = result.durationMs;
            logEntity.Success = result.success && !result.isToolError;
            // Set error message if failed
            if (!result.success || result.isToolError) {
                logEntity.ErrorMessage = result.error ?? 'Tool execution failed';
            // Log output content if enabled
            if (config.logOutputContent) {
                    structuredContent: result.structuredContent,
                    isToolError: result.isToolError
                const outputJson = JSON.stringify(outputData);
                const maxSize = config.maxOutputLogSize ?? ExecutionLogger.DEFAULT_MAX_OUTPUT_SIZE;
                if (outputJson.length > maxSize) {
                    logEntity.OutputContent = outputJson.substring(0, maxSize);
                    logEntity.OutputTruncated = true;
                    logEntity.OutputContent = outputJson;
                console.error('[MCPClient] Failed to complete execution log:', logEntity.LatestResult?.Message);
            // Remove from pending
            this.pendingLogs.delete(logId);
            console.error('[MCPClient] Error completing execution log:', error);
     * Marks a log entry as failed due to an error
     * @param logId - Log entry ID
     * @param error - Error that occurred
     * @param durationMs - Duration before failure
    async failLog(
        error: Error | string,
        durationMs: number,
        if (!logId) {
            logEntity.DurationMs = durationMs;
            logEntity.Success = false;
            logEntity.ErrorMessage = error instanceof Error ? error.message : String(error);
            await logEntity.Save();
        } catch (logError) {
            console.error('[MCPClient] Error failing execution log:', logError);
     * Gets recent execution logs for a connection
     * @param limit - Maximum number of logs to return
     * @returns Recent log entries
    async getRecentLogs(
        limit: number,
    ): Promise<MCPExecutionLogSummary[]> {
            const result = await rv.RunView<MCPExecutionLogSummary>({
                EntityName: ExecutionLogger.LOG_ENTITY_NAME,
                ExtraFilter: `MCPServerConnectionID = '${connectionId}'`,
                MaxRows: limit,
                Fields: ['ID', 'ToolName', 'StartedAt', 'EndedAt', 'DurationMs', 'Success', 'ErrorMessage']
                console.error('[MCPClient] Failed to get recent logs:', result.ErrorMessage);
            return result.Results ?? [];
            console.error('[MCPClient] Error getting recent logs:', error);
     * Gets execution statistics for a connection
     * @param sinceDays - Number of days to look back
     * @returns Execution statistics
    async getStats(
        sinceDays: number,
    ): Promise<MCPExecutionStats> {
        const stats: MCPExecutionStats = {
            totalCalls: 0,
            successfulCalls: 0,
            failedCalls: 0,
            averageDurationMs: 0,
            toolBreakdown: {}
            const sinceDate = new Date();
            sinceDate.setDate(sinceDate.getDate() - sinceDays);
            const sinceDateStr = sinceDate.toISOString();
            const result = await rv.RunView<MCPExecutionLogForStats>({
                ExtraFilter: `MCPServerConnectionID = '${connectionId}' AND StartedAt >= '${sinceDateStr}'`,
                Fields: ['ToolName', 'Success', 'DurationMs']
            const logs = result.Results;
            stats.totalCalls = logs.length;
            let totalDuration = 0;
            for (const log of logs) {
                if (log.Success) {
                    stats.successfulCalls++;
                    stats.failedCalls++;
                if (log.DurationMs) {
                    totalDuration += log.DurationMs;
                // Tool breakdown
                if (!stats.toolBreakdown[log.ToolName]) {
                    stats.toolBreakdown[log.ToolName] = {
                        calls: 0,
                        successes: 0,
                        failures: 0,
                        avgDurationMs: 0,
                        totalDurationMs: 0
                const toolStats = stats.toolBreakdown[log.ToolName];
                toolStats.calls++;
                    toolStats.successes++;
                    toolStats.failures++;
                    toolStats.totalDurationMs += log.DurationMs;
            if (stats.totalCalls > 0) {
                stats.averageDurationMs = Math.round(totalDuration / stats.totalCalls);
            for (const toolName in stats.toolBreakdown) {
                const toolStats = stats.toolBreakdown[toolName];
                if (toolStats.calls > 0) {
                    toolStats.avgDurationMs = Math.round(toolStats.totalDurationMs / toolStats.calls);
            console.error('[MCPClient] Error getting execution stats:', error);
     * Cleans up old log entries
     * @param connectionId - MCP connection ID (optional, cleans all if not specified)
     * @param olderThanDays - Delete logs older than this many days
     * @returns Number of deleted entries
    async cleanup(
        connectionId: string | undefined,
        olderThanDays: number,
    ): Promise<number> {
            const cutoffDate = new Date();
            cutoffDate.setDate(cutoffDate.getDate() - olderThanDays);
            const cutoffDateStr = cutoffDate.toISOString();
            let filter = `StartedAt < '${cutoffDateStr}'`;
                filter += ` AND MCPServerConnectionID = '${connectionId}'`;
            const result = await rv.RunView<{ ID: string }>({
                Fields: ['ID']
            let deleted = 0;
            for (const log of result.Results) {
                    const loaded = await logEntity.Load(log.ID);
                        const deleteResult = await logEntity.Delete();
                        if (deleteResult) {
                            deleted++;
                    // Continue with other deletions
            console.error('[MCPClient] Error cleaning up logs:', error);
 * Summary of an execution log entry (for listing)
export interface MCPExecutionLogSummary {
    ToolName: string;
    StartedAt: Date;
    EndedAt: Date | null;
    DurationMs: number | null;
    Success: boolean;
    ErrorMessage: string | null;
 * Execution log fields needed for stats calculation
interface MCPExecutionLogForStats {
 * Execution statistics for a connection
export interface MCPExecutionStats {
    /** Total number of calls */
    totalCalls: number;
    /** Number of successful calls */
    successfulCalls: number;
    /** Number of failed calls */
    failedCalls: number;
    /** Average duration in milliseconds */
    averageDurationMs: number;
    /** Breakdown by tool */
    toolBreakdown: Record<string, MCPToolStats>;
 * Statistics for a specific tool
export interface MCPToolStats {
    /** Number of calls */
    calls: number;
    /** Number of successes */
    successes: number;
    /** Number of failures */
    failures: number;
    avgDurationMs: number;
    /** Total duration for average calculation */
    totalDurationMs: number;
