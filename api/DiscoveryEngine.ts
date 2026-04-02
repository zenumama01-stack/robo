 * Discovery Engine
 * Orchestrates relationship discovery with iterative refinement and backpropagation
import { PKDetector } from './PKDetector.js';
import { FKDetector } from './FKDetector.js';
import { LLMDiscoveryValidator } from './LLMDiscoveryValidator.js';
import { LLMSanityChecker } from './LLMSanityChecker.js';
import { ColumnStatsCache } from './ColumnStatsCache.js';
import { SchemaDefinition, DatabaseDocumentation } from '../types/state.js';
  RelationshipDiscoveryPhase,
  RelationshipDiscoveryIteration,
  PKCandidate,
  FKCandidate,
  DiscoveryTriggerAnalysis
} from '../types/discovery.js';
import { RelationshipDiscoveryConfig, AIConfig } from '../types/config.js';
export interface DiscoveryEngineOptions {
  driver: BaseAutoDocDriver;
  config: RelationshipDiscoveryConfig;
  aiConfig: AIConfig;
  schemas: SchemaDefinition[];
export interface DiscoveryResult {
  phase: RelationshipDiscoveryPhase;
  guardrailsReached: boolean;
  guardrailReason?: string;
  statsCache: ColumnStatsCache; // Return the stats cache for persistence
export class DiscoveryEngine {
  private config: RelationshipDiscoveryConfig;
  private aiConfig: AIConfig;
  private schemas: SchemaDefinition[];
  private statsCache: ColumnStatsCache;
  private pkDetector: PKDetector;
  private fkDetector: FKDetector;
  private llmValidator?: LLMDiscoveryValidator;
  private sanityChecker?: LLMSanityChecker;
  constructor(options: DiscoveryEngineOptions) {
    this.driver = options.driver;
    this.aiConfig = options.aiConfig;
    this.schemas = options.schemas;
    // Create stats cache and detectors
    this.statsCache = new ColumnStatsCache();
    this.pkDetector = new PKDetector(this.driver, this.config, this.statsCache);
    this.fkDetector = new FKDetector(this.driver, this.config);
    // Create LLM sanity checker (runs once after statistical detection)
    this.sanityChecker = new LLMSanityChecker(this.aiConfig);
    // Create LLM validator if enabled (runs per-table validation)
    if (this.config.llmValidation?.enabled) {
      this.llmValidator = new LLMDiscoveryValidator(
        this.driver,
        this.config,
        this.aiConfig,
        this.statsCache,
        this.schemas
   * Get the column statistics cache
  public getStatsCache(): ColumnStatsCache {
    return this.statsCache;
   * Analyze if discovery should be triggered based on schema state
  public analyzeTrigger(): DiscoveryTriggerAnalysis {
    let totalTables = 0;
    let tablesWithPK = 0;
    for (const schema of this.schemas) {
      totalTables += schema.tables.length;
        // Check if table has PK
        const hasPK = table.columns.some(col => col.isPrimaryKey);
        if (hasPK) {
          tablesWithPK++;
        // Count FKs
        totalFKs += table.columns.filter(col => col.isForeignKey).length;
    const tablesWithoutPK = totalTables - tablesWithPK;
    // Heuristic: expect at least 0.3 FKs per table on average
    const expectedMinFKs = Math.floor(totalTables * 0.3);
    const fkDeficit = Math.max(0, expectedMinFKs - totalFKs);
    const fkDeficitPercentage = expectedMinFKs > 0 ? fkDeficit / expectedMinFKs : 0;
    // Determine if discovery should run
    let shouldRun = false;
    let reason = '';
    if (tablesWithoutPK > 0 && this.config.triggers.runOnMissingPKs) {
      shouldRun = true;
      reason = `${tablesWithoutPK} table(s) missing primary keys`;
    if (fkDeficitPercentage >= this.config.triggers.fkDeficitThreshold &&
        this.config.triggers.runOnInsufficientFKs) {
      if (reason) {
        reason += ` and FK deficit of ${(fkDeficitPercentage * 100).toFixed(1)}%`;
        reason = `FK deficit of ${(fkDeficitPercentage * 100).toFixed(1)}% (${totalFKs}/${expectedMinFKs})`;
      shouldRun,
      reason: shouldRun ? reason : 'Discovery not needed - schema is well-defined',
        totalTables,
        tablesWithPK,
        tablesWithoutPK,
        totalFKs,
        expectedMinFKs,
        fkDeficit,
        fkDeficitPercentage
   * Execute the discovery process
  public async discover(
    maxTokens: number,
    triggerAnalysis: DiscoveryTriggerAnalysis
  ): Promise<DiscoveryResult> {
    const phase: RelationshipDiscoveryPhase = this.initializePhase(maxTokens, triggerAnalysis);
    this.onProgress('Starting relationship discovery', {
      maxTokens,
      trigger: triggerAnalysis.reason
    let iteration = 1;
    let guardrailsReached = false;
    let guardrailReason: string | undefined;
    while (iteration <= this.config.backpropagation.maxIterations) {
      // Check guardrails before starting iteration
      const guardrailCheck = this.checkGuardrails(tokensUsed, maxTokens, iteration);
      if (!guardrailCheck.canContinue) {
        guardrailsReached = true;
        guardrailReason = guardrailCheck.reason;
        this.onProgress('Guardrails reached', { reason: guardrailReason });
      this.onProgress(`Discovery iteration ${iteration}`, { tokensUsed, maxTokens });
      // Execute iteration
      const iterationResult = await this.executeIteration(
        iteration,
        maxTokens - tokensUsed
      phase.iterations.push(iterationResult);
      tokensUsed += iterationResult.tokensUsed;
      phase.tokenBudget.used = tokensUsed;
      phase.tokenBudget.remaining = maxTokens - tokensUsed;
      // Update phase discoveries
      this.mergeDiscoveries(phase, iterationResult);
      this.onProgress(`Iteration ${iteration} complete`, {
        newPKs: iterationResult.discoveries.newPKs.length,
        newFKs: iterationResult.discoveries.newFKs.length,
        tokensUsed: iterationResult.tokensUsed
      // Check if we should continue
      const shouldContinue = this.shouldContinueDiscovery(iterationResult, phase, iteration);
      if (!shouldContinue) {
        this.onProgress('Discovery converged - no significant changes detected');
    // Finalize phase
    phase.completedAt = new Date().toISOString();
    this.calculateSummary(phase);
    this.onProgress('Discovery complete', {
      iterations: phase.iterations.length,
      tokensUsed,
      pksDiscovered: phase.discovered.primaryKeys.length,
      fksDiscovered: phase.discovered.foreignKeys.length
      guardrailsReached,
      guardrailReason,
      statsCache: this.statsCache
   * Initialize discovery phase
  private initializePhase(
  ): RelationshipDiscoveryPhase {
    const triggerReason = this.determineTriggerReason(triggerAnalysis);
      triggered: true,
      triggerReason,
      triggerDetails: {
        expectedFKs: triggerAnalysis.details.expectedMinFKs,
        fkDeficitPercentage: triggerAnalysis.details.fkDeficitPercentage
        allocated: maxTokens,
        used: 0,
        remaining: maxTokens
      iterations: [],
      discovered: {
        primaryKeys: [],
        foreignKeys: []
      schemaEnhancements: {
        pkeysAdded: 0,
        fkeysAdded: 0,
        overallConfidence: 0
      feedbackFromAnalysis: [],
        totalTablesAnalyzed: 0,
        tablesWithDiscoveredPKs: 0,
        relationshipsDiscovered: 0,
        averageConfidence: 0,
        highConfidenceCount: 0,
        mediumConfidenceCount: 0,
        lowConfidenceCount: 0,
        rejectedCount: 0
   * Determine trigger reason from analysis
  private determineTriggerReason(
    analysis: DiscoveryTriggerAnalysis
  ): 'missing_pks' | 'insufficient_fks' | 'both' | 'manual' {
    const hasMissingPKs = analysis.details.tablesWithoutPK > 0;
    const hasInsufficientFKs = analysis.details.fkDeficitPercentage >= this.config.triggers.fkDeficitThreshold;
    if (hasMissingPKs && hasInsufficientFKs) {
      return 'both';
    } else if (hasMissingPKs) {
      return 'missing_pks';
    } else if (hasInsufficientFKs) {
      return 'insufficient_fks';
      return 'manual';
   * Execute a single discovery iteration
    iteration: number,
    phase: RelationshipDiscoveryPhase,
    remainingTokens: number
  ): Promise<RelationshipDiscoveryIteration> {
    const startedAt = new Date().toISOString();
    const iterationResult: RelationshipDiscoveryIteration = {
      phase: 'sampling',
      startedAt,
      completedAt: '',
      discoveries: {
        newPKs: [],
        newFKs: [],
        validated: [],
        rejected: [],
        confidenceChanges: []
      backpropTriggered: false
    // Phase 1: PK Detection
    iterationResult.phase = 'pk_detection';
    let newPKs = await this.detectPrimaryKeys(iteration, phase);
    iterationResult.discoveries.newPKs = newPKs;
    this.onProgress(`Detected ${newPKs.length} PK candidates`, {
      candidates: newPKs.map(pk => `${pk.schemaName}.${pk.tableName}.${pk.columnNames.join('+')}`).slice(0, 5)
    // Phase 2: FK Detection
    iterationResult.phase = 'fk_detection';
    const allPKs = [...phase.discovered.primaryKeys, ...newPKs];
    let newFKs = await this.detectForeignKeys(iteration, phase, allPKs);
    this.onProgress(`Detected ${newFKs.length} FK candidates (pre-sanity)`, {
      candidates: newFKs.map(fk =>
        `${fk.schemaName}.${fk.sourceTable}.${fk.sourceColumn} -> ${fk.targetSchema}.${fk.targetTable}.${fk.targetColumn}`
      ).slice(0, 5)
    // Phase 2.5: LLM Sanity Check (FIRST TIME ONLY - reject obviously wrong candidates)
    if (this.sanityChecker && iteration === 1 && (newPKs.length > 0 || newFKs.length > 0)) {
      iterationResult.phase = 'sanity_check';
      this.onProgress('Running LLM sanity check for obvious errors', {
        pkCandidates: newPKs.length,
        fkCandidates: newFKs.length
      const sanityResult = await this.sanityChecker.reviewCandidates(newPKs, newFKs);
      iterationResult.tokensUsed += sanityResult.tokensUsed;
      // Remove invalid PKs
      if (sanityResult.invalidPKs.length > 0) {
        console.log(`[DiscoveryEngine] Sanity check rejecting ${sanityResult.invalidPKs.length} PKs:`);
        for (const invalid of sanityResult.invalidPKs) {
          console.log(`  - ${invalid.schema}.${invalid.table}.${invalid.column}: ${invalid.reason}`);
        newPKs = newPKs.filter(pk => {
          return !sanityResult.invalidPKs.some(invalid =>
            invalid.schema === pk.schemaName &&
            invalid.table === pk.tableName &&
            pk.columnNames.includes(invalid.column)
      // Remove invalid FKs
      if (sanityResult.invalidFKs.length > 0) {
        console.log(`[DiscoveryEngine] Sanity check rejecting ${sanityResult.invalidFKs.length} FKs:`);
        for (const invalid of sanityResult.invalidFKs) {
        newFKs = newFKs.filter(fk => {
          return !sanityResult.invalidFKs.some(invalid =>
            invalid.schema === fk.schemaName &&
            invalid.table === fk.sourceTable &&
            invalid.column === fk.sourceColumn
      // Log suggestions
      if (sanityResult.suggestions.length > 0) {
        console.log(`[DiscoveryEngine] Sanity check suggestions:`);
        for (const suggestion of sanityResult.suggestions) {
          console.log(`  - ${suggestion}`);
      this.onProgress(`Sanity check complete`, {
        invalidPKs: sanityResult.invalidPKs.length,
        invalidFKs: sanityResult.invalidFKs.length,
        finalPKs: newPKs.length,
        finalFKs: newFKs.length,
        tokensUsed: sanityResult.tokensUsed
      // Update discoveries
      iterationResult.discoveries.newFKs = newFKs;
    // Phase 3: LLM Validation (if enabled and we have candidates or need validation)
    if (this.llmValidator && (newPKs.length > 0 || newFKs.length > 0)) {
      iterationResult.phase = 'llm_validation';
      this.onProgress('Starting LLM validation', {
        fkCandidates: newFKs.length,
        tokensRemaining: remainingTokens - iterationResult.tokensUsed
      const validationResults = await this.performLLMValidation(
        newPKs,
        newFKs,
        allPKs,
        iterationResult,
        remainingTokens - iterationResult.tokensUsed
      // Update candidates based on LLM feedback
      newPKs = validationResults.updatedPKs;
      newFKs = validationResults.updatedFKs;
      iterationResult.tokensUsed += validationResults.tokensUsed;
      this.onProgress(`LLM validation complete`, {
        tokensUsed: validationResults.tokensUsed,
        validated: validationResults.validated.length,
        rejected: validationResults.rejected.length,
        finalFKs: newFKs.length
      iterationResult.discoveries.validated = validationResults.validated;
      iterationResult.discoveries.rejected = validationResults.rejected;
    // Phase 4: Backpropagation check
    iterationResult.phase = 'backprop';
    const backpropCheck = this.shouldTriggerBackprop(iterationResult, phase);
    iterationResult.backpropTriggered = backpropCheck.shouldTrigger;
    iterationResult.backpropReason = backpropCheck.reason;
    if (backpropCheck.shouldTrigger && this.config.backpropagation.enabled) {
      this.onProgress('Backpropagation triggered', { reason: backpropCheck.reason });
      // Re-analyze affected tables with new relationship context
      const confidenceChanges = await this.performBackpropagation(
        newFKs
      iterationResult.discoveries.confidenceChanges = confidenceChanges;
    iterationResult.completedAt = new Date().toISOString();
    return iterationResult;
   * Detect primary keys in all tables without PKs
  private async detectPrimaryKeys(
    phase: RelationshipDiscoveryPhase
  ): Promise<PKCandidate[]> {
    const allPKs: PKCandidate[] = [];
    const tablesAnalyzed = new Set<string>();
        const tableKey = `${schema.name}.${table.name}`;
        // Skip if already has PK or already analyzed
        const hasExistingPK = table.columns.some(col => col.isPrimaryKey);
        const hasDiscoveredPK = phase.discovered.primaryKeys.some(
          pk => pk.schemaName === schema.name && pk.tableName === table.name
        if (hasExistingPK || hasDiscoveredPK || tablesAnalyzed.has(tableKey)) {
        // Detect PK candidates for this table
        const candidates = await this.pkDetector.detectPKCandidates(
          schema.name,
          iteration
        allPKs.push(...candidates);
        tablesAnalyzed.add(tableKey);
    return allPKs;
   * Detect foreign keys across all schemas
  private async detectForeignKeys(
    discoveredPKs: PKCandidate[]
  ): Promise<FKCandidate[]> {
    const allFKs: FKCandidate[] = [];
        // Detect FK candidates for this table
        const candidates = await this.fkDetector.detectFKCandidates(
          this.schemas,
          discoveredPKs,
        // Filter out duplicates (already discovered FKs)
        const newCandidates = candidates.filter(newFK =>
          !phase.discovered.foreignKeys.some(existingFK =>
            existingFK.schemaName === newFK.schemaName &&
            existingFK.sourceTable === newFK.sourceTable &&
            existingFK.sourceColumn === newFK.sourceColumn &&
            existingFK.targetSchema === newFK.targetSchema &&
            existingFK.targetTable === newFK.targetTable &&
            existingFK.targetColumn === newFK.targetColumn
        allFKs.push(...newCandidates);
    return allFKs;
   * Perform LLM validation on PK/FK candidates
  private async performLLMValidation(
    pkCandidates: PKCandidate[],
    fkCandidates: FKCandidate[],
    allPKs: PKCandidate[],
    iteration: RelationshipDiscoveryIteration,
    updatedPKs: PKCandidate[];
    updatedFKs: FKCandidate[];
    validated: string[];
    rejected: string[];
    tokensUsed: number;
    if (!this.llmValidator) {
        updatedPKs: pkCandidates,
        updatedFKs: fkCandidates,
        tokensUsed: 0
    const updatedPKs = [...pkCandidates];
    const updatedFKs = [...fkCandidates];
    const validated: string[] = [];
    const rejected: string[] = [];
    // Group candidates by table for efficient validation
    const tableMap = new Map<string, { pks: PKCandidate[]; fks: FKCandidate[] }>();
    for (const pk of pkCandidates) {
      const key = `${pk.schemaName}.${pk.tableName}`;
      if (!tableMap.has(key)) {
        tableMap.set(key, { pks: [], fks: [] });
      tableMap.get(key)!.pks.push(pk);
    for (const fk of fkCandidates) {
      const key = `${fk.schemaName}.${fk.sourceTable}`;
      tableMap.get(key)!.fks.push(fk);
    // Validate each table's candidates
    for (const [tableKey, candidates] of tableMap.entries()) {
      if (tokensUsed >= remainingTokens) {
        this.onProgress('Token budget exhausted during LLM validation');
      this.onProgress(`Validating ${tableKey}`, {
        pks: candidates.pks.length,
        fks: candidates.fks.length
        const result = await this.llmValidator.validateTableRelationships(
          candidates.pks,
          candidates.fks
        tokensUsed += result.tokensUsed;
        if (!result.validated) {
          this.onProgress(`LLM validation failed for ${tableKey}: ${result.reasoning}`);
        // Process LLM recommendations
        for (const rec of result.recommendations) {
          const recId = `${rec.target}:${rec.schemaName}.${rec.tableName}.${rec.columnName}`;
          if (rec.type === 'confirm') {
            validated.push(recId);
            // Mark as validated and potentially boost confidence
            if (rec.target === 'pk') {
              const pk = updatedPKs.find(p =>
                p.schemaName === rec.schemaName &&
                p.tableName === rec.tableName &&
                p.columnNames.includes(rec.columnName!)
              if (pk) {
                pk.validatedByLLM = true;
                pk.confidence = Math.min(pk.confidence + 15, 100); // Boost confidence
            } else if (rec.target === 'fk') {
              const fk = updatedFKs.find(f =>
                f.schemaName === rec.schemaName &&
                f.sourceTable === rec.tableName &&
                f.sourceColumn === rec.columnName!
              if (fk) {
                fk.validatedByLLM = true;
                fk.confidence = Math.min(fk.confidence + 20, 100); // Bigger boost for FKs
          } else if (rec.type === 'reject') {
            rejected.push(recId);
            // Remove or downgrade confidence
              const pkIndex = updatedPKs.findIndex(p =>
                updatedPKs[pkIndex].status = 'rejected';
                updatedPKs.splice(pkIndex, 1); // Remove rejected
              const fkIndex = updatedFKs.findIndex(f =>
                updatedFKs[fkIndex].status = 'rejected';
                updatedFKs.splice(fkIndex, 1); // Remove rejected
        this.onProgress(`Validated ${tableKey}`, {
          reasoning: result.reasoning.substring(0, 100) + '...'
        this.onProgress(`Error validating ${tableKey}: ${(error as Error).message}`);
      updatedPKs,
      updatedFKs,
      validated,
      rejected,
      tokensUsed
   * Check if backpropagation should be triggered
  private shouldTriggerBackprop(
  ): { shouldTrigger: boolean; reason?: string } {
    // Don't trigger on first iteration
    if (iteration.iteration === 1) {
      return { shouldTrigger: false };
    // Check if significant new discoveries were made
    const significantPKs = iteration.discoveries.newPKs.filter(pk => pk.confidence >= 80).length;
    const significantFKs = iteration.discoveries.newFKs.filter(fk => fk.confidence >= 80).length;
    if (significantPKs > 0 || significantFKs > 0) {
        shouldTrigger: true,
        reason: `Discovered ${significantPKs} high-confidence PKs and ${significantFKs} high-confidence FKs`
   * Perform backpropagation to update confidence scores
  private async performBackpropagation(
    newPKs: PKCandidate[],
    newFKs: FKCandidate[]
  ): Promise<Array<{ id: string; oldConfidence: number; newConfidence: number; reason: string }>> {
    const changes: Array<{
      oldConfidence: number;
      newConfidence: number;
    // Update FK confidence if their target is now a confirmed PK
    for (const fk of phase.discovered.foreignKeys) {
      const targetIsNowPK = newPKs.some(pk =>
        pk.schemaName === fk.targetSchema &&
        pk.tableName === fk.targetTable &&
        pk.columnNames.includes(fk.targetColumn) &&
        pk.confidence >= 80
      if (targetIsNowPK && fk.confidence < 90) {
        const oldConfidence = fk.confidence;
        // Boost FK confidence if pointing to a confirmed PK
        fk.confidence = Math.min(fk.confidence + 15, 95);
          id: `${fk.schemaName}.${fk.sourceTable}.${fk.sourceColumn}`,
          oldConfidence,
          newConfidence: fk.confidence,
          reason: 'Target column confirmed as PK'
    // Update PK confidence if columns have FKs pointing to them
    for (const pk of phase.discovered.primaryKeys) {
      const incomingFKs = newFKs.filter(fk =>
        fk.targetSchema === pk.schemaName &&
        fk.targetTable === pk.tableName &&
        pk.columnNames.includes(fk.targetColumn)
      if (incomingFKs > 0 && pk.confidence < 95) {
        const oldConfidence = pk.confidence;
        // Boost PK confidence if other tables reference it
        pk.confidence = Math.min(pk.confidence + (incomingFKs * 5), 95);
          id: `${pk.schemaName}.${pk.tableName}.${pk.columnNames.join('+')}`,
          newConfidence: pk.confidence,
          reason: `${incomingFKs} FK(s) now reference this column`
   * Merge iteration discoveries into phase
  private mergeDiscoveries(
    iteration: RelationshipDiscoveryIteration
    // Add new PKs (no duplicates)
    for (const newPK of iteration.discoveries.newPKs) {
      const exists = phase.discovered.primaryKeys.some(pk =>
        pk.schemaName === newPK.schemaName &&
        pk.tableName === newPK.tableName &&
        pk.columnNames.join(',') === newPK.columnNames.join(',')
        phase.discovered.primaryKeys.push(newPK);
    // Add new FKs (no duplicates)
    for (const newFK of iteration.discoveries.newFKs) {
      const exists = phase.discovered.foreignKeys.some(fk =>
        fk.schemaName === newFK.schemaName &&
        fk.sourceTable === newFK.sourceTable &&
        fk.sourceColumn === newFK.sourceColumn &&
        fk.targetSchema === newFK.targetSchema &&
        fk.targetTable === newFK.targetTable &&
        fk.targetColumn === newFK.targetColumn
        phase.discovered.foreignKeys.push(newFK);
   * Check if discovery should continue
  private shouldContinueDiscovery(
    iterationNumber: number
    // Always run at least 2 iterations
    if (iterationNumber < 2) {
    // Stop if no new discoveries
    if (iteration.discoveries.newPKs.length === 0 &&
        iteration.discoveries.newFKs.length === 0 &&
        iteration.discoveries.confidenceChanges.length === 0) {
    // Stop if only low-confidence discoveries
    const hasHighConfidenceDiscoveries =
      iteration.discoveries.newPKs.some(pk => pk.confidence >= 70) ||
      iteration.discoveries.newFKs.some(fk => fk.confidence >= 60);
    if (!hasHighConfidenceDiscoveries) {
   * Check guardrails
  private checkGuardrails(
    tokensUsed: number,
    iteration: number
  ): { canContinue: boolean; reason?: string } {
    // Check token budget
    if (tokensUsed >= maxTokens) {
        reason: `Token budget exhausted (${tokensUsed}/${maxTokens})`
    // Check max iterations
    if (iteration > this.config.backpropagation.maxIterations) {
        reason: `Max iterations reached (${this.config.backpropagation.maxIterations})`
    // Check if we're within warning threshold (80%)
    const tokenPercentage = tokensUsed / maxTokens;
    if (tokenPercentage > 0.8) {
      this.onProgress('Warning: Token budget 80% consumed', {
        used: tokensUsed,
        total: maxTokens
   * Calculate final summary statistics
  private calculateSummary(phase: RelationshipDiscoveryPhase): void {
    const allPKs = phase.discovered.primaryKeys;
    const allFKs = phase.discovered.foreignKeys;
    const allDiscoveries = [...allPKs, ...allFKs];
    // Count by confidence level
    const highConfidence = allDiscoveries.filter(d => d.confidence >= 80).length;
    const mediumConfidence = allDiscoveries.filter(d => d.confidence >= 50 && d.confidence < 80).length;
    const lowConfidence = allDiscoveries.filter(d => d.confidence < 50).length;
    const rejected = allDiscoveries.filter(d => d.status === 'rejected').length;
    // Calculate average confidence
    const totalConfidence = allDiscoveries.reduce((sum, d) => sum + d.confidence, 0);
    const averageConfidence = allDiscoveries.length > 0
      ? totalConfidence / allDiscoveries.length
    // Count unique tables with discovered PKs
    const tablesWithPKs = new Set(
      allPKs.map(pk => `${pk.schemaName}.${pk.tableName}`)
    ).size;
    // Count total tables analyzed
        tablesAnalyzed.add(`${schema.name}.${table.name}`);
    phase.summary = {
      totalTablesAnalyzed: tablesAnalyzed.size,
      tablesWithDiscoveredPKs: tablesWithPKs,
      relationshipsDiscovered: allFKs.length,
      averageConfidence: Math.round(averageConfidence),
      highConfidenceCount: highConfidence,
      mediumConfidenceCount: mediumConfidence,
      lowConfidenceCount: lowConfidence,
      rejectedCount: rejected
    phase.schemaEnhancements = {
      pkeysAdded: allPKs.filter(pk => pk.status === 'confirmed').length,
      fkeysAdded: allFKs.filter(fk => fk.status === 'confirmed').length,
      overallConfidence: Math.round(averageConfidence)
   * Apply discovered relationships to state
  public applyDiscoveriesToState(
    // Apply high-confidence PKs
      if (pk.confidence >= this.config.confidence.primaryKeyMinimum * 100) {
        const schema = state.schemas.find(s => s.name === pk.schemaName);
        if (!schema) continue;
        const table = schema.tables.find(t => t.name === pk.tableName);
        if (!table) continue;
        for (const columnName of pk.columnNames) {
          const column = table.columns.find(c => c.name === columnName);
            column.isPrimaryKey = true;
    // Apply high-confidence FKs
      if (fk.confidence >= this.config.confidence.foreignKeyMinimum * 100) {
        const schema = state.schemas.find(s => s.name === fk.schemaName);
        const table = schema.tables.find(t => t.name === fk.sourceTable);
        const column = table.columns.find(c => c.name === fk.sourceColumn);
            schema: fk.targetSchema,
            table: fk.targetTable,
            column: fk.targetColumn,
            referencedColumn: fk.targetColumn
    // Store discovery phase in state (new phases structure)
    state.phases.keyDetection = phase;
