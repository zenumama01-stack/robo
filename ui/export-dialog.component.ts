  ExportFormat,
  ExportData,
  SamplingMode,
  ExportResult
import { ExportService, ExportDialogConfig, ExportDialogResult } from './export.service';
 * Export dialog component with progressive UX
 * Provides format selection, sampling options, and export preview
 * <mj-export-dialog
 *   [visible]="showExportDialog"
 *   [config]="exportConfig"
 *   (closed)="onExportDialogClosed($event)">
 * </mj-export-dialog>
  selector: 'mj-export-dialog',
  templateUrl: './export-dialog.component.html',
  styleUrls: ['./export-dialog.component.css']
export class ExportDialogComponent {
  selectedFormat: ExportFormat = 'excel';
  fileName = 'export';
  includeHeaders = true;
  samplingMode: SamplingMode = 'all';
  sampleCount = 100;
  sampleInterval = 10;
  exportError: string | null = null;
  availableFormats: ExportFormat[] = ['excel', 'csv', 'json'];
  samplingModes: { mode: SamplingMode; label: string; description: string }[];
    this.samplingModes = this.exportService.getSamplingModes();
    if (value && !this._visible) {
      this.initializeFromConfig();
  @Input() config: ExportDialogConfig | null = null;
  @Output() closed = new EventEmitter<ExportDialogResult>();
   * Initialize form from config
  private initializeFromConfig(): void {
    this.selectedFormat = this.config.defaultFormat || 'excel';
    this.fileName = this.config.defaultFileName || 'export';
    this.samplingMode = this.config.defaultSamplingMode || 'all';
    this.sampleCount = this.config.defaultSampleCount || 100;
    this.availableFormats = this.config.availableFormats || ['excel', 'csv', 'json'];
    this.exportError = null;
   * Get format info for display
  getFormatInfo(format: ExportFormat) {
    return this.exportService.getFormatInfo(format);
   * Check if sampling needs a count input
  get needsSampleCount(): boolean {
    return this.samplingMode === 'top' || this.samplingMode === 'bottom' || this.samplingMode === 'random';
   * Check if sampling needs an interval input
  get needsSampleInterval(): boolean {
    return this.samplingMode === 'every-nth';
   * Get total row count
  get totalRows(): number {
    return this.config?.data?.length || 0;
   * Estimate exported row count
  get estimatedRows(): number {
    const total = this.totalRows;
    switch (this.samplingMode) {
      case 'top':
      case 'bottom':
      case 'random':
        return Math.min(this.sampleCount, total);
      case 'every-nth':
        return Math.ceil(total / this.sampleInterval);
   * Get sampling description
  get samplingDescription(): string {
        return `Exporting all ${this.totalRows.toLocaleString()} rows`;
        return `Exporting first ${Math.min(this.sampleCount, this.totalRows).toLocaleString()} rows`;
        return `Exporting last ${Math.min(this.sampleCount, this.totalRows).toLocaleString()} rows`;
        return `Exporting ${Math.min(this.sampleCount, this.totalRows).toLocaleString()} random rows`;
        return `Exporting every ${this.sampleInterval}${this.getOrdinalSuffix(this.sampleInterval)} row (~${this.estimatedRows.toLocaleString()} rows)`;
   * Get ordinal suffix for number
  private getOrdinalSuffix(n: number): string {
    const s = ['th', 'st', 'nd', 'rd'];
    const v = n % 100;
    return s[(v - 20) % 10] || s[v] || s[0];
   * Handle format selection
   * Handle cancel button
    this._visible = false;
    this.closed.emit({ exported: false });
   * Handle export button
    if (!this.config?.data) {
      this.exportError = 'No data to export';
      const options: Partial<ExportOptions> = {
        format: this.selectedFormat,
        fileName: this.fileName,
        includeHeaders: this.includeHeaders,
        sampling: this.exportService.buildSamplingOptions(
          this.samplingMode,
          this.sampleCount,
          this.sampleInterval
      const result = await this.exportService.export(this.config.data, options);
        this.closed.emit({
          exported: true,
          options: options as ExportOptions
        this.exportError = result.error || 'Export failed';
      this.exportError = error instanceof Error ? error.message : 'Export failed';
   * Check if show sampling options
  get showSamplingOptions(): boolean {
    return this.config?.showSamplingOptions !== false;
   * Get dialog title
    return this.config?.dialogTitle || 'Export Data';
