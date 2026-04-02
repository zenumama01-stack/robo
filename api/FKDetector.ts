 * Foreign Key Detection
 * Analyzes columns to find potential foreign key relationships based on
 * naming patterns, value overlap, and cardinality analysis
import { SchemaDefinition, TableDefinition, ColumnDefinition } from '../types/state.js';
import { FKCandidate, FKEvidence, PKCandidate } from '../types/discovery.js';
import { RelationshipDiscoveryConfig } from '../types/config.js';
export class FKDetector {
    private config: RelationshipDiscoveryConfig
   * Detect foreign key candidates for a table
  public async detectFKCandidates(
    schemas: SchemaDefinition[],
    sourceSchema: string,
    sourceTable: TableDefinition,
    discoveredPKs: PKCandidate[],
    const candidates: FKCandidate[] = [];
    console.log(`[FKDetector] Analyzing table ${sourceSchema}.${sourceTable.name} with ${sourceTable.columns.length} columns`);
    console.log(`[FKDetector] Available PKs: ${discoveredPKs.length}`);
    // For each column in source table
    for (const sourceColumn of sourceTable.columns) {
      // Skip if column is a discovered PK
      const isPK = discoveredPKs.some(pk =>
        pk.schemaName === sourceSchema &&
        pk.tableName === sourceTable.name &&
        pk.columnNames.includes(sourceColumn.name)
      if (isPK) {
        console.log(`[FKDetector]   Skip ${sourceColumn.name} - is a PK`);
      // Find potential target tables/columns
      const potentialTargets = this.findPotentialTargets(
        sourceSchema,
        sourceTable.name,
        sourceColumn,
        discoveredPKs
      console.log(`[FKDetector]   Column ${sourceColumn.name}: Found ${potentialTargets.length} potential targets`);
      if (potentialTargets.length > 0) {
        console.log(`[FKDetector]     Targets: ${potentialTargets.map(t => `${t.schemaName}.${t.tableName}.${t.columnName}`).join(', ')}`);
      // Analyze each potential target
      for (const target of potentialTargets) {
        const candidate = await this.analyzeFKCandidate(
          target.schemaName,
          target.tableName,
          target.columnName,
          target.isPK,
          console.log(`[FKDetector]     Candidate confidence: ${candidate.confidence} (min: ${this.config.confidence.foreignKeyMinimum * 100})`);
        if (candidate && candidate.confidence >= this.config.confidence.foreignKeyMinimum * 100) {
          console.log(`[FKDetector]     ✓ Added FK candidate: ${sourceColumn.name} -> ${target.tableName}.${target.columnName}`);
    console.log(`[FKDetector] Table ${sourceSchema}.${sourceTable.name}: Found ${candidates.length} FK candidates`);
    // Sort by confidence descending
    return candidates.sort((a, b) => b.confidence - a.confidence);
   * Find potential FK target tables/columns based on naming
  private findPotentialTargets(
    sourceTable: string,
    sourceColumn: ColumnDefinition,
  ): Array<{ schemaName: string; tableName: string; columnName: string; isPK: boolean }> {
    const targets: Array<{ schemaName: string; tableName: string; columnName: string; isPK: boolean }> = [];
    // Pattern 1: CustomerID -> Customer.ID (exact match on table name)
    const tableNamePattern = this.extractTableNameFromColumn(sourceColumn.name);
    if (tableNamePattern) {
        const matchingTable = schema.tables.find(t =>
          t.name.toLowerCase() === tableNamePattern.toLowerCase()
        if (matchingTable && matchingTable.name !== sourceTable) {
          // Look for ID column
          const idColumn = matchingTable.columns.find(c =>
            c.name.toLowerCase() === `${matchingTable.name.toLowerCase()}id`
              pk.schemaName === schema.name &&
              pk.tableName === matchingTable.name &&
              pk.columnNames.includes(idColumn.name)
            targets.push({
              tableName: matchingTable.name,
              columnName: idColumn.name,
              isPK
    // Pattern 2: Check all discovered PKs with similar names
    for (const pk of discoveredPKs) {
      const pkSchema = schemas.find(s => s.name === pk.schemaName);
      if (!pkSchema) continue;
      const pkTable = pkSchema.tables.find(t => t.name === pk.tableName);
      if (!pkTable || pkTable.name === sourceTable) continue;
      for (const pkColumnName of pk.columnNames) {
        const similarity = this.calculateNameSimilarity(sourceColumn.name, pkColumnName);
        if (similarity > 0.6) {
            schemaName: pk.schemaName,
            tableName: pk.tableName,
            columnName: pkColumnName,
            isPK: true
    // Pattern 3: Same column name in different tables (might be FK)
        if (table.name === sourceTable) continue;
        const matchingColumn = table.columns.find(c =>
          c.name.toLowerCase() === sourceColumn.name.toLowerCase() &&
          c.dataType === sourceColumn.dataType
        if (matchingColumn) {
            pk.tableName === table.name &&
            pk.columnNames.includes(matchingColumn.name)
          // Only add if it's a PK in target table (more likely to be FK)
              tableName: table.name,
              columnName: matchingColumn.name,
    return targets;
   * Extract table name from column name (e.g., "CustomerID" -> "Customer")
  private extractTableNameFromColumn(columnName: string): string | null {
    // Remove common suffixes
    const suffixes = ['ID', 'Id', 'id', 'KEY', 'Key', 'key', 'FK', 'Fk', 'fk'];
      if (columnName.endsWith(suffix)) {
        const tableName = columnName.slice(0, -suffix.length);
        if (tableName.length > 0) {
          return tableName;
   * Calculate name similarity (0-1)
  private calculateNameSimilarity(name1: string, name2: string): number {
    const lower1 = name1.toLowerCase();
    const lower2 = name2.toLowerCase();
    if (lower1 === lower2) return 1.0;
    // One contains the other
    if (lower1.includes(lower2) || lower2.includes(lower1)) {
      return 0.8;
    // Levenshtein distance
    const distance = this.levenshteinDistance(lower1, lower2);
    const maxLength = Math.max(lower1.length, lower2.length);
    return 1 - (distance / maxLength);
   * Calculate Levenshtein distance between two strings
   * Analyze a specific FK candidate
  private async analyzeFKCandidate(
    targetSchema: string,
    targetTable: string,
    targetColumn: string,
    targetIsPK: boolean,
  ): Promise<FKCandidate | null> {
    // Get target column info
    const targetColumnDef = await this.driver.getColumnInfo(targetSchema, targetTable, targetColumn);
    // Check data type match
    const dataTypeMatch = this.isDataTypeCompatible(sourceColumn.dataType, targetColumnDef.type);
    if (!dataTypeMatch) {
      return null; // Data types must match
    // Calculate naming match
    const namingMatch = this.calculateNameSimilarity(sourceColumn.name, targetColumn);
    // Check value overlap
    const overlapResult = await this.driver.testValueOverlap(
      `${sourceSchema}.${sourceTable}`,
      sourceColumn.name,
      `${targetSchema}.${targetTable}`,
      targetColumn,
      this.config.sampling.valueOverlapSampleSize
    // Get column statistics for cardinality analysis
    const sourceStats = await this.driver.getColumnStatisticsForDiscovery(
      sourceColumn.dataType,
      this.config.sampling.maxRowsPerTable
    const targetStats = await this.driver.getColumnStatisticsForDiscovery(
      targetSchema,
      targetTable,
      targetColumnDef.type,
    // Calculate cardinality ratio (should be many:one for FK)
    const cardinalityRatio = targetStats.distinctCount > 0
      ? sourceStats.distinctCount / targetStats.distinctCount
    // Calculate null percentage
    const nullPercentage = sourceStats.totalRows > 0
      ? sourceStats.nullCount / sourceStats.totalRows
    // Calculate orphan count
    const orphanCount = Math.floor(sourceStats.totalRows * (1 - overlapResult));
    // Build evidence
    const evidence: FKEvidence = {
      namingMatch,
      valueOverlap: overlapResult,
      cardinalityRatio,
      nullPercentage,
      sampleSize: this.config.sampling.valueOverlapSampleSize,
      orphanCount,
    // Add warnings
    if (overlapResult < 0.8) {
      evidence.warnings.push(`Only ${(overlapResult * 100).toFixed(1)}% of values exist in target`);
    if (orphanCount > 0) {
      evidence.warnings.push(`${orphanCount} orphaned values found`);
    if (nullPercentage > 0.5) {
      evidence.warnings.push(`${(nullPercentage * 100).toFixed(1)}% null values (optional FK)`);
    if (cardinalityRatio < 0.5) {
      evidence.warnings.push('Cardinality suggests one:many instead of many:one');
    // Calculate confidence
    const confidence = this.calculateFKConfidence(evidence, targetIsPK);
    // Don't return if confidence too low
    if (confidence < 40) {
      schemaName: sourceSchema,
      sourceColumn: sourceColumn.name,
      confidence,
      evidence,
      discoveredInIteration: iteration,
      validatedByLLM: false,
      status: 'candidate'
   * Check if data types are compatible for FK relationship
  private isDataTypeCompatible(sourceType: string, targetType: string): boolean {
    const normalize = (type: string) => type.toLowerCase()
      .replace(/\([^)]*\)/g, '') // Remove size specifiers
      .replace(/\s+/g, '');
    const source = normalize(sourceType);
    const target = normalize(targetType);
    if (source === target) return true;
    // INT variants
    const intTypes = ['int', 'integer', 'bigint', 'smallint', 'tinyint'];
    if (intTypes.some(t => source.includes(t)) && intTypes.some(t => target.includes(t))) {
    // String variants
    const stringTypes = ['varchar', 'char', 'nvarchar', 'nchar', 'text'];
    if (stringTypes.some(t => source.includes(t)) && stringTypes.some(t => target.includes(t))) {
    // GUID variants
    if ((source.includes('uniqueidentifier') || source.includes('uuid') || source.includes('guid')) &&
        (target.includes('uniqueidentifier') || target.includes('uuid') || target.includes('guid'))) {
   * Calculate FK confidence score (0-100)
  private calculateFKConfidence(evidence: FKEvidence, targetIsPK: boolean): number {
    // Value overlap is critical (40% weight)
    score += evidence.valueOverlap * 40;
    // Naming match (20% weight)
    score += evidence.namingMatch * 20;
    // Cardinality check (15% weight)
    // We want many:one ratio (ratio > 1 is good)
    const cardinalityScore = Math.min(evidence.cardinalityRatio, 2) / 2; // Cap at 2:1
    score += cardinalityScore * 15;
    // Target is PK bonus (15% weight)
    if (targetIsPK) {
      score += 15;
    // Null handling (10% weight)
    // Some nulls are OK (optional FK), but too many is suspicious
    const nullScore = evidence.nullPercentage < 0.3 ? 10 :
                     evidence.nullPercentage < 0.7 ? 5 : 0;
    score += nullScore;
    // Penalties
    if (evidence.orphanCount > evidence.sampleSize * 0.2) {
      score *= 0.7; // 30% penalty for many orphans
    if (!evidence.dataTypeMatch) {
      score *= 0.5; // 50% penalty for type mismatch
