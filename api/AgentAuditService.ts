import { UserInfo, RunView, Metadata } from '@memberjunction/core';
  MJAIAgentRunEntity,
  MJAIAgentRunStepEntity
} from '@memberjunction/core-entities';
import { AuditFormatter, AuditOutputFormat } from '../lib/audit-formatter';
export interface ListRunsOptions {
  status: 'success' | 'failed' | 'running' | 'all';
  days: number;
  limit: number;
export interface StepDetailOptions {
  detailLevel: 'minimal' | 'standard' | 'detailed' | 'full';
  maxTokens: number;
export interface RunSummaryOptions {
  includeStepList: boolean;
export interface RunSummary {
  // Run metadata
  runId: string;
  agentName: string;
  agentId: string;
  startedAt: string;
  completedAt?: string;
  duration: number; // milliseconds
  // Performance metrics
  estimatedCost: number;
  stepCount: number;
  // Step list with identifiable information
  steps: Array<{
    stepNumber: number;
    stepId: string;      // UUID for MCP queries
    stepName: string;
    stepType: string;
    inputTokens?: number;
    outputTokens?: number;
  // Error summary
  hasErrors: boolean;
  errorCount: number;
  firstError?: {
export interface StepDetail {
  stepId: string;
  // Input/Output with smart truncation
  input: {
    raw: string;
    truncated: boolean;
    tokenCount: number;
    preview?: string; // First N chars
  output: {
    preview?: string;
  // Tokens and cost
  cost?: number;
  // Error details
  stackTrace?: string;
export interface ErrorAnalysis {
  failedSteps: Array<{
    errorMessage: string;
    // Context: step before failure
    previousStep?: {
      outputPreview: string; // First 500 chars
  // Pattern detection
  errorPattern?: string;
  suggestedFixes: string[];
 * Service for auditing and analyzing AI Agent Run executions
export class AgentAuditService {
  private analyzer: AuditAnalyzer;
  private formatter: AuditFormatter;
    this.analyzer = new AuditAnalyzer();
    this.formatter = new AuditFormatter();
   * List recent agent runs with filtering
  async listRecentRuns(options: ListRunsOptions): Promise<MJAIAgentRunEntity[]> {
    startDate.setDate(startDate.getDate() - options.days);
    let filter = `StartedAt >= '${startDate.toISOString()}' AND StartedAt <= '${endDate.toISOString()}'`;
    if (options.agentName) {
      filter += ` AND Agent LIKE '%${options.agentName.replace(/'/g, "''")}%'`;
    if (options.status !== 'all') {
      const statusMap: Record<string, string> = {
        success: 'Success',
        failed: 'Failed',
        running: 'Running',
      filter += ` AND Status = '${statusMap[options.status]}'`;
    const result = await rv.RunView<MJAIAgentRunEntity>({
      ExtraFilter: filter,
      OrderBy: 'StartedAt DESC',
      MaxRows: options.limit,
      ResultType: 'simple',
      throw new Error(`Failed to load agent runs: ${result.ErrorMessage}`);
    return result.Results || [];
   * Get high-level summary of a run with step list
  async getRunSummary(runId: string, options: RunSummaryOptions): Promise<RunSummary> {
    // Load run entity
    const runEntity = await md.GetEntityObject<MJAIAgentRunEntity>('MJ: AI Agent Runs', this.contextUser);
    const loaded = await runEntity.Load(runId);
      throw new Error(`Agent run not found: ${runId}`);
    // Load all steps for this run
    const stepsResult = await rv.RunView<MJAIAgentRunStepEntity>({
      EntityName: 'MJ: AI Agent Run Steps',
      ExtraFilter: `AgentRunID = '${runId}'`,
      OrderBy: 'StepNumber',
    if (!stepsResult.Success) {
      throw new Error(`Failed to load steps: ${stepsResult.ErrorMessage}`);
    const steps = stepsResult.Results || [];
    // Calculate metrics - note: token counts are at the run level, not step level
    const totalTokens = runEntity.TotalTokensUsed || 0;
    const duration = runEntity.StartedAt && runEntity.CompletedAt
      ? new Date(runEntity.CompletedAt).getTime() - new Date(runEntity.StartedAt).getTime()
    const errorSteps = steps.filter(s => s.Status === 'Failed' || s.ErrorMessage);
    const firstError = errorSteps.length > 0 ? errorSteps[0] : undefined;
    // Build summary
    const summary: RunSummary = {
      runId: runEntity.ID!,
      agentName: runEntity.Agent || 'Unknown',
      agentId: runEntity.AgentID!,
      status: runEntity.Status || 'Unknown',
      startedAt: runEntity.StartedAt?.toISOString() || '',
      completedAt: runEntity.CompletedAt?.toISOString(),
      estimatedCost: this.analyzer.estimateCost(totalTokens),
      stepCount: steps.length,
      hasErrors: errorSteps.length > 0,
      errorCount: errorSteps.length,
      steps: steps.map((step, index) => ({
        stepNumber: index + 1, // 1-based for user display
        stepId: step.ID!,
        stepName: step.StepName || `Step ${index + 1}`,
        stepType: step.StepType || 'Unknown',
        status: step.Status || 'Unknown',
        duration: this.analyzer.calculateStepDuration(step),
        inputTokens: undefined, // Token counts are not tracked per step
        outputTokens: undefined, // Token counts are not tracked per step
        errorMessage: step.ErrorMessage || undefined,
    if (firstError) {
      summary.firstError = {
        stepNumber: steps.indexOf(firstError) + 1,
        stepName: firstError.StepName || 'Unknown',
        message: firstError.ErrorMessage || 'No error message',
    return summary;
   * Get detailed information for a specific step
  async getStepDetail(runId: string, stepNumber: number, options: StepDetailOptions): Promise<StepDetail> {
    // Load all steps to find the right one by sequence
    const result = await rv.RunView<MJAIAgentRunStepEntity>({
      throw new Error(`Failed to load steps: ${result.ErrorMessage}`);
    const steps = result.Results || [];
    if (stepNumber < 1 || stepNumber > steps.length) {
      throw new Error(`Invalid step number: ${stepNumber} (run has ${steps.length} steps)`);
    const step = steps[stepNumber - 1]; // Convert to 0-based index
    // Parse input/output JSON
    const inputRaw = step.InputData || '{}';
    const outputRaw = step.OutputData || '{}';
    const inputTokenCount = this.analyzer.estimateTokenCount(inputRaw);
    const outputTokenCount = this.analyzer.estimateTokenCount(outputRaw);
    // Apply truncation based on detail level and maxTokens
    const truncationRules = this.analyzer.getTruncationRules(options.detailLevel, options.maxTokens);
    const detail: StepDetail = {
      stepNumber,
      stepName: step.StepName || `Step ${stepNumber}`,
      startedAt: step.StartedAt?.toISOString() || '',
      completedAt: step.CompletedAt?.toISOString(),
        raw: this.analyzer.truncateField(inputRaw, truncationRules.inputMaxChars),
        truncated: inputRaw.length > truncationRules.inputMaxChars,
        tokenCount: inputTokenCount,
        preview: inputRaw.substring(0, 500),
        raw: this.analyzer.truncateField(outputRaw, truncationRules.outputMaxChars),
        truncated: outputRaw.length > truncationRules.outputMaxChars,
        tokenCount: outputTokenCount,
        preview: outputRaw.substring(0, 500),
      inputTokens: undefined, // Token counts not tracked at step level
      outputTokens: undefined, // Token counts not tracked at step level
      cost: undefined, // Cannot calculate cost without token counts
      stackTrace: undefined, // Stack trace not available in entity
    return detail;
   * Analyze all errors in a run with context
  async analyzeErrors(runId: string): Promise<ErrorAnalysis> {
    const summary = await this.getRunSummary(runId, { includeStepList: true, maxTokens: 500 });
    const failedSteps = summary.steps.filter(s => s.status === 'Failed' || s.errorMessage);
    const failedStepDetails = await Promise.all(
      failedSteps.map(async (step) => {
        const detail = await this.getStepDetail(runId, step.stepNumber, {
          detailLevel: 'standard',
          maxTokens: 1000,
        // Get previous step context
        let previousStep;
        if (step.stepNumber > 1) {
          const prevDetail = await this.getStepDetail(runId, step.stepNumber - 1, {
            detailLevel: 'minimal',
            maxTokens: 500,
          previousStep = {
            stepNumber: step.stepNumber - 1,
            stepName: prevDetail.stepName,
            status: prevDetail.status,
            outputPreview: prevDetail.output.preview || '',
          stepNumber: step.stepNumber,
          stepName: step.stepName,
          stepType: step.stepType,
          errorMessage: detail.errorMessage || 'Unknown error',
          stackTrace: detail.stackTrace,
          previousStep,
    // Detect error patterns
    const errorPattern = this.analyzer.detectErrorPattern(failedStepDetails.map(s => s.errorMessage));
    const suggestedFixes = this.analyzer.suggestFixes(errorPattern, failedStepDetails);
      runId,
      agentName: summary.agentName,
      errorCount: failedSteps.length,
      failedSteps: failedStepDetails,
      errorPattern,
      suggestedFixes,
   * Export full run data to file (no truncation)
  async exportRun(runId: string, exportType: 'full' | 'summary' | 'steps'): Promise<RunSummary | StepDetail[] | { summary: RunSummary; steps: StepDetail[] }> {
    const summary = await this.getRunSummary(runId, { includeStepList: true, maxTokens: 0 });
    if (exportType === 'summary') {
    // Load full step details
    const stepDetails = await Promise.all(
      summary.steps.map(step =>
        this.getStepDetail(runId, step.stepNumber, {
          detailLevel: 'full',
          maxTokens: 0, // No truncation for export
    if (exportType === 'steps') {
      return stepDetails;
    // Full export
      summary,
      steps: stepDetails,
   * Format run list for display
    return this.formatter.formatRunList(runs, format);
   * Format run summary for display
    return this.formatter.formatRunSummary(summary, format);
   * Format step detail for display
    return this.formatter.formatStepDetail(detail, format);
   * Format error analysis for display
    return this.formatter.formatErrorAnalysis(analysis, format);
    if (!UserCache.Users || UserCache.Users.length === 0) {
      throw new Error('No users found in UserCache');
    return UserCache.Users[0];
