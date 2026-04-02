 * State file format for DBAutoDoc
 * Tracks all documentation, iterations, and analysis progress
import { RelationshipDiscoveryPhase, CachedColumnStats } from './discovery.js';
import { SampleQuery, SampleQueryGenerationSummary } from './sample-queries.js';
export interface DatabaseDocumentation {
  summary: AnalysisSummary; // Now includes timing and iteration counts
  database: DatabaseInfo;
  seedContext?: SeedContext;
  phases: AnalysisPhases; // Multi-phase workflow organization
  schemas: SchemaDefinition[]; // Deliverable: Database structure with descriptions
  sampleQueries?: SampleQueriesDeliverable; // Deliverable: Training queries generated from schemas
  resumedFromFile?: string; // Path to the state file this analysis resumed from
 * Summary of analysis with timing and high-level metrics
 * Moved timing fields from top-level DatabaseDocumentation here
export interface AnalysisSummary {
  // Timing and versioning
  totalIterations: number;
  // Token and cost metrics
  totalPromptsRun: number;
  totalInputTokens: number;
  totalOutputTokens: number;
  // Schema metrics
  totalSchemas: number;
export interface DatabaseInfo {
  analyzedAt: string;
export interface SeedContext {
 * Multi-phase workflow organization
 * Each phase is optional and tracked separately
 * Phases describe HOW deliverables were generated (process history)
export interface AnalysisPhases {
  keyDetection?: RelationshipDiscoveryPhase; // Primary key and foreign key detection
  descriptionGeneration: AnalysisRun[]; // Table and column description analysis
  queryGeneration?: QueryGenerationPhase; // Metadata about query generation process
 * Query generation phase metadata (process tracking)
 * The actual queries are stored as a top-level deliverable
export interface QueryGenerationPhase {
  queriesGenerated: number;
 * Sample queries deliverable (product of analysis)
 * Training queries generated from database schemas for AI agents
export interface SampleQueriesDeliverable {
  status: 'completed' | 'partial' | 'failed';
  modelUsed?: string;
export interface SchemaDefinition {
  tables: TableDefinition[];
  descriptionIterations: DescriptionIteration[];
  inferredPurpose?: string;
export interface TableDefinition {
  dependencyLevel?: number;
  dependsOn: ForeignKeyReference[];
  dependents: ForeignKeyReference[];
  columns: ColumnDefinition[];
  userDescription?: string;
  userApproved?: boolean;
export interface ColumnDefinition {
  foreignKeyReferences?: ForeignKeyReference;
  possibleValues?: any[];
  statistics?: ColumnStatistics;
export interface ForeignKeyReference {
 * Column statistics merged from both discovery cache and description analysis
 * Replaces separate ColumnStatisticsCache - now embedded in column definitions
  // Core statistics (from discovery cache)
  uniquenessRatio: number; // distinctCount / totalRows
  // Data patterns (from discovery cache)
  dataPattern?: 'sequential' | 'guid' | 'composite' | 'natural' | 'unknown';
  minValue?: string | number; // Alias for min
  maxValue?: string | number; // Alias for max
  // Numeric statistics
  median?: number;
  percentiles?: {
    p25: number;
    p50: number;
    p75: number;
    p95: number;
  // String statistics
  commonPrefixes?: string[];
  formatPattern?: string;
  containsUrls?: boolean;
  containsEmails?: boolean;
  containsPhones?: boolean;
  // Cache metadata
  computedAt?: string;
  queryTimeMs?: number;
export interface DescriptionIteration {
  triggeredBy?: 'initial' | 'backpropagation' | 'refinement' | 'dependency_sanity_check' | 'schema_sanity_check' | 'cross_schema_sanity_check';
  changedFrom?: string;
export interface AnalysisRun {
  status: 'in_progress' | 'completed' | 'failed' | 'converged';
  levelsProcessed: number;
  sanityCheckCount: number;
  convergenceReason?: string;
  vendor: string;
  temperature: number;
  processingLog: ProcessingLogEntry[];
  sanityChecks: SanityCheckRecord[];
  resumedFromFile?: string; // Path to the state file this run resumed from
  resumedAt?: string; // Timestamp when this run resumed
  // Granular guardrail tracking
  phaseMetrics?: PhaseMetrics; // Per-phase token and cost tracking
  iterationMetrics?: IterationMetrics[]; // Per-iteration tracking
  guardrailsEnforced?: GuardrailEnforcement; // Info about guardrails that triggered
 * Per-phase token and cost metrics for granular guardrail enforcement
export interface PhaseMetrics {
  discovery?: PhaseMetric;     // Discovery phase
  analysis?: PhaseMetric;      // Main analysis phase
  sanityChecks?: PhaseMetric;  // Sanity checks phase
export interface PhaseMetric {
  warned?: boolean;      // Did this phase trigger a token warning?
  exceeded?: boolean;    // Did this phase exceed its hard limit?
 * Per-iteration metrics for detecting iteration-level resource exhaustion
export interface IterationMetrics {
  iterationNumber: number;
  warned?: boolean;  // Did this iteration trigger a warning?
 * Information about guardrails that were enforced or triggered
export interface GuardrailEnforcement {
  exceedances: GuardrailExceeded[];  // Which limits were exceeded
  warnings: GuardrailWarning[];      // Which warnings were triggered
  stoppedDueToGuardrails?: boolean;  // Was execution stopped due to guardrails?
  stoppedReason?: string;            // Reason for stopping
export interface GuardrailExceeded {
  type: 'tokens_per_run' | 'tokens_per_phase' | 'tokens_per_iteration' | 'duration' | 'cost' | 'iteration_duration';
  phase?: string;                    // Phase name if phase-specific
  iteration?: number;                // Iteration number if iteration-specific
  actual: number;
  unit: string;                      // 'tokens', 'seconds', 'dollars', etc.
export interface GuardrailWarning {
  type: string;                      // Type of warning
  percentage: number;                // How close to limit (0-100)
export interface ProcessingLogEntry {
  action: 'analyze' | 'backpropagate' | 'dependency_sanity_check' | 'schema_sanity_check' | 'cross_schema_sanity_check';
  result: 'success' | 'changed' | 'unchanged' | 'error';
  semanticComparison?: {
    columnChanges: Array<{
export interface SanityCheckRecord {
  checkType: 'dependency_level' | 'schema_level' | 'cross_schema';
  scope: string; // e.g., "level 2", "AssociationDemo schema", "all schemas"
  issuesFound: number;
  tablesAffected: string[];
  result: 'no_issues' | 'issues_corrected' | 'issues_flagged';
 * DEPRECATED: ColumnStatisticsCache is no longer used
 * Statistics are now embedded directly in ColumnDefinition.statistics
 * This type remains for backward compatibility with old state files
export interface ColumnStatisticsCache {
  tables: Record<string, TableStatisticsEntry>; // Key: "schema.table"
 * DEPRECATED: TableStatisticsEntry is no longer used
export interface TableStatisticsEntry {
  columns: CachedColumnStats[];
