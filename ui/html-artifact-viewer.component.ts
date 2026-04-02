 * Viewer component for HTML artifacts
  selector: 'mj-html-artifact-viewer',
    <div class="html-artifact-viewer" [ngClass]="cssClass">
      <div class="html-toolbar">
        <button class="btn-icon" [class.active]="viewMode === 'preview'"
                title="Preview" (click)="viewMode = 'preview'">
          <i class="fas fa-eye"></i> Preview
        <button class="btn-icon" [class.active]="viewMode === 'source'"
                title="Source" (click)="viewMode = 'source'">
          <i class="fas fa-code"></i> Source
        <button class="btn-icon" title="Copy HTML" (click)="onCopy()">
      <div class="html-content-container">
        @if (viewMode === 'preview') {
          <div class="html-preview">
            <div [innerHTML]="safeHtmlContent"></div>
            [(ngModel)]="htmlContent"
            [language]="'html'"
    .html-artifact-viewer {
    .html-toolbar {
      border-color: #007acc;
    .html-content-container {
    .html-preview {
@RegisterClass(BaseArtifactViewerPluginComponent, 'HtmlArtifactViewerPlugin')
export class HtmlArtifactViewerComponent extends BaseArtifactViewerPluginComponent {
  public htmlContent = '';
  public safeHtmlContent: SafeHtml = '';
  public viewMode: 'preview' | 'source' = 'preview';
   * HTML artifacts always have content to display
  constructor(private sanitizer: DomSanitizer) {
    // Get content and clean up double-escaped characters that can appear in LLM-generated HTML
    // These appear as literal "\\n", "\\t", "\\\"" in the string and cause rendering issues
    let content = this.getContent();
    // Remove escaped quotes (\" becomes ")
    content = content.replace(/\\"/g, '"');
    // Remove double-escaped newlines (\\n becomes nothing)
    content = content.replace(/\\n/g, '');
    content = content.replace(/\\\\n/g, '');
    // Also remove double-escaped tabs if present
    content = content.replace(/\\t/g, '');
    content = content.replace(/\\\\t/g, '');
    this.htmlContent = content;
    this.safeHtmlContent = this.sanitizer.sanitize(1, this.htmlContent) || '';
    if (this.htmlContent) {
      navigator.clipboard.writeText(this.htmlContent).then(() => {
        console.log('✅ Copied HTML to clipboard');
