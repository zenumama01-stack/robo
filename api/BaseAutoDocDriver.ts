 * Base database driver for DBAutoDoc
 * Defines abstract interface for all database providers
  AutoDocSchema,
  AutoDocTable,
  AutoDocColumn,
  AutoDocConnectionConfig,
  AutoDocQueryResult,
  AutoDocConnectionTestResult,
  AutoDocSchemaFilter,
  AutoDocTableFilter,
  AutoDocColumnStatistics,
  AutoDocExistingDescription
} from '../types/driver.js';
 * Abstract base class for database drivers
 * All provider-specific implementations must extend this class
export abstract class BaseAutoDocDriver {
  constructor(protected config: AutoDocConnectionConfig) {}
  // CONNECTION MANAGEMENT
   * Connect to the database
  public abstract connect(): Promise<void>;
   * Test database connectivity
  public abstract test(): Promise<AutoDocConnectionTestResult>;
   * Close database connection
  public abstract close(): Promise<void>;
  // SCHEMA INTROSPECTION
   * Get all schemas with filtered tables
  public abstract getSchemas(
    schemaFilter: AutoDocSchemaFilter,
    tableFilter: AutoDocTableFilter
  ): Promise<AutoDocSchema[]>;
   * Get all tables in a schema
  protected abstract getTables(
  ): Promise<AutoDocTable[]>;
   * Get all columns for a table
  protected abstract getColumns(
  ): Promise<AutoDocColumn[]>;
   * Get existing descriptions from database metadata
   * (e.g., MS_Description for SQL Server, COMMENT for MySQL/PostgreSQL)
  public abstract getExistingDescriptions(
  ): Promise<AutoDocExistingDescription[]>;
  // DATA SAMPLING AND STATISTICS
   * Get column statistics (cardinality, null count, etc.)
  public abstract getColumnStatistics(
    columnName: string,
    dataType: string,
    cardinalityThreshold: number,
    sampleSize: number
  ): Promise<AutoDocColumnStatistics>;
   * Get distinct value count for a column
  protected abstract getDistinctCount(
  ): Promise<number>;
   * Get value distribution for low-cardinality columns
  protected abstract getValueDistribution(
    limit: number
  ): Promise<Array<{ value: any; frequency: number }>>;
   * Get sample values from a column
  protected abstract getSampleValues(
  ): Promise<any[]>;
  // RELATIONSHIP DISCOVERY METHODS
   * Get simplified column statistics for discovery
   * Uses existing getColumnStatistics but with simpler parameters
  public async getColumnStatisticsForDiscovery(
    columnType: string,
    maxSampleSize: number = 1000
  ): Promise<import('../types/discovery.js').ColumnStatistics> {
    const stats = await this.getColumnStatistics(
      columnType,
      100, // cardinalityThreshold
      maxSampleSize
    // Calculate total rows from null percentage if available
    const totalRows = stats.nullCount > 0 && stats.nullPercentage > 0
      ? Math.round(stats.nullCount / stats.nullPercentage)
      : stats.nullCount + stats.distinctCount; // Estimate
      dataType: columnType,
      totalRows,
      minValue: stats.min,
      maxValue: stats.max,
      sampleValues: stats.sampleValues || []
   * Get column information (for FK detection)
  public abstract getColumnInfo(
  ): Promise<{ name: string; type: string; nullable: boolean }>;
   * Test value overlap between two columns (for FK detection)
   * Returns percentage of source values that exist in target (0-1)
  public abstract testValueOverlap(
    sourceTable: string,  // format: "schema.table"
    sourceColumn: string,
    targetTable: string,  // format: "schema.table"
   * Check if a combination of columns is unique (for composite PK detection)
  public abstract checkColumnCombinationUniqueness(
    columnNames: string[],
  ): Promise<boolean>;
  // QUERY EXECUTION
   * Execute a raw SQL query
   * @param query SQL query to execute
   * @param maxRetries Number of retry attempts for transient errors
  public abstract executeQuery<T = any>(
    maxRetries?: number
  ): Promise<AutoDocQueryResult<T>>;
  // PROVIDER-SPECIFIC HELPERS
   * Escape identifier (schema, table, column names)
   * Each provider has different quoting rules
  protected abstract escapeIdentifier(identifier: string): string;
   * Get SQL for limiting result set
   * Different providers use different syntax (TOP, LIMIT, ROWNUM)
  protected abstract getLimitClause(limit: number): string;
   * Check if data type is numeric
  protected isNumericType(dataType: string): boolean {
    const numericTypes = [
      'int', 'integer', 'bigint', 'smallint', 'tinyint',
      'decimal', 'numeric', 'float', 'real', 'double',
      'money', 'smallmoney', 'number'
    return numericTypes.some(type => dataType.toLowerCase().includes(type));
   * Check if data type is date/time
  protected isDateType(dataType: string): boolean {
    const dateTypes = [
      'date', 'time', 'datetime', 'datetime2', 'timestamp',
      'smalldatetime', 'datetimeoffset', 'timestamptz'
    return dateTypes.some(type => dataType.toLowerCase().includes(type));
   * Check if data type is string/text
  protected isStringType(dataType: string): boolean {
    const stringTypes = [
      'char', 'varchar', 'text', 'nchar', 'nvarchar', 'ntext',
      'string', 'clob', 'mediumtext', 'longtext'
    return stringTypes.some(type => dataType.toLowerCase().includes(type));
   * Sleep for specified milliseconds (for retry logic)
   * Build schema filter WHERE clause
   * Helper method for constructing SQL filters
  protected buildSchemaFilterClause(
    filter: AutoDocSchemaFilter,
    schemaColumnName: string
    // If include list is provided, only include those schemas
    // If not provided or empty, include all schemas (except those in exclude list)
    if (filter.include && filter.include.length > 0) {
      const schemaList = filter.include.map(s => `'${s}'`).join(',');
      conditions.push(`${schemaColumnName} IN (${schemaList})`);
    // Always apply exclude list if provided
    if (filter.exclude && filter.exclude.length > 0) {
      const schemaList = filter.exclude.map(s => `'${s}'`).join(',');
      conditions.push(`${schemaColumnName} NOT IN (${schemaList})`);
    return conditions.length > 0 ? `AND ${conditions.join(' AND ')}` : '';
   * Build table filter WHERE clause
  protected buildTableFilterClause(
    filter: AutoDocTableFilter,
    tableColumnName: string
      const tableList = filter.exclude.map(t => `'${t}'`).join(',');
      conditions.push(`${tableColumnName} NOT IN (${tableList})`);
   * Get database provider name
  public getProviderName(): string {
    return this.config.provider;
