import { Component, OnInit, OnDestroy, ChangeDetectorRef, ViewChild, ElementRef } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
 * Viewer component for JSON artifacts.
 * Supports extract rules - shows displayMarkdown, displayHtml, or raw JSON editor (in that priority order).
 * All content is displayed in the parent's Display tab.
  selector: 'mj-json-artifact-viewer',
    <div class="json-artifact-viewer" [ngClass]="cssClass">
      <!-- Display toolbar -->
        <button class="btn-icon" title="Copy Content" (click)="onCopy()">
        @if (displayHtml) {
          <button class="btn-icon" title="Open in New Window" (click)="openInNewWindow()">
            <i class="fas fa-external-link-alt"></i> New Window
          <button class="btn-icon" title="Print" (click)="printHtml()">
      <!-- Display content: priority order = displayHtml > displayMarkdown > JSON editor -->
        @if (displayHtml && htmlBlobUrl) {
          <!-- Sandboxed iframe for rich HTML using blob URL -->
          <iframe
            #htmlFrame
            [src]="htmlBlobUrl"
            sandbox="allow-same-origin allow-scripts allow-popups allow-popups-to-escape-sandbox allow-modals"
            class="html-iframe"
            (load)="onIframeLoad()">
          </iframe>
        } @else if (displayMarkdown) {
    .json-artifact-viewer {
    .html-iframe {
@RegisterClass(BaseArtifactViewerPluginComponent, 'JsonArtifactViewerPlugin')
export class JsonArtifactViewerComponent extends BaseArtifactViewerPluginComponent implements OnInit, OnDestroy {
  @ViewChild('htmlFrame') htmlFrame?: ElementRef<HTMLIFrameElement>;
  public htmlBlobUrl: SafeResourceUrl | null = null;
  private versionAttributes: MJArtifactVersionAttributeEntity[] = [];
  private unsafeBlobUrl: string | null = null; // Keep unsafe URL for cleanup
   * JSON artifacts always have content to display (JSON editor, displayHtml, or displayMarkdown)
    private sanitizer: DomSanitizer
    // Clean up blob URL to prevent memory leaks
    if (this.unsafeBlobUrl) {
      URL.revokeObjectURL(this.unsafeBlobUrl);
      this.unsafeBlobUrl = null;
      this.htmlBlobUrl = null;
    this.jsonContent = this.getContent();
    // Load version attributes to check for extract rules
    // Trigger change detection after async load
   * Override to return true when showing extracted displayHtml or displayMarkdown.
   * Returns false when showing raw JSON editor (no extract rules available).
  public override get isShowingElevatedDisplay(): boolean {
    return !!(this.displayHtml || this.displayMarkdown);
   * Override to tell parent whether to show raw JSON tab.
   * When showing elevated display (markdown/HTML), return true so parent shows JSON tab.
   * When showing raw JSON editor, return false (no need for duplicate JSON tab).
  public override get parentShouldShowRawContent(): boolean {
    return this.isShowingElevatedDisplay;
    if (!this.artifactVersion?.ID) {
      console.log('📦 JSON Plugin: No artifactVersion.ID, skipping attribute load');
    console.log(`📦 JSON Plugin: Loading attributes for version ID: ${this.artifactVersion.ID}`);
      console.log(`📦 JSON Plugin: RunView completed. Success=${result.Success}, Results count=${result.Results?.length || 0}`);
        console.log(`📦 JSON Plugin: Loaded ${this.versionAttributes.length} attributes for version ${this.artifactVersion.ID}`);
        console.log(`📦 Attributes:`, this.versionAttributes.map(a => ({ name: a.Name, hasValue: !!a.Value })));
        // Check for displayHtml and displayMarkdown attributes (from extract rules)
        // Priority: displayHtml > displayMarkdown
        console.log(`📦 displayHtmlAttr:`, displayHtmlAttr ? { name: displayHtmlAttr.Name, valueLength: displayHtmlAttr.Value?.length } : 'not found');
        console.log(`📦 displayMarkdownAttr:`, displayMarkdownAttr ? { name: displayMarkdownAttr.Name, valueLength: displayMarkdownAttr.Value?.length } : 'not found');
        // Parse attribute values - fix "null" string bug
        // Create blob URL for HTML to avoid srcdoc sanitization issues
          const blob = new Blob([this.displayHtml], { type: 'text/html' });
          this.unsafeBlobUrl = URL.createObjectURL(blob);
          // Sanitize the blob URL so Angular trusts it in the iframe
          this.htmlBlobUrl = this.sanitizer.bypassSecurityTrustResourceUrl(this.unsafeBlobUrl);
        // Note: Markdown rendering is now handled by <mj-markdown> component in template
        console.log(`📦 JSON Plugin: displayHtml=${!!this.displayHtml} (${this.displayHtml?.length || 0} chars), displayMarkdown=${!!this.displayMarkdown} (${this.displayMarkdown?.length || 0} chars)`);
        console.log(`📦 isShowingElevatedDisplay=${this.isShowingElevatedDisplay}`);
        console.log(`📦 JSON Plugin: No attributes found or query failed. Success=${result.Success}, ResultsLength=${result.Results?.length}`);
      console.error('📦 JSON Plugin: Error loading version attributes:', err);
    // Fix bug: Some extractors return string "null" instead of actual null
    if (value === 'null' || value.trim() === '') {
      const parsed = JSON.parse(value);
      if (typeof parsed === 'string') {
        // Check if parsed string is also "null"
        if (parsed === 'null' || parsed.trim() === '') {
      // If not valid JSON, return as-is (unless it's "null")
      return value === 'null' ? null : value;
  onIframeLoad(): void {
    // Inject base styles if HTML doesn't have them
    if (this.htmlFrame) {
      const iframe = this.htmlFrame.nativeElement;
      const iframeDoc = iframe.contentDocument || iframe.contentWindow?.document;
      if (iframeDoc) {
        // Check if HTML already has styles
        const hasStyles = iframeDoc.querySelector('style') || iframeDoc.querySelector('link[rel="stylesheet"]');
        if (!hasStyles) {
          // Inject minimal base styles for better defaults
          const style = iframeDoc.createElement('style');
          style.textContent = `
            h1, h2, h3 { color: #2c3e50; margin-top: 1.5em; margin-bottom: 0.5em; }
            h1 { font-size: 2em; border-bottom: 2px solid #3498db; padding-bottom: 0.3em; }
            h2 { font-size: 1.5em; }
            table { border-collapse: collapse; width: 100%; margin: 1em 0; }
            th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
            th { background-color: #f8f9fa; font-weight: 600; }
            pre { background: #f6f8fa; padding: 16px; border-radius: 6px; overflow-x: auto; }
            code { background: #f6f8fa; padding: 2px 6px; border-radius: 3px; font-family: monospace; }
          iframeDoc.head.appendChild(style);
        // Inject width guardrails to prevent iframe content from causing horizontal overflow
        const widthOverride = iframeDoc.createElement('style');
        widthOverride.textContent = `
          html, body {
            overflow-x: hidden !important;
            margin: 20px 10px 5px 20px !important; /* top right bottom left */
          img, svg, table, pre {
        iframeDoc.head.appendChild(widthOverride);
        console.log('📦 Iframe loaded, hasStyles:', !!hasStyles);
        // Auto-resize iframe to fit content
        this.resizeIframeToContent();
  private resizeIframeToContent(): void {
      if (iframeDoc && iframeDoc.body) {
        // Get the actual content height
        const contentHeight = Math.max(
          iframeDoc.body.scrollHeight,
          iframeDoc.body.offsetHeight,
          iframeDoc.documentElement.scrollHeight,
          iframeDoc.documentElement.offsetHeight
        // Set iframe height to match content (with a bit of padding)
        iframe.style.height = `${contentHeight + 20}px`;
        console.log('📦 Iframe resized - Height:', contentHeight + 20, 'px');
  openInNewWindow(): void {
      const newWindow = window.open('', '_blank');
      if (newWindow) {
        newWindow.document.write(this.displayHtml);
        newWindow.document.close();
  printHtml(): void {
      const iframeWindow = iframe.contentWindow;
      if (iframeWindow) {
        iframeWindow.focus();
        iframeWindow.print();
    // Copy based on what's being displayed - prioritize displayHtml
    const content = this.displayHtml || this.displayMarkdown || this.jsonContent;
      navigator.clipboard.writeText(content).then(() => {
        console.log('✅ Copied content to clipboard');
   * Removes literal "\\n", "\\t", and "\\\"" which cause rendering issues
    let cleaned = html.replace(/\\"/g, '"');
    cleaned = cleaned.replace(/\\n/g, '');
