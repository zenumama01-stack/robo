 * State file management
 * Handles loading, saving, and updating the state file
  DatabaseDocumentation,
  SchemaDefinition,
  TableDefinition,
  ColumnDefinition,
  DescriptionIteration,
  AnalysisRun
} from '../types/state.js';
export class StateManager {
  constructor(private stateFilePath: string) {}
   * Load state from file
  public async load(): Promise<DatabaseDocumentation | null> {
      const exists = await this.fileExists();
      const content = await fs.readFile(this.stateFilePath, 'utf-8');
      const state = JSON.parse(content) as DatabaseDocumentation;
      // Migrate old structure to new phases structure (backward compatibility)
      if (!state.phases) {
        state.phases = {
          descriptionGeneration: (state as any).analysisRuns || []
        if ((state as any).relationshipDiscoveryPhase) {
          state.phases.keyDetection = (state as any).relationshipDiscoveryPhase;
      // Initialize summary if it doesn't exist (backward compatibility)
      if (!state.summary) {
        state.summary = {
          createdAt: (state as any).createdAt || new Date().toISOString(),
          lastModified: (state as any).lastModified || new Date().toISOString(),
          totalIterations: (state as any).totalIterations || 0,
          totalColumns: 0,
        this.updateSummary(state);
        // Migrate timing fields into summary if they exist at top level
        if ((state as any).createdAt && !state.summary.createdAt) {
          state.summary.createdAt = (state as any).createdAt;
        if ((state as any).lastModified && !state.summary.lastModified) {
          state.summary.lastModified = (state as any).lastModified;
        if ((state as any).totalIterations !== undefined && !state.summary.totalIterations) {
          state.summary.totalIterations = (state as any).totalIterations;
      throw new Error(`Failed to load state file: ${(error as Error).message}`);
   * Save state to file
  public async save(state: DatabaseDocumentation): Promise<void> {
      // Update lastModified timestamp in summary
      state.summary.lastModified = new Date().toISOString();
      const dir = path.dirname(this.stateFilePath);
      await fs.mkdir(dir, { recursive: true });
      // Write state file
      const content = JSON.stringify(state, null, 2);
      await fs.writeFile(this.stateFilePath, content, 'utf-8');
      throw new Error(`Failed to save state file: ${(error as Error).message}`);
   * Create initial empty state
  public createInitialState(
    databaseName: string,
    serverName: string
  ): DatabaseDocumentation {
        lastModified: now,
        name: databaseName,
        server: serverName,
        analyzedAt: now
        descriptionGeneration: []
   * Start a new analysis run
  public createAnalysisRun(
    vendor: string,
    temperature: number,
    topP?: number,
    topK?: number
  ): AnalysisRun {
    const run: AnalysisRun = {
      runId: this.generateRunId(),
      modelUsed,
      vendor,
      topP,
    state.phases.descriptionGeneration.push(run);
   * Update table description with new iteration
  public updateTableDescription(
    confidence: number,
    triggeredBy: 'initial' | 'backpropagation' | 'refinement' | 'dependency_sanity_check' | 'schema_sanity_check' | 'cross_schema_sanity_check'
    const iteration: DescriptionIteration = {
      triggeredBy,
      changedFrom: table.description
    table.descriptionIterations.push(iteration);
    table.description = description;
   * Update column description with new iteration
  public updateColumnDescription(
    modelUsed: string
      modelUsed
    column.descriptionIterations.push(iteration);
    column.description = description;
   * Update schema description
  public updateSchemaDescription(
      triggeredBy: 'schema_sanity_check'
    schema.descriptionIterations.push(iteration);
    schema.description = description;
   * Find a table in the state
  public findTable(
  ): TableDefinition | null {
    if (!schema) {
    return schema.tables.find(t => t.name === tableName) || null;
   * Get all unapproved tables
  public getUnapprovedTables(state: DatabaseDocumentation): TableDefinition[] {
    const unapproved: TableDefinition[] = [];
        if (!table.userApproved) {
          unapproved.push(table);
    return unapproved;
   * Get low-confidence tables
  public getLowConfidenceTables(
  ): Array<{ schema: string; table: string; confidence: number; description: string; reasoning: string }> {
    const lowConfidence: Array<{
          const confidence = latest.confidence ?? 0;
          if (confidence < threshold) {
            lowConfidence.push({
              description: latest.description,
              reasoning: latest.reasoning
    return lowConfidence;
   * Get tables that need processing (no descriptions yet)
  public getUnprocessedTables(state: DatabaseDocumentation): Array<{ schema: string; table: string }> {
    const unprocessed: Array<{ schema: string; table: string }> = [];
        if (table.descriptionIterations.length === 0) {
          unprocessed.push({
    return unprocessed;
   * Check if state file exists
  private async fileExists(): Promise<boolean> {
      await fs.access(this.stateFilePath);
   * Generate unique run ID
  private generateRunId(): string {
    return `run_${Date.now()}_${Math.random().toString(36).substr(2, 9)}`;
   * Update summary statistics from current state and analysis runs
  public updateSummary(state: DatabaseDocumentation): void {
    // Count schemas, tables, columns
        totalColumns += table.columns.length;
    // Aggregate from all analysis runs
    let totalPromptsRun = 0;
    let estimatedCost = 0;
      // Count prompts from processing log
      totalPromptsRun += run.processingLog.filter(
        log => log.tokensUsed && log.tokensUsed > 0
      // Sum tokens and cost
      totalTokens += run.totalTokensUsed;
      estimatedCost += run.estimatedCost;
    // Update summary (preserve timing fields)
      ...state.summary,
      totalPromptsRun,
      totalInputTokens: 0,  // TODO: Will need to track separately when available
      totalOutputTokens: 0, // TODO: Will need to track separately when available
      totalSchemas: state.schemas.length,
      estimatedCost
   * Delete state file
  public async delete(): Promise<void> {
        await fs.unlink(this.stateFilePath);
      throw new Error(`Failed to delete state file: ${(error as Error).message}`);
