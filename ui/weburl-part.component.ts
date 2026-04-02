import { Component, ChangeDetectorRef, ElementRef, ViewChild, AfterViewInit } from '@angular/core';
 * Runtime renderer for WebURL dashboard parts.
 * Displays web content in an iframe with configurable sandbox permissions.
@RegisterClass(BaseDashboardPart, 'WebURLPanelRenderer')
    selector: 'mj-weburl-part',
        <div class="weburl-part" [class.loading]="IsLoading" [class.error]="ErrorMessage">
              @if (rawUrl) {
                <a [href]="rawUrl" target="_blank" class="open-link">
                  Open in new window
          <!-- No URL configured -->
          @if (!IsLoading && !ErrorMessage && !SafeUrl) {
              <h4>No URL Configured</h4>
              <p>Click the configure button to set a URL for this part.</p>
          <!-- Iframe container - sandbox and allowfullscreen must be static, so we use ng-container to switch -->
          @if (!IsLoading && !ErrorMessage && SafeUrl) {
            <!-- Standard sandbox + fullscreen enabled -->
            @if (sandboxMode === 'standard' && allowFullscreen) {
                #iframe
                [src]="SafeUrl"
                sandbox="allow-scripts allow-same-origin allow-forms allow-popups"
                allowfullscreen
                [title]="Panel?.title || 'Embedded content'"
                (load)="onIframeLoad()"
                (error)="onIframeError($event)"
                class="content-iframe">
            <!-- Standard sandbox + fullscreen disabled -->
            @if (sandboxMode === 'standard' && !allowFullscreen) {
            <!-- Strict sandbox + fullscreen enabled -->
            @if (sandboxMode === 'strict' && allowFullscreen) {
                sandbox="allow-scripts"
            <!-- Strict sandbox + fullscreen disabled -->
            @if (sandboxMode === 'strict' && !allowFullscreen) {
            <!-- Permissive sandbox + fullscreen enabled -->
            @if (sandboxMode === 'permissive' && allowFullscreen) {
                sandbox="allow-scripts allow-same-origin allow-forms allow-popups allow-modals allow-top-navigation"
            <!-- Permissive sandbox + fullscreen disabled -->
            @if (sandboxMode === 'permissive' && !allowFullscreen) {
          <!-- Fallback link shown below iframe if content might be blocked -->
          @if (!IsLoading && !ErrorMessage && SafeUrl && showFallbackLink) {
            <div class="iframe-fallback">
              <span>If the content doesn't load:</span>
              <a [href]="rawUrl" target="_blank">
        .weburl-part {
            position: relative; /* Required for overlay positioning */
        .error-state .open-link {
        .error-state .open-link:hover {
        .content-iframe {
        .iframe-fallback {
        .iframe-fallback a {
        .iframe-fallback a:hover {
export class WebURLPartComponent extends BaseDashboardPart implements AfterViewInit {
    @ViewChild('iframe') iframeRef!: ElementRef<HTMLIFrameElement>;
    public SafeUrl: SafeResourceUrl | null = null;
    public rawUrl: string = '';
    public allowFullscreen: boolean = true;
    public showFallbackLink: boolean = false; // Hidden by default, shown if content might be blocked
    private sanitizer: DomSanitizer;
    private loadCheckTimeout: ReturnType<typeof setTimeout> | null = null;
    private iframeLoaded: boolean = false;
    constructor(cdr: ChangeDetectorRef, sanitizer: DomSanitizer) {
        this.sanitizer = sanitizer;
        // Initial load if panel is already set
        const url = config?.['url'] as string | undefined;
        if (this.loadCheckTimeout) {
            clearTimeout(this.loadCheckTimeout);
            this.loadCheckTimeout = null;
        this.iframeLoaded = false;
        this.showFallbackLink = false;
            this.SafeUrl = null;
            this.rawUrl = '';
            // Store raw URL for fallback link
            this.rawUrl = url;
            // Sanitize and set URL for iframe src binding
            this.SafeUrl = this.sanitizer.bypassSecurityTrustResourceUrl(url);
            // Set sandbox mode (used by template to select correct iframe)
            this.sandboxMode = (config?.['sandboxMode'] as 'standard' | 'strict' | 'permissive') || 'standard';
            // Set fullscreen permission
            this.allowFullscreen = (config?.['allowFullscreen'] as boolean) ?? true;
            // Set up a delayed check to detect if embedding might be blocked
            // If after 3 seconds the iframe hasn't successfully loaded content we can access,
            // show the blocked overlay. This is a heuristic since we can't detect X-Frame-Options directly.
            this.loadCheckTimeout = setTimeout(() => {
                this.checkIfBlocked();
            this.setError(error instanceof Error ? error.message : 'Failed to load URL');
     * Check if the iframe content might be blocked.
     * Called after a timeout - if the iframe load event hasn't fired, show the fallback link.
    private checkIfBlocked(): void {
        // If the load event hasn't fired after the timeout, content might be blocked
        if (!this.iframeLoaded) {
            this.showFallbackLink = true;
        // If iframeLoaded is true, content loaded successfully - keep fallback hidden
     * Handle iframe load event
    public onIframeLoad(): void {
        // Mark that the load event fired
        this.iframeLoaded = true;
        // Clear the timeout since the iframe loaded
        // The iframe loaded something - could still be blocked content
        // We can't reliably detect X-Frame-Options blocking from JS
        // The fallback link provides a way for users to open the site if it doesn't load
     * Handle iframe error event
    public onIframeError(_event: Event): void {
        // This may fire for some load failures, but not for X-Frame-Options
        this.setError('This site cannot be embedded. It may block iframe embedding for security reasons.');
