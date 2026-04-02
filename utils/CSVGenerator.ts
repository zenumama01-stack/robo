 * Generates CSV exports for tables and columns
export interface CSVGeneratorOptions {
export interface CSVExport {
  tables: string;
  columns: string;
export class CSVGenerator {
   * Generate CSV exports (returns both tables and columns CSV)
  public generate(
    options: CSVGeneratorOptions = {}
  ): CSVExport {
      tables: this.generateTablesCsv(state, options),
      columns: this.generateColumnsCsv(state, options)
   * Generate tables CSV
  private generateTablesCsv(
    options: CSVGeneratorOptions
    lines.push(this.csvLine([
      'Schema',
      'Table',
      'Row Count',
      'Dependency Level',
      'Confidence %',
      'User Approved',
      'Business Domains',
      'Inferred Purpose'
    ]));
        // Check filters
        if (options.approvedOnly && !table.userApproved) {
        if (options.confidenceThreshold && table.descriptionIterations.length > 0) {
          if ((latest.confidence || 0) < options.confidenceThreshold) {
        // Get confidence
        let confidence = '';
          if (latest.confidence) {
            confidence = (latest.confidence * 100).toFixed(0);
          table.description || '',
          table.rowCount.toString(),
          table.dependencyLevel !== undefined ? table.dependencyLevel.toString() : '',
          table.userApproved ? 'Yes' : 'No',
          (table as any).businessDomains ? (table as any).businessDomains.join('; ') : '',
          (table as any).inferredPurpose || ''
   * Generate columns CSV
  private generateColumnsCsv(
      'Column',
      'Data Type',
      'Is Nullable',
      'Is Primary Key',
      'Is Foreign Key',
      'Foreign Key References',
      'Check Constraint',
      'Default Value'
        // Check table-level filters
          if (column.descriptionIterations && column.descriptionIterations.length > 0) {
            const latest = column.descriptionIterations[column.descriptionIterations.length - 1];
          // Get foreign key reference
          let fkReference = '';
          if (column.foreignKeyReferences) {
            fkReference = `${column.foreignKeyReferences.schema}.${column.foreignKeyReferences.table}.${column.foreignKeyReferences.column}`;
            column.isNullable ? 'Yes' : 'No',
            column.isPrimaryKey ? 'Yes' : 'No',
            column.isForeignKey ? 'Yes' : 'No',
            fkReference,
            column.description || '',
            column.checkConstraint || '',
            column.defaultValue || ''
   * Escape and format a CSV line
  private csvLine(values: string[]): string {
    return values.map(value => this.escapeCsvField(value)).join(',');
   * Escape a CSV field value
  private escapeCsvField(value: string): string {
    // If field contains comma, newline, or quotes, wrap in quotes and escape quotes
    if (value.includes(',') || value.includes('\n') || value.includes('"')) {
