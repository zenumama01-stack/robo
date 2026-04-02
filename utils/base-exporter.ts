import { ExportOptions, ExportResult, ExportColumn, ExportData, ExportDataRow, DEFAULT_EXPORT_OPTIONS } from './types';
 * Base class for all exporters
 * Provides common functionality for data processing, sampling, and column handling
export abstract class BaseExporter {
  protected options: ExportOptions;
  constructor(options: Partial<ExportOptions> = {}) {
    this.options = { ...DEFAULT_EXPORT_OPTIONS, ...options } as ExportOptions;
   * Export data to the target format
   * @param data Array of data objects or arrays to export
  abstract export(data: ExportData): Promise<ExportResult>;
   * Get the MIME type for this export format
  abstract getMimeType(): string;
   * Get the file extension for this export format
  abstract getFileExtension(): string;
   * Derive columns from data if not explicitly provided
  protected deriveColumns(data: ExportData): ExportColumn[] {
    if (this.options.columns && this.options.columns.length > 0) {
      return this.options.columns;
    if (Array.isArray(firstRow)) {
      // Array of arrays - create generic column names
      return firstRow.map((_, index) => ({
        name: `Column${index + 1}`,
        displayName: `Column ${index + 1}`
      // Array of objects - use keys as column names
      return Object.keys(firstRow as Record<string, unknown>).map(key => ({
        displayName: this.formatColumnName(key)
   * Format a column name for display (e.g., "firstName" -> "First Name")
  protected formatColumnName(name: string): string {
    // Handle camelCase and PascalCase
    const withSpaces = name.replace(/([A-Z])/g, ' $1').trim();
    // Handle snake_case
    const withoutUnderscores = withSpaces.replace(/_/g, ' ');
    // Capitalize first letter of each word
    return withoutUnderscores.replace(/\b\w/g, char => char.toUpperCase());
   * Apply sampling to data based on options
  protected applySampling(data: ExportData): ExportData {
    const sampling = this.options.sampling || { mode: 'all' };
    switch (sampling.mode) {
        return this.sampleTop(data, sampling.count || 100);
        return this.sampleBottom(data, sampling.count || 100);
        return this.sampleEveryNth(data, sampling.interval || 10);
        return this.sampleRandom(data, sampling.count || 100);
   * Get top N rows
  private sampleTop(data: ExportData, count: number): ExportData {
    return data.slice(0, count);
   * Get bottom N rows
  private sampleBottom(data: ExportData, count: number): ExportData {
    return data.slice(-count);
   * Get every Nth row
  private sampleEveryNth(data: ExportData, interval: number): ExportData {
    const result: ExportData = [];
    for (let i = 0; i < data.length; i += interval) {
      result.push(data[i]);
   * Get random N rows
  private sampleRandom(data: ExportData, count: number): ExportData {
    if (data.length <= count) {
    // Fisher-Yates shuffle on indices, then take first N
    const indices = Array.from({ length: data.length }, (_, i) => i);
    for (let i = indices.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [indices[i], indices[j]] = [indices[j], indices[i]];
    return indices.slice(0, count).sort((a, b) => a - b).map(i => data[i]);
   * Extract row values in column order
  protected extractRowValues(row: ExportDataRow, columns: ExportColumn[]): unknown[] {
    if (Array.isArray(row)) {
    return columns.map(col => {
      const value = row[col.name];
      return this.formatValue(value, col);
   * Format a cell value based on column type
  protected formatValue(value: unknown, column: ExportColumn): unknown {
    switch (column.dataType) {
          return isNaN(date.getTime()) ? value : date;
        return typeof value === 'boolean' ? value : Boolean(value);
        return isNaN(num) ? value : num;
   * Generate the full file name with extension
  protected getFullFileName(): string {
    const baseName = this.options.fileName || 'export';
    return `${baseName}.${this.getFileExtension()}`;
