 * LLM Discovery Validator
 * Uses LLM reasoning to validate and refine PK/FK candidates discovered through statistical analysis
 * Processes one table at a time with rich statistical context
import { BaseLLM, ChatParams, ChatResult } from '@memberjunction/ai';
  CachedColumnStats,
  LLMDiscoveryContext,
  LLMValidationResult
export class LLMDiscoveryValidator {
  private llm: BaseLLM;
    private config: RelationshipDiscoveryConfig,
    private aiConfig: AIConfig,
    private statsCache: ColumnStatsCache,
    private schemas: SchemaDefinition[]
    // Create LLM instance using MJ ClassFactory
    const llm = MJGlobal.Instance.ClassFactory.CreateInstance<BaseLLM>(
      aiConfig.provider,
      aiConfig.apiKey
    if (!llm) {
        `Failed to create LLM instance for provider: ${aiConfig.provider}. Check that the provider name matches a registered BaseLLM subclass.`
    this.llm = llm;
   * Validate PK/FK candidates for a single table using LLM reasoning
  public async validateTableRelationships(
    fkCandidates: FKCandidate[]
  ): Promise<LLMValidationResult> {
    // Build rich context for LLM
    const context = this.buildTableContext(schemaName, tableName, pkCandidates, fkCandidates);
    // Create prompt for LLM
    const prompt = this.buildValidationPrompt(context);
    // Call LLM
      model: this.aiConfig.model,
          content: 'You are a database schema expert specializing in relationship discovery and validation.'
      temperature: this.aiConfig.temperature ?? 0.1,
      maxOutputTokens: this.aiConfig.maxTokens,
    const chatResult: ChatResult = await this.llm.ChatCompletion(params);
        validated: false,
        reasoning: `LLM call failed: ${chatResult.errorMessage}`,
        confidenceAdjustment: 0,
        recommendations: [],
    // Parse LLM response
    const content = chatResult.data.choices[0].message.content;
    const usage = chatResult.data.usage;
      const result = JSON.parse(content) as {
        validated: boolean;
        confidenceAdjustments?: Array<{
          type: 'pk' | 'fk';
          column: string;
          adjustment: number;
        recommendations?: Array<{
          type: 'confirm' | 'reject' | 'modify' | 'add_new';
          target: 'pk' | 'fk';
          tableName?: string;
          columnName?: string;
          details: string;
        validated: result.validated,
        reasoning: result.reasoning,
        confidenceAdjustment: 0, // Overall adjustment
        recommendations: result.recommendations || [],
        tokensUsed: usage?.totalTokens || 0
        reasoning: `Failed to parse LLM response: ${(parseError as Error).message}\n\nRaw content:\n${content}`,
   * Build rich context for a table including stats from cache
  ): LLMDiscoveryContext {
    // Get table stats from cache
    const tableStats = this.statsCache.getTableStats(schemaName, tableName);
      console.warn(`[LLMDiscoveryValidator] No cached stats found for table ${schemaName}.${tableName} - validation will be limited`);
      // Return minimal context without column stats
        targetTable: {
          columns: []
        relatedTables: [],
        pkCandidates: pkCandidates.map(pk => ({
          columnNames: pk.columnNames,
          confidence: pk.confidence,
          reasoning: pk.evidence ? `Uniqueness: ${pk.evidence.uniqueness}, Naming: ${pk.evidence.namingScore}` : 'Statistical analysis'
        fkCandidates: fkCandidates.map(fk => ({
          sourceColumn: fk.sourceColumn,
          targetTable: `${fk.targetSchema}.${fk.targetTable}`,
          targetColumn: fk.targetColumn,
          confidence: fk.confidence,
          reasoning: fk.evidence ? `Naming: ${fk.evidence.namingMatch}, Value overlap: ${fk.evidence.valueOverlap}` : 'Statistical analysis'
    // Build target table context with column stats
    const targetTable = {
      rowCount: tableStats.totalRows,
      columns: Array.from(tableStats.columns.values()).map(col => ({
        name: col.columnName,
        type: col.dataType,
        uniqueness: col.uniqueness,
        nullPercentage: col.nullPercentage,
        distinctCount: col.distinctCount,
        dataPattern: col.dataPattern,
        sampleValues: col.sampleValues.slice(0, 10) // Limit sample size for prompt
    // Find related tables (tables with columns that have similar names)
    const relatedTables = this.findRelatedTables(schemaName, tableName);
    // Build PK candidates context
    const pkContext = pkCandidates.map(pk => ({
      reasoning: this.buildPKReasoning(pk)
    // Build FK candidates context
    const fkContext = fkCandidates
      .filter(fk => fk.sourceTable === tableName) // Only FKs from this table
      .map(fk => ({
        reasoning: this.buildFKReasoning(fk)
      relatedTables,
      pkCandidates: pkContext,
      fkCandidates: fkContext
   * Find tables that might be related based on column name patterns
  private findRelatedTables(
    rowCount: number;
    potentialRelationships: Array<{
      columnName: string;
    const related: Array<{
    // Get all columns in target table
    const targetTableStats = this.statsCache.getTableStats(schemaName, tableName);
    if (!targetTableStats) return related;
    const targetColumns = Array.from(targetTableStats.columns.values());
    // Look for columns with similar names in other tables
    for (const otherTableStats of this.statsCache.getAllTables()) {
      // Skip the target table itself
        otherTableStats.schemaName === schemaName &&
        otherTableStats.tableName === tableName
      const potentialRelationships: Array<{
      for (const targetCol of targetColumns) {
        for (const otherCol of otherTableStats.columns.values()) {
          const similarity = this.calculateColumnSimilarity(
            targetCol,
            otherCol
            potentialRelationships.push({
              columnName: `${targetCol.columnName} ↔ ${otherCol.columnName}`,
              similarity,
              reason: this.explainSimilarity(targetCol, otherCol, similarity)
      if (potentialRelationships.length > 0) {
        related.push({
          schema: otherTableStats.schemaName,
          table: otherTableStats.tableName,
          rowCount: otherTableStats.totalRows,
          potentialRelationships: potentialRelationships.sort(
            (a, b) => b.similarity - a.similarity
          ).slice(0, 5) // Top 5 relationships
    // Return top 10 most related tables
    return related
        const aMaxSim = Math.max(...a.potentialRelationships.map(r => r.similarity));
        const bMaxSim = Math.max(...b.potentialRelationships.map(r => r.similarity));
        return bMaxSim - aMaxSim;
   * Calculate similarity between two columns
  private calculateColumnSimilarity(
    col1: CachedColumnStats,
    col2: CachedColumnStats
    // Name similarity (50% weight)
    const nameSim = this.calculateNameSimilarity(col1.columnName, col2.columnName);
    score += nameSim * 0.5;
    // Data type match (30% weight)
    if (this.areDataTypesCompatible(col1.dataType, col2.dataType)) {
      score += 0.3;
    // Cardinality relationship (20% weight)
    // If one column has much higher uniqueness, they might be PK-FK pair
    const uniquenessDiff = Math.abs(col1.uniqueness - col2.uniqueness);
    if (uniquenessDiff > 0.3) {
      score += 0.2;
    return score;
   * Calculate name similarity using Levenshtein distance
    return 1 - distance / maxLength;
   * Calculate Levenshtein distance
   * Check if data types are compatible
  private areDataTypesCompatible(type1: string, type2: string): boolean {
    const normalize = (type: string) =>
        .replace(/\([^)]*\)/g, '')
    const t1 = normalize(type1);
    const t2 = normalize(type2);
    if (t1 === t2) return true;
    if (intTypes.some(t => t1.includes(t)) && intTypes.some(t => t2.includes(t))) {
      stringTypes.some(t => t1.includes(t)) &&
      stringTypes.some(t => t2.includes(t))
      (t1.includes('uniqueidentifier') || t1.includes('uuid') || t1.includes('guid')) &&
      (t2.includes('uniqueidentifier') || t2.includes('uuid') || t2.includes('guid'))
   * Explain why two columns are similar
  private explainSimilarity(
    col2: CachedColumnStats,
    similarity: number
    if (col1.columnName.toLowerCase() === col2.columnName.toLowerCase()) {
      reasons.push('exact name match');
    } else if (
      col1.columnName.toLowerCase().includes(col2.columnName.toLowerCase()) ||
      col2.columnName.toLowerCase().includes(col1.columnName.toLowerCase())
      reasons.push('name similarity');
      reasons.push('compatible types');
    if (Math.abs(col1.uniqueness - col2.uniqueness) > 0.3) {
      reasons.push('PK-FK cardinality pattern');
    return reasons.join(', ');
   * Build reasoning string for PK candidate
  private buildPKReasoning(pk: PKCandidate): string {
    if (pk.evidence.uniqueness >= 0.99) {
      reasons.push('highly unique');
    if (pk.evidence.nullCount === 0) {
      reasons.push('no nulls');
    if (pk.evidence.namingScore > 0.7) {
      reasons.push('matches PK naming pattern');
    if (pk.evidence.dataPattern !== 'unknown') {
      reasons.push(`${pk.evidence.dataPattern} pattern`);
   * Build reasoning string for FK candidate
  private buildFKReasoning(fk: FKCandidate): string {
    if (fk.evidence.valueOverlap >= 0.9) {
      reasons.push(`${(fk.evidence.valueOverlap * 100).toFixed(0)}% value overlap`);
    if (fk.evidence.namingMatch > 0.7) {
      reasons.push('strong name similarity');
    if (fk.evidence.dataTypeMatch) {
      reasons.push('matching data types');
    if (fk.evidence.orphanCount === 0) {
      reasons.push('no orphans');
   * Build LLM validation prompt
  private buildValidationPrompt(context: LLMDiscoveryContext): string {
You are a database schema expert analyzing potential primary keys and foreign keys.
## Target Table: ${context.targetTable.schema}.${context.targetTable.table}
Row Count: ${context.targetTable.rowCount}
### Columns and Statistics:
${context.targetTable.columns
    col => `
- **${col.name}** (${col.type})
  - Uniqueness: ${(col.uniqueness * 100).toFixed(1)}%
  - Null %: ${(col.nullPercentage * 100).toFixed(1)}%
  - Distinct Values: ${col.distinctCount}
  - Pattern: ${col.dataPattern}
  - Sample: ${col.sampleValues.slice(0, 5).join(', ')}
  context.relatedTables && context.relatedTables.length > 0
### Related Tables (by column name similarity):
${context.relatedTables
    table => `
- **${table.schema}.${table.table}** (${table.rowCount} rows)
  ${table.potentialRelationships.map(rel => `  - ${rel.columnName}: ${rel.reason}`).join('\n  ')}
### Statistical Analysis Found:
**Primary Key Candidates:**
  context.pkCandidates.length > 0
    ? context.pkCandidates
          pk => `
- ${pk.columnNames.join(', ')} (${pk.confidence}% confidence)
  Reasoning: ${pk.reasoning}
    : 'None'
**Foreign Key Candidates:**
  context.fkCandidates.length > 0
    ? context.fkCandidates
          fk => `
- ${fk.sourceColumn} → ${fk.targetTable}.${fk.targetColumn} (${fk.confidence}% confidence)
  Reasoning: ${fk.reasoning}
## Your Task:
Analyze the statistical findings and provide validation:
1. **Validate Primary Keys**: Are the PK candidates correct? Should any be removed or added?
2. **Validate Foreign Keys**: Are the FK candidates correct? Look for:
   - Columns marked as PKs that should actually be FKs
   - Missing FK relationships based on column names and data patterns
3. **Cross-table Context**: Use the related tables information to identify relationships
**Output Format:**
  "validated": true/false,
  "reasoning": "Your detailed analysis",
  "confidenceAdjustments": [
    { "type": "pk|fk", "table": "...", "column": "...", "adjustment": -20 to +20, "reason": "..." }
  "recommendations": [
    { "type": "confirm|reject|modify|add_new", "target": "pk|fk", "details": "..." }
