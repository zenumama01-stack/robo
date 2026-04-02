import { ExportService, ExportFormat, ExportOptions } from '../../services/export.service';
  selector: 'mj-export-modal',
        [title]="exportTitle"
        <div class="export-modal-content">
          <section class="format-section">
              Export Format
            <p class="section-description">
              Choose a format to export this conversation:
            <div class="format-options">
              @for (format of formats; track format.value) {
                  class="format-option"
                  [class.selected]="selectedFormat === format.value"
                  (click)="selectFormat(format.value)">
                  <i [class]="format.icon"></i>
                  <div class="format-details">
                    <div class="format-name">{{ format.name }}</div>
                    <div class="format-description">{{ format.description }}</div>
                  @if (selectedFormat === format.value) {
                    <i class="fa-solid fa-check-circle check-icon"></i>
          <section class="options-section">
              Export Options
            <div class="option-checkboxes">
                  [(ngModel)]="exportOptions.includeMessages"
                  [disabled]="isExporting">
                <span>Include messages</span>
                <small>Export all conversation messages</small>
                  [(ngModel)]="exportOptions.includeMetadata"
                <span>Include metadata</span>
                <small>Add creation date, IDs, and other metadata</small>
            @if (selectedFormat === 'json') {
              <div class="format-specific-options">
                <h4 class="subsection-title">JSON Options</h4>
                    [(ngModel)]="exportOptions.prettyPrint"
                  <span>Pretty print</span>
                  <small>Format JSON with indentation</small>
            @if (selectedFormat === 'html') {
                <h4 class="subsection-title">HTML Options</h4>
                    [(ngModel)]="exportOptions.includeCSS"
                  <span>Include CSS styling</span>
                  <small>Embed styles for better presentation</small>
            <div class="loading-indicator">
              <mj-loading text="Exporting conversation..." size="small"></mj-loading>
          <button kendoButton [disabled]="isExporting" (click)="onCancel()">
          <button kendoButton [primary]="true" [disabled]="!canExport" (click)="onExport()">
    .export-modal-content {
      max-height: calc(600px - 120px);
    section {
    .format-options {
    .format-option {
    .format-option:hover {
    .format-option.selected {
    .format-option > i.fa-solid,
    .format-option > i.fas {
    .format-option.selected > i.fa-solid,
    .format-option.selected > i.fas {
    .format-details {
    .format-name {
    .format-description {
    .option-checkboxes {
    .checkbox-label > span {
    .checkbox-label input[type="checkbox"]:disabled {
    .checkbox-label small {
    .format-specific-options {
      border-left: 3px solid #d32f2f;
    kendo-dialog-actions {
      padding: 15px 20px;
    kendo-dialog-actions button i {
export class ExportModalComponent {
  @Input() conversation?: MJConversationEntity;
  @Output() exported = new EventEmitter<void>();
  selectedFormat: ExportFormat | null = 'markdown';
  isExporting = false;
  exportOptions: ExportOptions = {
    includeMessages: true,
    includeCSS: true
  get exportTitle(): string {
    return `Export: ${this.conversation?.Name || 'Conversation'}`;
  get canExport(): boolean {
    return !this.isExporting &&
           !!this.selectedFormat &&
           (this.exportOptions.includeMessages === true);
  formats = [
      value: 'markdown' as ExportFormat,
      name: 'Markdown',
      description: 'Formatted text with markdown syntax',
      value: 'json' as ExportFormat,
      name: 'JSON',
      description: 'Structured data format',
      icon: 'fa-solid fa-code'
      value: 'html' as ExportFormat,
      name: 'HTML',
      description: 'Web page format',
      icon: 'fa-solid fa-file-code'
      value: 'text' as ExportFormat,
      name: 'Plain Text',
      description: 'Simple text file',
      icon: 'fa-solid fa-file'
    private toastService: ToastService
  selectFormat(format: ExportFormat): void {
    if (!this.isExporting) {
      this.selectedFormat = format;
  async onExport(): Promise<void> {
    if (!this.canExport || !this.conversation) {
    if (!this.exportOptions.includeMessages) {
      this.errorMessage = 'At least "Include messages" must be selected';
      await this.exportService.exportConversation(
        this.selectedFormat!,
        this.currentUser,
        this.exportOptions
      this.toastService.success('Conversation exported successfully');
      this.exported.emit();
      console.error('Error exporting conversation:', error);
      this.errorMessage = error instanceof Error ? error.message : 'Failed to export conversation';
    this.selectedFormat = 'markdown';
    this.exportOptions = {
