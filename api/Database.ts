 * Database connection and introspection classes
 * Primary interface for all database operations
import { BaseAutoDocDriver } from '../drivers/BaseAutoDocDriver.js';
import { SchemaDefinition, TableDefinition, ColumnDefinition, ForeignKeyReference } from '../types/state.js';
import { SchemaFilterConfig, TableFilterConfig, AnalysisConfig } from '../types/config.js';
import '../drivers/SQLServerDriver.js'; // Import to ensure registration
import '../drivers/MySQLDriver.js'; // Import to ensure registration
import '../drivers/PostgreSQLDriver.js'; // Import to ensure registration
 * Create a database driver instance
export function createDriver(config: AutoDocConnectionConfig): BaseAutoDocDriver {
  const providerKey = config.provider === 'sqlserver' || !config.provider ? 'SQLServer' :
                      config.provider === 'mysql' ? 'MySQL' :
                      config.provider === 'postgresql' ? 'PostgreSQL' :
                      config.provider === 'oracle' ? 'Oracle' : 'SQLServer';
  const driver = MJGlobal.Instance.ClassFactory.CreateInstance<BaseAutoDocDriver>(
    BaseAutoDocDriver,
    providerKey,
  if (!driver) {
    throw new Error(`Database provider '${providerKey}' is not registered`);
  return driver;
 * Database connection class
 * Provides connection management and query execution
export class DatabaseConnection {
  private driver: BaseAutoDocDriver;
  constructor(dbConfig: AutoDocConnectionConfig) {
    this.driver = createDriver(dbConfig);
  public async connect(): Promise<void> {
    await this.driver.connect();
  public async test(): Promise<{ success: boolean; message: string }> {
    return await this.driver.test();
  public async query<T = any>(
  ): Promise<{ success: boolean; data?: T[]; errorMessage?: string }> {
    return await this.driver.executeQuery<T>(queryText, maxRetries);
  public async close(): Promise<void> {
    await this.driver.close();
  // Expose the underlying driver for advanced usage
  public getDriver(): BaseAutoDocDriver {
    return this.driver;
 * Database introspector
 * Retrieves schema and table information from the database
export class Introspector {
  constructor(private driver: BaseAutoDocDriver) {}
  public async getSchemas(
    schemaFilter: SchemaFilterConfig,
    tableFilter: TableFilterConfig
  ): Promise<SchemaDefinition[]> {
    const autoDocSchemas = await this.driver.getSchemas(schemaFilter, tableFilter);
    // Convert AutoDocSchema[] to SchemaDefinition[]
    const schemas: SchemaDefinition[] = [];
    for (const autoDocSchema of autoDocSchemas) {
      const tables: TableDefinition[] = [];
      for (const autoDocTable of autoDocSchema.tables) {
        // Convert foreign keys to dependsOn/dependents format
        const dependsOn: ForeignKeyReference[] = autoDocTable.foreignKeys.map(fk => ({
          schema: fk.referencedSchema,
          table: fk.referencedTable,
          column: fk.columnName,
          referencedColumn: fk.referencedColumn
        // Convert columns
        const columns: ColumnDefinition[] = autoDocTable.columns.map(col => ({
        tables.push({
          name: autoDocTable.tableName,
          rowCount: autoDocTable.rowCount,
          dependents: [], // Will be populated later
      schemas.push({
        name: autoDocSchema.name,
    // Populate dependents (reverse FKs)
        for (const dep of table.dependsOn) {
          const parentSchema = schemas.find(s => s.name === dep.schema);
          if (parentSchema) {
            const parentTable = parentSchema.tables.find(t => t.name === dep.table);
            if (parentTable) {
              if (!parentTable.dependents.some(d => d.schema === schema.name && d.table === table.name)) {
                parentTable.dependents.push({
                  column: dep.referencedColumn,
                  referencedColumn: dep.column
  public async getExistingDescriptions(
  ): Promise<Map<string, string>> {
    const descriptions = await this.driver.getExistingDescriptions(schemaName, tableName);
    for (const desc of descriptions) {
      const key = desc.target === 'table' ? '__TABLE__' : desc.targetName;
      map.set(key, desc.description);
 * Data sampler
 * Analyzes table data and gathers column statistics
export class DataSampler {
    private driver: BaseAutoDocDriver,
    private config: AnalysisConfig
  public async analyzeTable(
    columns: ColumnDefinition[]
    let columnErrors = 0;
    for (const column of columns) {
        const stats = await this.driver.getColumnStatistics(
          column.name,
          column.dataType,
          this.config.cardinalityThreshold,
          this.config.sampleSize
        // Convert AutoDocColumnStatistics to ColumnStatistics
        column.statistics = {
          totalRows: stats.totalRows,
          distinctCount: stats.distinctCount,
          uniquenessRatio: stats.uniquenessRatio,
          nullCount: stats.nullCount,
          nullPercentage: stats.nullPercentage,
          sampleValues: stats.sampleValues,
          min: stats.min,
          max: stats.max,
          avg: stats.avg,
          stdDev: stats.stdDev,
          avgLength: stats.avgLength,
          maxLength: stats.maxLength,
          minLength: stats.minLength
        // Set possible values if low cardinality
        if (stats.valueDistribution && stats.valueDistribution.length > 0) {
          column.possibleValues = stats.valueDistribution.map(v => v.value);
        columnErrors++;
        console.error(
          `  Failed to get statistics for ${schemaName}.${tableName}.${column.name}: ${(error as Error).message}`
        // Leave column without statistics - table analysis will continue
        // AI can still generate descriptions based on column name and type
    if (columnErrors > 0) {
        `  Warning: ${columnErrors} column(s) in ${schemaName}.${tableName} failed statistics gathering`
