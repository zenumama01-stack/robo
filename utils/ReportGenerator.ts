 * Generates analysis reports
export class ReportGenerator {
  constructor(private stateManager: StateManager) {}
   * Generate analysis report
    lines.push('# Database Documentation Analysis Report');
    lines.push(`**Database**: ${state.database.name}`);
    const stats = this.calculateStatistics(state);
    lines.push('## Overall Statistics');
    lines.push(`- **Schemas**: ${stats.schemaCount}`);
    lines.push(`- **Tables**: ${stats.tableCount}`);
    lines.push(`- **Columns**: ${stats.columnCount}`);
    lines.push(`- **Total Iterations**: ${state.summary.totalIterations}`);
    lines.push(`- **Analysis Runs**: ${state.phases.descriptionGeneration.length}`);
    // Latest run details
      lines.push('## Latest Analysis Run');
      lines.push(`- **Run ID**: ${lastRun.runId}`);
      lines.push(`- **Started**: ${lastRun.startedAt}`);
      if (lastRun.completedAt) {
        lines.push(`- **Completed**: ${lastRun.completedAt}`);
      lines.push(`- **Model**: ${lastRun.modelUsed}`);
      lines.push(`- **Levels Processed**: ${lastRun.levelsProcessed}`);
      lines.push(`- **Backpropagations**: ${lastRun.backpropagationCount}`);
      lines.push(`- **Total Tokens**: ${lastRun.totalTokensUsed.toLocaleString()}`);
        lines.push(`**Convergence**: ${lastRun.convergenceReason}`);
      if (lastRun.warnings.length > 0) {
        lines.push('### Warnings');
        for (const warning of lastRun.warnings) {
          lines.push(`- ${warning}`);
      // Errors
      if (lastRun.errors.length > 0) {
        lines.push('### Errors');
        for (const error of lastRun.errors) {
          lines.push(`- ${error}`);
    // Confidence distribution
    const confidenceStats = this.calculateConfidenceStats(state);
    lines.push('## Confidence Distribution');
    lines.push(`- **Average Confidence**: ${(confidenceStats.average * 100).toFixed(1)}%`);
    lines.push(`- **High (>= 0.9)**: ${confidenceStats.high} tables`);
    lines.push(`- **Medium (0.7 - 0.9)**: ${confidenceStats.medium} tables`);
    lines.push(`- **Low (< 0.7)**: ${confidenceStats.low} tables`);
    const lowConfidence = this.stateManager.getLowConfidenceTables(state, 0.7);
      lines.push('### Low Confidence Tables');
      lines.push('| Schema | Table | Confidence | Description |');
      lines.push('|--------|-------|------------|-------------|');
      for (const item of lowConfidence) {
        const descPreview = item.description.substring(0, 100);
        lines.push(
          `| ${item.schema} | ${item.table} | ${(item.confidence * 100).toFixed(0)}% | ${descPreview}... |`
    const unprocessed = this.stateManager.getUnprocessedTables(state);
      lines.push('### Unprocessed Tables');
      lines.push('| Schema | Table |');
      lines.push('|--------|-------|');
      for (const item of unprocessed) {
        lines.push(`| ${item.schema} | ${item.table} |`);
    // Iteration history
    if (state.phases.descriptionGeneration.length > 1) {
      lines.push('## Iteration History');
      lines.push('| Run | Status | Iterations | Tokens | Cost | Converged |');
      lines.push('|-----|--------|------------|--------|------|-----------|');
      for (const run of state.phases.descriptionGeneration) {
          `| ${run.runId} | ${run.status} | ${run.iterationsPerformed} | ${run.totalTokensUsed.toLocaleString()} | $${run.estimatedCost.toFixed(2)} | ${run.converged ? 'Yes' : 'No'} |`
   * Calculate overall statistics
  private calculateStatistics(state: DatabaseDocumentation): {
    schemaCount: number;
    columnCount: number;
    let tableCount = 0;
      tableCount += schema.tables.length;
        columnCount += table.columns.length;
      schemaCount: state.schemas.length,
   * Calculate confidence statistics
  private calculateConfidenceStats(state: DatabaseDocumentation): {
    average: number;
    medium: number;
    let high = 0;
    let medium = 0;
    let low = 0;
            total += latest.confidence;
            if (latest.confidence >= 0.9) {
              high++;
            } else if (latest.confidence >= 0.7) {
              medium++;
              low++;
      average: count > 0 ? total / count : 0,
      high,
      medium,
      low
