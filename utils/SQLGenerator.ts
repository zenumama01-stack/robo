 * Generates SQL scripts with sp_addextendedproperty statements
export interface SQLGeneratorOptions {
export class SQLGenerator {
   * Generate SQL script
    options: SQLGeneratorOptions = {}
    lines.push('-- Database Documentation Script');
    lines.push(`-- Generated: ${new Date().toISOString()}`);
    lines.push(`-- Database: ${state.database.name}`);
    lines.push(`-- Server: ${state.database.server}`);
    lines.push('-- This script adds MS_Description extended properties to database objects');
    // Generate statements for each schema
      lines.push(`-- Schema: ${schema.name}`);
      // Schema description
        lines.push(this.generateSchemaDescription(schema.name, schema.description));
        lines.push('GO');
      // Table descriptions
        // Table description
          lines.push(`-- Table: ${schema.name}.${table.name}`);
          lines.push(this.generateTableDescription(schema.name, table.name, table.description));
        // Column descriptions
          if (column.description) {
              this.generateColumnDescription(
                column.description
   * Generate schema description statement
  private generateSchemaDescription(schemaName: string, description: string): string {
    const escapedDescription = this.escapeString(description);
    SELECT 1 FROM sys.extended_properties
    WHERE major_id = SCHEMA_ID('${schemaName}')
    AND name = 'MS_Description'
    AND minor_id = 0
    EXEC sp_dropextendedproperty
        @name = N'MS_Description',
        @level0type = N'SCHEMA',
        @level0name = N'${schemaName}';
EXEC sp_addextendedproperty
    @value = N'${escapedDescription}',
   * Generate table description statement
  private generateTableDescription(
    SELECT 1 FROM sys.extended_properties ep
    AND ep.name = 'MS_Description'
    AND ep.minor_id = 0
        @level0name = N'${schemaName}',
        @level1type = N'TABLE',
        @level1name = N'${tableName}';
   * Generate column description statement
  private generateColumnDescription(
    INNER JOIN sys.columns c ON ep.major_id = c.object_id AND ep.minor_id = c.column_id
        @level1name = N'${tableName}',
        @level2type = N'COLUMN',
        @level2name = N'${columnName}';
   * Escape string for SQL
  private escapeString(str: string): string {
