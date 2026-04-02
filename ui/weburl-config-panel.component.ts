 * Configuration panel for WebURL parts.
 * Contains only the form content - no dialog chrome.
@RegisterClass(BaseConfigPanel, 'WebURLPanelConfigDialog')
    selector: 'mj-weburl-config-panel',
    templateUrl: './weburl-config-panel.component.html',
export class WebURLConfigPanelComponent extends BaseConfigPanel {
    public url = '';
    public sandboxMode: 'standard' | 'strict' | 'permissive' = 'standard';
    public allowFullscreen = true;
    public refreshOnResize = false;
    public urlError = '';
    public showUrlPreview = false;
        if (config && config.type === 'WebURL') {
            this.url = (config['url'] as string) || '';
            this.sandboxMode = (config['sandboxMode'] as 'standard' | 'strict' | 'permissive') || 'standard';
            this.allowFullscreen = (config['allowFullscreen'] as boolean) ?? true;
            this.refreshOnResize = (config['refreshOnResize'] as boolean) ?? false;
            // Defaults for new WebURL panel
            this.url = '';
            this.sandboxMode = 'standard';
            this.allowFullscreen = true;
            this.refreshOnResize = false;
        this.urlError = '';
        this.showUrlPreview = this.url ? this.isValidUrl(this.url) : false;
            type: 'WebURL',
            url: this.url.trim(),
            sandboxMode: this.sandboxMode,
            allowFullscreen: this.allowFullscreen,
            refreshOnResize: this.refreshOnResize
        if (!this.url.trim()) {
            this.urlError = 'URL is required';
            errors.push(this.urlError);
        } else if (!this.isValidUrl(this.url.trim())) {
            this.urlError = 'Please enter a valid URL (e.g., https://example.com)';
        if (this.url) {
                const hostname = new URL(this.url).hostname;
                return hostname || 'Web Page';
                return 'Web Page';
    public onUrlChange(): void {
        this.showUrlPreview = false;
        if (this.url.trim() && this.isValidUrl(this.url.trim())) {
            this.showUrlPreview = true;
            // Update title if it's still empty
            if (!this.title) {
                this.title = this.getDefaultTitle();
    public onSandboxModeChange(): void {
    public previewUrl(): void {
        if (this.url && this.isValidUrl(this.url)) {
            window.open(this.url, '_blank', 'noopener,noreferrer');
    private isValidUrl(url: string): boolean {
            const parsed = new URL(url);
            return parsed.protocol === 'http:' || parsed.protocol === 'https:';
    public getSandboxModeDescription(): string {
        switch (this.sandboxMode) {
            case 'strict':
                return 'Only allows scripts to run. Most secure but may break some sites.';
            case 'permissive':
                return 'Allows most features including popups and navigation. Use with trusted sites only.';
                return 'Allows scripts, forms, and popups. Good balance of security and compatibility.';
