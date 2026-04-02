 * Database driver abstraction types
 * Defines database-agnostic interfaces for multi-provider support
 * Database-agnostic schema representation
export interface AutoDocSchema {
  tables: AutoDocTable[];
 * Database-agnostic table representation
export interface AutoDocTable {
  columns: AutoDocColumn[];
  foreignKeys: AutoDocForeignKey[];
  primaryKeys: AutoDocPrimaryKey[];
 * Database-agnostic column representation
export interface AutoDocColumn {
  checkConstraint?: string;
 * Database-agnostic foreign key representation
export interface AutoDocForeignKey {
  referencedSchema: string;
  referencedTable: string;
  referencedColumn: string;
  constraintName?: string;
 * Database-agnostic primary key representation
export interface AutoDocPrimaryKey {
  ordinalPosition: number;
 * Database-agnostic column statistics
export interface AutoDocColumnStatistics {
  sampleValues: any[];
  min?: any;
  max?: any;
  stdDev?: number;
  avgLength?: number;
  valueDistribution?: AutoDocValueDistribution[];
 * Value distribution for low-cardinality columns
export interface AutoDocValueDistribution {
  frequency: number;
 * Existing description from database metadata
export interface AutoDocExistingDescription {
  target: 'table' | 'column';
  targetName: string; // Empty for table, column name for columns
 * Database connection configuration (provider-agnostic)
export interface AutoDocConnectionConfig {
  provider: 'sqlserver' | 'mysql' | 'postgresql' | 'oracle';
  username?: string; // Alias for user
  // MySQL specific
  socketPath?: string;
  // PostgreSQL specific
  ssl?: boolean | { rejectUnauthorized?: boolean };
  // Connection pool settings
 * Query result wrapper
export interface AutoDocQueryResult<T = any> {
  data?: T[];
 * Connection test result
export interface AutoDocConnectionTestResult {
  databaseName?: string;
 * Schema filter options
export interface AutoDocSchemaFilter {
 * Table filter options
export interface AutoDocTableFilter {
  includePattern?: RegExp;
  excludePattern?: RegExp;
