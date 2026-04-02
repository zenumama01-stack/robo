 * Type definitions for relationship discovery phase
 * Used to detect primary keys and foreign keys in databases with missing metadata
 * Evidence for why a column might be a primary key
export interface PKEvidence {
  uniqueness: number;        // 0-1: Percentage of unique values
  nullCount: number;         // Number of null values found
  totalRows: number;         // Total rows sampled
  dataPattern: 'sequential' | 'guid' | 'composite' | 'natural' | 'unknown';
  namingScore: number;       // 0-1: How well the name matches PK patterns
  dataTypeScore: number;     // 0-1: How appropriate the data type is for PK
  warnings: string[];        // Any issues found (e.g., "has nulls", "not unique")
 * Primary key candidate discovered during analysis
export interface PKCandidate {
  schemaName: string;
  tableName: string;
  columnNames: string[];     // Array to support composite keys
  confidence: number;        // 0-100: Overall confidence score
  evidence: PKEvidence;
  discoveredInIteration: number;
  validatedByLLM: boolean;
  status: 'candidate' | 'confirmed' | 'rejected';
 * Evidence for why a column might be a foreign key
export interface FKEvidence {
  namingMatch: number;       // 0-1: Similarity between column names
  valueOverlap: number;      // 0-1: Percentage of values that exist in target
  cardinalityRatio: number;  // Ratio of distinct values (many:one expected)
  dataTypeMatch: boolean;    // Do the data types match?
  nullPercentage: number;    // 0-1: Percentage of nulls (optional FK has nulls)
  sampleSize: number;        // How many rows were checked
  orphanCount: number;       // Values with no match in target
 * Foreign key candidate discovered during analysis
export interface FKCandidate {
  sourceColumn: string;
  targetSchema: string;
  targetColumn: string;
  evidence: FKEvidence;
 * Statistics about a column's data (for discovery)
 * Extended version of AutoDocColumnStatistics
export interface ColumnStatistics {
  minValue?: string | number;
  maxValue?: string | number;
  avgLength?: number;        // For string columns
  commonPatterns?: string[]; // Regex patterns found in data
  sampleValues: Array<string | number | null>;
 * Simpler column statistics interface for discovery
 * Maps to what the driver provides
export interface SimpleColumnStats {
 * Single iteration of the discovery process
export interface RelationshipDiscoveryIteration {
  phase: 'sampling' | 'pk_detection' | 'fk_detection' | 'sanity_check' | 'llm_validation' | 'backprop';
  completedAt: string;
    newPKs: PKCandidate[];
    newFKs: FKCandidate[];
    validated: string[];     // IDs of candidates that were validated
    rejected: string[];      // IDs of candidates that were rejected
    confidenceChanges: Array<{
  backpropTriggered: boolean;
  backpropReason?: string;
 * Feedback from analysis phase back to discovery
export interface AnalysisToDiscoveryFeedback {
  type: 'pk_invalidated' | 'fk_invalidated' | 'new_relationship' | 'confidence_change';
  evidence: string;          // What the LLM learned during analysis
  affectedCandidates: string[]; // IDs of affected PK/FK candidates
  recommendation: 'remove' | 'downgrade_confidence' | 'upgrade_confidence' | 'add_new';
  newConfidence?: number;    // If recommendation is to change confidence
  newRelationship?: {        // If recommendation is to add new relationship
 * Complete state of relationship discovery phase
export interface RelationshipDiscoveryPhase {
  triggered: boolean;
  triggerReason: 'missing_pks' | 'insufficient_fks' | 'both' | 'manual';
    actualFKs: number;
    fkDeficitPercentage: number;
    allocated: number;
    used: number;
  iterations: RelationshipDiscoveryIteration[];
    primaryKeys: PKCandidate[];
    foreignKeys: FKCandidate[];
    pkeysAdded: number;
    fkeysAdded: number;
    overallConfidence: number;  // 0-100: Confidence in all discoveries
  feedbackFromAnalysis: AnalysisToDiscoveryFeedback[];
    totalTablesAnalyzed: number;
    tablesWithDiscoveredPKs: number;
    relationshipsDiscovered: number;
    highConfidenceCount: number;   // confidence >= 80
    mediumConfidenceCount: number; // confidence 50-79
    lowConfidenceCount: number;    // confidence < 50
    rejectedCount: number;
 * Discovery trigger analysis
export interface DiscoveryTriggerAnalysis {
  shouldRun: boolean;
    expectedMinFKs: number;
    fkDeficit: number;
 * Cached column statistics for reuse across discovery and analysis
 * Pre-computed once and stored to avoid redundant queries
export interface CachedColumnStats {
  // Core statistics
  uniqueness: number;        // distinctCount / totalRows
  // Data ranges
  // Patterns and samples
  valueDistribution?: Array<{ value: string | number; frequency: number }>;
  computedAt: string;
  queryTimeMs: number;
 * Collection of cached stats for a table
export interface TableStatsCache {
  columns: Map<string, CachedColumnStats>;
 * LLM context for relationship discovery
 * Provides selective stats to LLM for intelligent reasoning
export interface LLMDiscoveryContext {
      uniqueness: number;
      dataPattern: string;
  relatedTables?: Array<{
  pkCandidates: Array<{
    columnNames: string[];
  fkCandidates: Array<{
 * LLM validation result
export interface LLMValidationResult {
  confidenceAdjustment: number;  // -100 to +100
  recommendations: Array<{
