 * Sample query generation types for DBAutoDoc
 * These queries serve as reference implementations for AI agents like Skip
 * Phase 1: Query Planning - lightweight descriptions of what queries to create
export interface QueryPlan {
  primaryEntities: EntityReference[];
  relatedEntities: EntityReference[];
  relatedQueryIds: string[];  // For alignment tracking
 * Phase 2: SQL Generation - detailed SQL implementation for a single query
export interface QuerySQL {
  parameters: QueryParameter[];
  sampleResultColumns: ResultColumn[];
 * Complete sample query combining plan + SQL + execution results
export interface SampleQuery {
  /** Unique identifier for this query */
  /** Detailed description of what this query does */
  /** Business purpose and use case */
  /** Schema this query belongs to */
  /** Entity context */
  /** Query metadata */
  /** The actual SQL query */
  /** Query parameters */
  /** Results documentation */
  sampleResultRows: Record<string, unknown>[];
  expectedRowCount?: RowCountRange;
  /** Business logic documentation */
  /** For multi-query alignment */
  /** Execution metadata */
  validationError?: string;
  /** Fix attempt tracking */
  fixAttempts?: number;
  fixHistory?: Array<{ sql: string; error: string }>;
  /** Refinement tracking */
  refinementAttempts?: number;
  refinementHistory?: Array<{ sql: string; feedback: string }>;
  wasRefined?: boolean;
  /** Generation metadata */
  generatedAt: string;
  modelUsed: string;
export interface EntityReference {
  alias?: string;
export type QueryType =
  | 'aggregation'
  | 'filter'
  | 'join'
  | 'detail'
  | 'summary'
  | 'ranking'
  | 'time-series'
  | 'drill-down';
export type QueryPattern =
  | 'simple-select'
  | 'filtered-select'
  | 'aggregation-group-by'
  | 'time-series-aggregation'
  | 'join-detail'
  | 'left-join-counts'
  | 'drill-down-detail'
  | 'ranking-top-n'
  | 'multi-level-aggregation';
export type QueryComplexity = 'simple' | 'moderate' | 'complex';
export interface QueryParameter {
export interface ResultColumn {
export interface RowCountRange {
  typical: number;
export interface SampleQueryGenerationResult {
  queries: SampleQuery[];
  summary: SampleQueryGenerationSummary;
export interface SampleQueryGenerationSummary {
  totalQueriesGenerated: number;
  queriesValidated: number;
  queriesFailed: number;
  queriesByType: Record<QueryType, number>;
  queriesByPattern: Record<QueryPattern, number>;
  queriesByComplexity: Record<QueryComplexity, number>;
  queriesPerTable: number;
  maxExecutionTime: number;
  includeMultiQueryPatterns: boolean;
  validateAlignment: boolean;
  tokenBudget: number;  // Token budget for query generation phase (default: 100000, set to 0 for unlimited)
  queryTypes?: QueryType[];
  maxRowsInSample: number;
  maxTables?: number;  // Max tables to generate queries for (default: 10, set to 0 for all tables)
  enableQueryFix?: boolean;  // Enable automatic query fix attempts (default: true)
  maxFixAttempts?: number;  // Maximum number of fix attempts per query (default: 3)
  enableQueryRefinement?: boolean;  // Enable LLM-based result analysis and refinement (default: false)
  maxRefinementAttempts?: number;  // Maximum refinement iterations per query (default: 1)
export interface QueryGenerationContext {
  tables: TableContext[];
  existingQueries: SampleQuery[];
export interface TableContext {
  columns: ColumnContext[];
  foreignKeys: ForeignKeyContext[];
  dependents: string[];
export interface ColumnContext {
export interface ForeignKeyContext {
