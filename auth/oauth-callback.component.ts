 * @fileoverview OAuth Callback Component
 * Handles OAuth provider redirects in the frontend. When an OAuth provider
 * redirects back to the application after user authorization, this component:
 * 1. Waits for the user's session to be restored (authentication)
 * 2. Extracts the code and state from URL query parameters
 * 3. Calls the backend /oauth/exchange endpoint to complete the token exchange
 * 4. Redirects to the original page with success/error notification
 * @module OAuth Callback
 * Response from the OAuth exchange endpoint
interface OAuthExchangeResponse {
    connectionId?: string;
    isRetryable?: boolean;
/** Maximum time to wait for provider initialization (30 seconds) */
const MAX_WAIT_TIME_MS = 30000;
/** Polling interval for checking provider readiness (500ms) */
const POLL_INTERVAL_MS = 500;
 * OAuth Callback Component
 * Handles the redirect from OAuth providers after user authorization.
 * This component is loaded at /oauth/callback and processes the
 * authorization code to complete the OAuth flow.
 * The component waits for authentication to complete before attempting
 * the code exchange, since the user's session needs to be restored first.
    selector: 'mj-oauth-callback',
    templateUrl: './oauth-callback.component.html',
    styleUrls: ['./oauth-callback.component.css']
export class OAuthCallbackComponent implements OnInit, OnDestroy {
    /** Loading state while processing OAuth callback */
    /** Error state */
    /** Error code from OAuth provider or exchange */
    public ErrorCode = '';
    public IsRetryable = false;
    /** Status message shown during processing */
    public StatusMessage = 'Restoring session...';
    /** Timer for polling provider readiness */
    private pollTimer: ReturnType<typeof setInterval> | null = null;
    /** Flag to prevent double processing */
    private isProcessing = false;
        // Wait for the GraphQL provider to be ready, then process the callback
        await this.waitForProviderAndProcess();
        // Clean up the polling timer
        if (this.pollTimer) {
            clearInterval(this.pollTimer);
            this.pollTimer = null;
     * Waits for the GraphQL provider to be initialized (indicating auth is complete)
     * then processes the OAuth callback.
    private async waitForProviderAndProcess(): Promise<void> {
        // Check if provider is already ready
        if (this.isProviderReady()) {
            await this.safeProcessCallback();
        // Poll for provider readiness
        this.StatusMessage = 'Waiting for authentication...';
            this.pollTimer = setInterval(async () => {
                const elapsed = Date.now() - startTime;
                if (elapsed >= MAX_WAIT_TIME_MS) {
                    this.showError(
                        'timeout',
                        'Timed out waiting for authentication. Please try logging in again.',
                // Update status message with progress
                const secondsRemaining = Math.ceil((MAX_WAIT_TIME_MS - elapsed) / 1000);
                this.StatusMessage = `Waiting for authentication... (${secondsRemaining}s)`;
            }, POLL_INTERVAL_MS);
     * Checks if the GraphQL provider is initialized and has a valid token
    private isProviderReady(): boolean {
            const configData = provider.ConfigData;
            if (!configData || !configData.Token || !configData.URL) {
     * Safely processes the callback, preventing double execution
    private async safeProcessCallback(): Promise<void> {
        if (this.isProcessing) {
            await this.processOAuthCallback();
            console.error('[OAuth Callback] Error in processOAuthCallback:', error);
            this.showError('component_error', error instanceof Error ? error.message : String(error), true);
     * Process the OAuth callback by extracting params and exchanging the code
    private async processOAuthCallback(): Promise<void> {
        // Check for error from OAuth provider
        if (params['error']) {
                params['error'] as string,
                params['error_description'] as string || 'Authorization was denied or failed'
        const code = params['code'] as string;
        const state = params['state'] as string;
            this.showError('missing_code', 'No authorization code received from the OAuth provider');
            this.showError('missing_state', 'No state parameter received from the OAuth provider');
        // Exchange the code for tokens
        this.StatusMessage = 'Exchanging authorization code...';
            const result = await this.exchangeCode(code, state);
                this.handleSuccess(result.connectionId);
                    result.errorCode || 'exchange_failed',
                    result.errorMessage || 'Failed to complete authorization',
                    result.isRetryable
            const message = error instanceof Error ? error.message : 'An unexpected error occurred';
            this.showError('network_error', message, true);
     * Call the backend to exchange the authorization code for tokens
    private async exchangeCode(code: string, state: string): Promise<OAuthExchangeResponse> {
        // Get the GraphQL provider - we know it's ready at this point
        const gqlProvider = Metadata.Provider as GraphQLDataProvider;
        const configData = gqlProvider.ConfigData;
        // Build the exchange endpoint URL from the GraphQL URL
        // Handle both cases: URL with /graphql suffix and URL without
        let apiUrl = configData.URL;
        if (apiUrl.includes('/graphql')) {
            apiUrl = apiUrl.replace('/graphql', '/oauth/exchange');
            // URL doesn't have /graphql, append /oauth/exchange
            apiUrl = apiUrl.replace(/\/$/, '') + '/oauth/exchange';
            const response = await fetch(apiUrl, {
                    'Authorization': `Bearer ${configData.Token}`
                body: JSON.stringify({ code, state })
            // Parse response regardless of status code
            return data as OAuthExchangeResponse;
                errorCode: 'fetch_error',
                errorMessage: fetchError instanceof Error ? fetchError.message : 'Network error during token exchange'
     * Handle successful OAuth completion
    private handleSuccess(connectionId?: string): void {
        this.StatusMessage = 'Authorization successful! Redirecting...';
        // Get the stored return URL - try both localStorage and sessionStorage
        const storedUrl = localStorage.getItem('oauth_return_url');
        const storedPath = localStorage.getItem('oauth_return_path');
        const sessionUrl = sessionStorage.getItem('oauth_return_url');
        const sessionPath = sessionStorage.getItem('oauth_return_path');
        let returnUrl = storedUrl || sessionUrl || storedPath || sessionPath;
        // Clean up all possible storage locations
        this.clearStoredReturnUrls();
        // Default to MCP dashboard if no return URL found
        const defaultPath = '/app/admin/MCP';
        if (!returnUrl || returnUrl.trim() === '' || returnUrl === '/') {
            returnUrl = defaultPath;
        // Build the final navigation path
        const finalPath = this.buildFinalPath(returnUrl, connectionId, defaultPath);
        // Use window.location for reliable navigation after OAuth
            window.location.href = finalPath;
     * Builds the final navigation path with success query params
    private buildFinalPath(returnUrl: string, connectionId: string | undefined, defaultPath: string): string {
            let finalPath: string;
            // If it's a full URL, extract just the path and search
            if (returnUrl.startsWith('http://') || returnUrl.startsWith('https://')) {
                const url = new URL(returnUrl);
                finalPath = url.pathname + url.search;
                finalPath = returnUrl;
            // Ensure we have a valid path (not empty or just "/")
            if (!finalPath || finalPath.trim() === '' || finalPath === '/') {
                finalPath = defaultPath;
            // Add success query params
            const separator = finalPath.includes('?') ? '&' : '?';
            finalPath = `${finalPath}${separator}oauth=success`;
                finalPath += `&connectionId=${connectionId}`;
            return finalPath;
            console.error('[OAuth Callback] Error parsing return URL:', e);
            return `${defaultPath}?oauth=success${connectionId ? `&connectionId=${connectionId}` : ''}`;
     * Clears all stored return URLs from storage
    private clearStoredReturnUrls(): void {
        localStorage.removeItem('oauth_return_url');
        localStorage.removeItem('oauth_return_path');
        sessionStorage.removeItem('oauth_return_url');
        sessionStorage.removeItem('oauth_return_path');
     * Show error state
    private showError(errorCode: string, message: string, isRetryable = false): void {
        this.ErrorCode = errorCode;
        this.ErrorMessage = message;
        this.IsRetryable = isRetryable;
     * Handle retry button click
    public onRetry(): void {
        // Get the stored return URL and redirect there to start fresh
        const returnUrl = localStorage.getItem('oauth_return_url') || '/app/admin/MCP';
        this.router.navigateByUrl(returnUrl);
     * Handle close button click (return to MCP dashboard)
        this.router.navigate(['/app/admin/MCP']);
