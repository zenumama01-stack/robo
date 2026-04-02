 * Prompt-related types for DBAutoDoc
export interface PromptExecutionResult<T> {
  promptInput?: string;
  promptOutput?: string;
  guardrailExceeded?: boolean;
export interface TableAnalysisPromptResult {
  tableDescription: string;
  columnDescriptions: ColumnDescriptionPromptResult[];
  foreignKeys?: ForeignKeyPromptResult[];
  parentTableInsights?: ParentTableInsight[];
export interface ForeignKeyPromptResult {
  referencesSchema: string;
  referencesTable: string;
  referencesColumn: string;
export interface ParentTableInsight {
  parentTable: string;  // "schema.table" format
export interface ColumnDescriptionPromptResult {
export interface BackpropagationPromptResult {
  needsRevision: boolean;
  revisedDescription?: string;
export interface SchemaSanityCheckPromptResult {
  schemaDescription: string;
  inconsistencies: string[];
export interface CrossSchemaSanityCheckPromptResult {
  insights: string[];
  globalPatterns: string[];
export interface ConvergenceCheckPromptResult {
  hasConverged: boolean;
  recommendedActions: string[];
export interface SemanticComparisonPromptResult {
  tableMateriallyChanged: boolean;
  tableChangeReasoning: string;
  columnChanges: ColumnChangeResult[];
export interface ColumnChangeResult {
  materiallyChanged: boolean;
  changeReasoning: string;
// Dependency-Level Sanity Check Types
export interface DependencyLevelSanityCheckResult {
  hasMaterialIssues: boolean;
  overallAssessment: string;
  tableIssues: TableIssue[];
  crossTableObservations: CrossTableObservation[];
export interface TableIssue {
  issueType: 'description' | 'business_purpose' | 'relationships' | 'terminology';
  suggestedFix: string;
export interface CrossTableObservation {
  tables: string[];
  observation: string;
  impact: string;
  recommendation: string;
// Schema-Level Sanity Check Types
export interface SchemaLevelSanityCheckResult {
  schemaCoherence: 'excellent' | 'good' | 'fair' | 'poor';
  schemaLevelIssues: SchemaLevelIssue[];
  architecturalPatterns: ArchitecturalPattern[];
  businessDomainSuggestions: BusinessDomainSuggestion[];
export interface SchemaLevelIssue {
  issueType: 'consistency' | 'relationships' | 'business_domain' | 'naming' | 'architecture' | 'missing_pattern';
  affectedTables: string[];
  suggestedSchemaDescription?: string;
export interface ArchitecturalPattern {
  pattern: 'audit_trail' | 'configuration' | 'lookup' | 'transaction' | 'versioning' | 'soft_delete' | 'hierarchy';
export interface BusinessDomainSuggestion {
  suggestedDomain: string;
// Cross-Schema Sanity Check Types
export interface CrossSchemaSanityCheckResult {
  overallConsistency: 'excellent' | 'good' | 'fair' | 'poor';
  crossSchemaIssues: CrossSchemaIssue[];
  terminologyConflicts: TerminologyConflict[];
  schemaIssues: SchemaIssue[];
  databaseLevelObservations: DatabaseLevelObservation[];
export interface CrossSchemaIssue {
  issueType: 'terminology' | 'shared_tables' | 'relationships' | 'business_domains' | 'naming' | 'duplication';
  affectedSchemas: string[];
  affectedTables: Array<{ schema: string; table: string }>;
  suggestedResolution: string;
export interface TerminologyConflict {
  term: string;
  usages: Array<{
    meaning: string;
  recommendedStandardization: string;
export interface SchemaIssue {
  issueType: 'description' | 'business_domain' | 'relationships';
export interface DatabaseLevelObservation {
