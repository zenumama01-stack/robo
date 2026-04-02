 * Viewer component for SVG artifacts
  selector: 'mj-svg-artifact-viewer',
    <div class="svg-artifact-viewer" [ngClass]="cssClass">
      <div class="svg-toolbar">
        <button class="btn-icon" title="Copy SVG" (click)="onCopy()">
      <div class="svg-content-container">
          <div class="svg-preview">
            <div class="svg-wrapper" [innerHTML]="safeSvgContent"></div>
            [(ngModel)]="svgContent"
            [language]="'xml'"
    .svg-artifact-viewer {
    .svg-toolbar {
    .svg-content-container {
    .svg-preview {
    .svg-wrapper {
    .svg-wrapper ::ng-deep svg {
@RegisterClass(BaseArtifactViewerPluginComponent, 'SvgArtifactViewerPlugin')
export class SvgArtifactViewerComponent extends BaseArtifactViewerPluginComponent {
  public svgContent = '';
  public safeSvgContent: SafeHtml = '';
   * SVG artifacts always have content to display
    this.svgContent = this.getContent();
    // For SVG, we use sanitize with SecurityContext.HTML (1) to allow safe rendering
    this.safeSvgContent = this.sanitizer.sanitize(1, this.svgContent) || '';
    if (this.svgContent) {
      navigator.clipboard.writeText(this.svgContent).then(() => {
        console.log('✅ Copied SVG to clipboard');
