 * Export dialog configuration
  /** Total number of rows available for export */
  totalRows: number;
  /** Column definitions for the data */
  columns?: ExportColumnInfo[];
  /** Whether to show advanced options by default */
  showAdvancedOptions?: boolean;
  /** Default file name */
 * Column information for export
export interface ExportColumnInfo {
  dataType?: 'string' | 'number' | 'date' | 'boolean' | 'currency';
 * Result from the export dialog
  /** Whether the user confirmed the export */
  confirmed: boolean;
  /** Selected export format */
  format?: 'excel' | 'csv' | 'json';
  /** Whether to include headers */
  /** Row sampling configuration */
  sampling?: {
    mode: 'all' | 'top' | 'bottom' | 'every-nth' | 'random';
    interval?: number;
  /** Selected columns (if column selection was enabled) */
  selectedColumns?: string[];
  /** Custom file name */
 * Options for the export service
export interface ExportServiceOptions {
  data: Record<string, unknown>[];
  /** Export format */
  format: 'excel' | 'csv' | 'json';
  /** File name without extension */
  /** Include column headers */
  /** Row sampling options */
  /** Columns to export */
