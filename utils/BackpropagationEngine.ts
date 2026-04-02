 * Handles backpropagation of insights from child tables to parent tables
import { DatabaseDocumentation, TableDefinition, AnalysisRun } from '../types/state.js';
import { BackpropagationTrigger } from '../types/analysis.js';
import { BackpropagationPromptResult } from '../types/prompts.js';
export class BackpropagationEngine {
    private iterationTracker: IterationTracker,
    private maxDepth: number
   * Execute backpropagation for a set of triggers
  public async execute(
  ): Promise<{ tablesUpdated: number; tokensUsed: number }> {
    let tablesUpdated = 0;
    // Group triggers by target table
    const triggersByTable = this.groupTriggersByTable(triggers);
    for (const [tableKey, tableTriggers] of triggersByTable) {
      const [schemaName, tableName] = tableKey.split('.');
      const table = this.stateManager.findTable(state, schemaName, tableName);
          `Backpropagation target table not found: ${tableKey}`
      // Skip if no current description
      if (!table.description) {
      // Get current description details
      const latestIteration = table.descriptionIterations[table.descriptionIterations.length - 1];
      // Combine insights from all triggers for this table
      const insights = tableTriggers.map(t => t.insight).join('\n\n');
      // Execute backpropagation prompt
      const result = await this.promptEngine.executePrompt<BackpropagationPromptResult>(
        'backpropagation',
          currentDescription: table.description,
          currentReasoning: latestIteration?.reasoning || '',
          currentConfidence: latestIteration?.confidence || 0,
        this.iterationTracker.addError(run, `Backpropagation failed for ${tableKey}: ${result.errorMessage}`);
      totalTokensUsed += result.tokensUsed;
      // Check if revision is needed
      if (result.result.needsRevision) {
          result.result.revisedDescription || result.result.reasoning,
          'backpropagation'
        tablesUpdated++;
          level: table.dependencyLevel || 0,
          schema: schemaName,
          table: tableName,
          action: 'backpropagate',
          result: 'changed',
          message: 'Description revised based on child table insights',
          tokensUsed: result.tokensUsed
          result: 'unchanged',
          message: 'No revision needed',
    if (tablesUpdated > 0) {
      this.iterationTracker.incrementBackpropagation(run);
    return { tablesUpdated, tokensUsed: totalTokensUsed };
   * Detect insights about parent tables from analysis result
   * Uses LLM-provided insights instead of NLP pattern matching
  public detectParentInsights(
    analysisResult: any,
    schema?: string,
    tableName?: string
  ): BackpropagationTrigger[] {
    // Current table full name for source
    const sourceTable = schema && tableName ? `${schema}.${tableName}` : 'unknown';
    // Use LLM-provided parent table insights (much more accurate than NLP)
    const parentInsights = analysisResult.parentTableInsights || [];
    for (const insight of parentInsights) {
      triggers.push({
        sourceTable,
        targetTable: insight.parentTable,
        insight: insight.insight,
        confidence: insight.confidence
   * Group triggers by target table
  private groupTriggersByTable(
  ): Map<string, BackpropagationTrigger[]> {
    const grouped = new Map<string, BackpropagationTrigger[]>();
    for (const trigger of triggers) {
      if (!grouped.has(trigger.targetTable)) {
        grouped.set(trigger.targetTable, []);
      grouped.get(trigger.targetTable)!.push(trigger);
