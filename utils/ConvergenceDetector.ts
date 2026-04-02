 * Determines when analysis has converged
import { ConvergenceConfig } from '../types/config.js';
import { ConvergenceResult } from '../types/analysis.js';
export class ConvergenceDetector {
    private config: ConvergenceConfig,
   * Check if analysis has converged
  public hasConverged(
  ): ConvergenceResult {
    // 1. Check max iterations
    if (run.iterationsPerformed >= this.config.maxIterations) {
        reason: `Reached maximum iteration limit (${this.config.maxIterations})`,
        iterationsPerformed: run.iterationsPerformed,
        suggestions: ['Consider increasing maxIterations if results are unsatisfactory']
    // 2. Check stability window (no changes in last N iterations)
    if (run.iterationsPerformed >= 2) {
      const hasChanges = this.hasRecentChanges(run, this.config.stabilityWindow);
        reasons.push(
          `No changes in last ${this.config.stabilityWindow} iterations (stability achieved)`
    // 3. Check confidence threshold
    const lowConfidenceTables = this.stateManager.getLowConfidenceTables(
      this.config.confidenceThreshold
    if (lowConfidenceTables.length === 0 && run.iterationsPerformed >= 1) {
        `All tables meet confidence threshold (${this.config.confidenceThreshold})`
    // Converged if we have at least 2 reasons and minimum iterations
    if (reasons.length >= 2 && run.iterationsPerformed >= 2) {
        reason: reasons.join('; '),
        iterationsPerformed: run.iterationsPerformed
    // Not yet converged
    if (lowConfidenceTables.length > 0) {
      suggestions.push(
        `${lowConfidenceTables.length} tables below confidence threshold - needs more iteration`
    if (run.iterationsPerformed < 2) {
      suggestions.push('Minimum 2 iterations required before convergence check');
      reason: 'Analysis still evolving',
      suggestions
   * Check if there were changes in recent iterations
  private hasRecentChanges(run: AnalysisRun, windowSize: number): boolean {
    // Get log entries from last window
    const logsPerIteration = Math.ceil(run.processingLog.length / run.iterationsPerformed);
    const windowLogs = run.processingLog.slice(-(windowSize * logsPerIteration));
    return windowLogs.some(entry => entry.result === 'changed');
   * Calculate average confidence across all tables
  public calculateAverageConfidence(state: DatabaseDocumentation): number {
    let totalConfidence = 0;
        if (table.descriptionIterations.length > 0) {
          const latest = table.descriptionIterations[table.descriptionIterations.length - 1];
          if (latest.confidence !== undefined) {
            totalConfidence += latest.confidence;
    return count > 0 ? totalConfidence / count : 0;
