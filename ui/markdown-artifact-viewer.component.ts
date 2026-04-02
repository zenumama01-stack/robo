 * Viewer component for Markdown artifacts
  selector: 'mj-markdown-artifact-viewer',
    <div class="markdown-artifact-viewer" [ngClass]="cssClass">
      <div class="markdown-toolbar">
        <button class="btn-icon" title="Copy Markdown" (click)="onCopy()">
      <div class="markdown-content-container">
          <div class="markdown-preview">
            <mj-markdown [data]="markdownContent"
            [(ngModel)]="markdownContent"
    .markdown-artifact-viewer {
    .markdown-toolbar {
    .markdown-content-container {
    .markdown-preview {
    .markdown-preview ::ng-deep h1 {
      margin-top: 0.67em;
      margin-bottom: 0.67em;
      border-bottom: 1px solid #eaecef;
    .markdown-preview ::ng-deep h2 {
      margin-top: 0.83em;
      margin-bottom: 0.83em;
    .markdown-preview ::ng-deep code {
      background: #f6f8fa;
    .markdown-preview ::ng-deep pre {
    .markdown-preview ::ng-deep blockquote {
      border-left: 4px solid #dfe2e5;
      color: #6a737d;
    .markdown-preview ::ng-deep table {
    .markdown-preview ::ng-deep th,
    .markdown-preview ::ng-deep td {
      border: 1px solid #dfe2e5;
    .markdown-preview ::ng-deep th {
@RegisterClass(BaseArtifactViewerPluginComponent, 'MarkdownArtifactViewerPlugin')
export class MarkdownArtifactViewerComponent extends BaseArtifactViewerPluginComponent {
  public markdownContent = '';
   * Markdown artifacts always have content to display
    this.markdownContent = this.getContent();
    if (this.markdownContent) {
      navigator.clipboard.writeText(this.markdownContent).then(() => {
        console.log('✅ Copied markdown to clipboard');
