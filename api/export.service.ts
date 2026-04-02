export type ExportFormat = 'json' | 'markdown' | 'html' | 'text';
export interface ExportOptions {
  includeMessages?: boolean;
  prettyPrint?: boolean;
  includeCSS?: boolean;
export class ExportService {
  async exportConversation(
    format: ExportFormat,
    options: ExportOptions = {}
    const conversation = await this.loadConversationData(conversationId, currentUser);
    // Apply default options
    const exportOptions: Required<ExportOptions> = {
      includeMessages: options.includeMessages ?? true,
      includeMetadata: options.includeMetadata ?? true,
      prettyPrint: options.prettyPrint ?? true,
      includeCSS: options.includeCSS ?? true
    let filename: string;
    let mimeType: string;
        content = this.exportAsJSON(conversation, exportOptions);
        filename = `conversation-${conversation.conversation.Name}-${this.getTimestamp()}.json`;
        mimeType = 'application/json';
        content = this.exportAsMarkdown(conversation, exportOptions);
        filename = `conversation-${conversation.conversation.Name}-${this.getTimestamp()}.md`;
        mimeType = 'text/markdown';
        content = this.exportAsHTML(conversation, exportOptions);
        filename = `conversation-${conversation.conversation.Name}-${this.getTimestamp()}.html`;
        mimeType = 'text/html';
        content = this.exportAsText(conversation, exportOptions);
        filename = `conversation-${conversation.conversation.Name}-${this.getTimestamp()}.txt`;
        mimeType = 'text/plain';
        throw new Error(`Unsupported export format: ${format}`);
    this.downloadFile(content, filename, mimeType);
  private async loadConversationData(
  ): Promise<{ conversation: MJConversationEntity; details: MJConversationDetailEntity[] }> {
    // Load conversation and details in parallel
    const [conversationResult, detailsResult] = await rv.RunViews([
        ExtraFilter: `ID='${conversationId}'`,
    if (!conversationResult.Success || !conversationResult.Results?.length) {
      conversation: conversationResult.Results[0] as MJConversationEntity,
      details: (detailsResult.Results || []) as MJConversationDetailEntity[]
  private exportAsJSON(
      details: MJConversationDetailEntity[];
    options: Required<ExportOptions>
    const exportData: Record<string, unknown> = {};
    // Add metadata if requested
    if (options.includeMetadata) {
      exportData.conversation = {
        id: data.conversation.ID,
        name: data.conversation.Name,
        description: data.conversation.Description,
        createdAt: data.conversation.__mj_CreatedAt,
        updatedAt: data.conversation.__mj_UpdatedAt
        name: data.conversation.Name
    // Add messages if requested
    if (options.includeMessages) {
      exportData.messages = data.details.map((detail, index) => {
        const message: Record<string, unknown> = {
          message: detail.Message
          message.id = detail.ID;
          message.sequence = index + 1;
          message.timestamp = detail.__mj_CreatedAt;
    // Use pretty print option
    return JSON.stringify(exportData, null, options.prettyPrint ? 2 : 0);
  private exportAsMarkdown(
    let md = `# ${data.conversation.Name}\n\n`;
    if (data.conversation.Description) {
      md += `${data.conversation.Description}\n\n`;
      md += `**Created:** ${this.formatDate(data.conversation.__mj_CreatedAt)}\n\n`;
    md += `---\n\n`;
      for (const detail of data.details) {
        md += `## ${this.capitalizeRole(detail.Role || 'Unknown')}\n\n`;
        md += `${detail.Message}\n\n`;
          md += `*${this.formatDate(detail.__mj_CreatedAt)}*\n\n`;
  private exportAsHTML(
    const styles = options.includeCSS ? `
    body { font-family: system-ui, -apple-system, sans-serif; max-width: 800px; margin: 40px auto; padding: 0 20px; line-height: 1.6; }
    h1 { color: #333; border-bottom: 2px solid #007bff; padding-bottom: 10px; }
    .meta { color: #666; font-size: 14px; margin-bottom: 30px; }
    .message { margin-bottom: 30px; padding: 20px; border-radius: 8px; background: #f5f5f5; }
    .message.user { background: #e3f2fd; }
    .message.assistant { background: #f5f5f5; }
    .role { font-weight: 600; color: #007bff; margin-bottom: 10px; }
    .content { white-space: pre-wrap; }
    .timestamp { color: #999; font-size: 12px; margin-top: 10px; }
  </style>` : '';
    let html = `<!DOCTYPE html>
  <title>${this.escapeHtml(data.conversation.Name || 'Conversation')}</title>${styles}
  <h1>${this.escapeHtml(data.conversation.Name || 'Conversation')}</h1>`;
      html += `
  <div class="meta">
    ${data.conversation.Description ? `<p>${this.escapeHtml(data.conversation.Description)}</p>` : ''}
    <p>Created: ${this.formatDate(data.conversation.__mj_CreatedAt)}</p>
        const roleClass = detail.Role?.toLowerCase() || 'unknown';
  <div class="message ${roleClass}">
    <div class="role">${this.capitalizeRole(detail.Role || 'Unknown')}</div>
    <div class="content">${this.escapeHtml(detail.Message || '')}</div>`;
    <div class="timestamp">${this.formatDate(detail.__mj_CreatedAt)}</div>`;
  private exportAsText(
    const name = data.conversation.Name || 'Conversation';
    let text = `${name}\n`;
    text += '='.repeat(name.length) + '\n\n';
      text += `${data.conversation.Description}\n\n`;
      text += `Created: ${this.formatDate(data.conversation.__mj_CreatedAt)}\n\n`;
    text += '-'.repeat(80) + '\n\n';
        text += `[${this.capitalizeRole(detail.Role || 'Unknown')}]\n`;
        text += `${detail.Message}\n`;
          text += `(${this.formatDate(detail.__mj_CreatedAt)})\n`;
        text += '\n' + '-'.repeat(80) + '\n\n';
  private downloadFile(content: string, filename: string, mimeType: string): void {
    const blob = new Blob([content], { type: mimeType });
  private formatDate(date: Date | string): string {
    return d.toLocaleString('en-US', {
  private capitalizeRole(role: string): string {
    return role.charAt(0).toUpperCase() + role.slice(1).toLowerCase();
  private getTimestamp(): string {
    return now.toISOString().replace(/[:.]/g, '-').slice(0, -5);
  SamplingOptions
 * Configuration for the export dialog
export interface ExportDialogConfig {
  /** Data to export */
  data: ExportData;
  /** Columns available for export (derived from data if not provided) */
  columns?: ExportColumn[];
  /** Default file name (without extension) */
  defaultFileName?: string;
  /** Available formats to show in dialog */
  availableFormats?: ExportFormat[];
  /** Default format selection */
  defaultFormat?: ExportFormat;
  /** Whether to show sampling options */
  showSamplingOptions?: boolean;
  /** Default sampling mode */
  defaultSamplingMode?: SamplingMode;
  /** Default sample count */
  defaultSampleCount?: number;
  dialogTitle?: string;
 * Result from export dialog
export interface ExportDialogResult {
  /** Whether the user proceeded with export */
  exported: boolean;
  /** The export result if exported */
  result?: ExportResult;
  /** Options used for export */
  options?: ExportOptions;
 * Angular service for data export functionality
 * Wraps the @memberjunction/export-engine for Angular usage
   * Export data directly without dialog
   * @param data Data to export
   * @param options Export options
   * @returns Export result with buffer and metadata
  async export(data: ExportData, options: Partial<ExportOptions> = {}): Promise<ExportResult> {
    return ExportEngine.export(data, options);
   * Export to Excel format
  async toExcel(data: ExportData, options: Omit<Partial<ExportOptions>, 'format'> = {}): Promise<ExportResult> {
    return ExportEngine.toExcel(data, options);
   * Export to CSV format
  async toCSV(data: ExportData, options: Omit<Partial<ExportOptions>, 'format'> = {}): Promise<ExportResult> {
    return ExportEngine.toCSV(data, options);
   * Export to JSON format
  async toJSON(data: ExportData, options: Omit<Partial<ExportOptions>, 'format'> = {}): Promise<ExportResult> {
    return ExportEngine.toJSON(data, options);
   * Get supported export formats
  getSupportedFormats(): ExportFormat[] {
    return ExportEngine.getSupportedFormats();
   * Download the export result as a file
   * @param result Export result containing the data
  downloadResult(result: ExportResult): void {
      throw new Error(result.error || 'Export failed - no data to download');
    const blob = new Blob([result.data as BlobPart], { type: result.mimeType });
    link.download = result.fileName || 'export';
   * Export and immediately download
  async exportAndDownload(data: ExportData, options: Partial<ExportOptions> = {}): Promise<ExportResult> {
    const result = await this.export(data, options);
      this.downloadResult(result);
   * Get available sampling modes with display labels
  getSamplingModes(): { mode: SamplingMode; label: string; description: string }[] {
      { mode: 'all', label: 'All Rows', description: 'Export all data rows' },
      { mode: 'top', label: 'Top N', description: 'Export the first N rows' },
      { mode: 'bottom', label: 'Bottom N', description: 'Export the last N rows' },
      { mode: 'every-nth', label: 'Every Nth', description: 'Export every Nth row' },
      { mode: 'random', label: 'Random N', description: 'Export N random rows' }
   * Get format display info
  getFormatInfo(format: ExportFormat): { label: string; icon: string; description: string } {
          label: 'Excel',
          icon: 'fa-file-excel',
          description: 'Microsoft Excel spreadsheet (.xlsx)'
          label: 'CSV',
          icon: 'fa-file-csv',
          description: 'Comma-separated values (.csv)'
          label: 'JSON',
          description: 'JavaScript Object Notation (.json)'
   * Build sampling options from user selections
  buildSamplingOptions(mode: SamplingMode, count?: number, interval?: number): SamplingOptions {
    const options: SamplingOptions = { mode };
    if (mode === 'top' || mode === 'bottom' || mode === 'random') {
      options.count = count || 100;
    } else if (mode === 'every-nth') {
      options.interval = interval || 10;
