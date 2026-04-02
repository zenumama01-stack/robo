 * @fileoverview SQL Logger for capturing database operations during metadata sync
 * @module sql-logger
 * This module provides SQL logging functionality to capture all database operations
 * during push commands. It supports both raw SQL logging and migration-formatted output.
import { SyncConfig } from '../config';
export interface SQLLoggerOptions {
  outputDirectory: string;
export class SQLLogger {
  private options: SQLLoggerOptions;
  private statements: string[] = [];
  constructor(syncConfig: SyncConfig | null) {
      enabled: syncConfig?.sqlLogging?.enabled ?? false,
      outputDirectory: syncConfig?.sqlLogging?.outputDirectory ?? './sql_logging',
      formatAsMigration: syncConfig?.sqlLogging?.formatAsMigration ?? false
  get enabled(): boolean {
    return this.options.enabled;
   * Initialize the SQL logger and prepare output directory
    if (!this.options.enabled || this.isInitialized) {
    await fs.ensureDir(this.options.outputDirectory);
   * Log a SQL statement
  logStatement(sql: string, params?: any[]): void {
    if (!this.options.enabled) {
    // Format SQL with parameters inline for readability
    let formattedSql = sql;
      // Replace parameter placeholders with actual values
      params.forEach((param, index) => {
        const placeholder = `@param${index + 1}`;
        const value = this.formatParamValue(param);
        formattedSql = formattedSql.replace(new RegExp(placeholder, 'g'), value);
    this.statements.push(formattedSql);
   * Log a transaction boundary
  logTransaction(action: 'BEGIN' | 'COMMIT' | 'ROLLBACK'): void {
    this.statements.push(`${action} TRANSACTION;`);
   * Write the collected SQL statements to file
  async writeLog(): Promise<string | undefined> {
    if (!this.options.enabled || this.statements.length === 0) {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    if (this.options.formatAsMigration) {
      // Format as Flyway migration
      const migrationTimestamp = timestamp.replace(/[-T]/g, '').substring(0, 14);
      filename = `V${migrationTimestamp}__MetadataSync_Push.sql`;
      content = [
        '-- MemberJunction MetadataSync Push Migration',
        `-- Generated at: ${new Date().toISOString()}`,
        '-- Description: Metadata changes pushed via mj sync push command',
        '-- Note: Schema placeholders can be replaced during deployment',
        '-- Replace ${flyway:defaultSchema} with your target schema name',
        ...this.statements.map(stmt => {
          // Add schema placeholders for migration format
          return stmt.replace(/(\[?)__mj(\]?)\./g, '${flyway:defaultSchema}.');
      // Regular SQL log format
      filename = `metadatasync-push-${timestamp}.sql`;
        '-- MemberJunction MetadataSync SQL Log',
        `-- Total statements: ${this.statements.length}`,
        ...this.statements
    const filePath = path.join(this.options.outputDirectory, filename);
    await fs.writeFile(filePath, content, 'utf8');
   * Clear all logged statements
    this.statements = [];
   * Format a parameter value for SQL
  private formatParamValue(value: any): string {
      // Escape single quotes and wrap in quotes
      return `'${value.replace(/'/g, "''")}'`;
      return `'${value.toISOString()}'`;
