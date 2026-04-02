 * JSON exporter - exports data as formatted JSON
 * Lightweight, no external dependencies
export class JSONExporter extends BaseExporter {
  private prettyPrint: boolean;
  private indent: number;
  constructor(options: Partial<ExportOptions> & { prettyPrint?: boolean; indent?: number } = {}) {
    super({ ...options, format: 'json' });
    this.prettyPrint = options.prettyPrint !== false;
    this.indent = options.indent ?? 2;
    return 'application/json;charset=utf-8';
      // Derive columns for potential filtering
      // If columns specified, filter data to only include those columns
      let outputData: unknown[];
      if (this.options.columns && this.options.columns.length > 0 && !Array.isArray(sampledData[0])) {
        outputData = (sampledData as Record<string, unknown>[]).map(row => {
          const filtered: Record<string, unknown> = {};
            filtered[col.displayName || col.name] = row[col.name];
        outputData = sampledData;
      // Serialize to JSON
      const jsonString = this.prettyPrint
        ? JSON.stringify(outputData, null, this.indent)
        : JSON.stringify(outputData);
      // Convert to bytes
      const bytes = encoder.encode(jsonString);
