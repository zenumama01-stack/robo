 * @fileoverview OAuth Callback Handler for MCP OAuth flows
 * Handles OAuth authorization callbacks from external authorization servers.
 * This endpoint is unauthenticated since it's called by the auth server after user consent.
 * Endpoints:
 * - GET /api/v1/oauth/callback - Authorization callback
 * - GET /api/v1/oauth/status/:stateParameter - Poll for authorization status (authenticated)
 * - POST /api/v1/oauth/initiate - Initiate OAuth flow (authenticated)
 * @module @memberjunction/server/rest/OAuthCallbackHandler
import { LogError, LogStatus, RunView, UserInfo } from '@memberjunction/core';
import { OAuthManager, MCPClientManager } from '@memberjunction/ai-mcp-client';
import type { MCPServerOAuthConfig } from '@memberjunction/ai-mcp-client';
/** Entity name for MCP Server Connections */
const ENTITY_MCP_SERVER_CONNECTIONS = 'MJ: MCP Server Connections';
/** Entity name for MCP Servers */
const ENTITY_MCP_SERVERS = 'MJ: MCP Servers';
/** Entity name for OAuth Authorization States */
 * Configuration for OAuth callback handler
export interface OAuthCallbackHandlerOptions {
    /** Base URL for this MJAPI instance (for redirects) */
    publicUrl: string;
    /** URL to redirect to after successful authorization */
    successRedirectUrl?: string;
    /** URL to redirect to after failed authorization */
    errorRedirectUrl?: string;
 * Handles OAuth callbacks and related endpoints for MCP server authentication.
 * The callback endpoint is unauthenticated because it's called by external auth servers.
 * It uses the state parameter to look up the authorization context and validate the flow.
 * const oauthHandler = new OAuthCallbackHandler({
 *     publicUrl: 'https://api.example.com'
 * // Mount unauthenticated callback route
 * app.use('/api/v1/oauth', oauthHandler.getCallbackRouter());
 * // Mount authenticated routes
 * app.use('/api/v1/oauth', authMiddleware, oauthHandler.getAuthenticatedRouter());
export class OAuthCallbackHandler {
    private readonly options: OAuthCallbackHandlerOptions;
    private readonly oauthManager: OAuthManager;
    private readonly callbackRouter: express.Router;
    private readonly authenticatedRouter: express.Router;
    constructor(options: OAuthCallbackHandlerOptions) {
            successRedirectUrl: `${options.publicUrl}/oauth/success`,
            errorRedirectUrl: `${options.publicUrl}/oauth/error`,
        this.oauthManager = new OAuthManager();
        this.callbackRouter = express.Router();
        this.authenticatedRouter = express.Router();
     * Sets up all routes for OAuth handling.
    private setupRoutes(): void {
        // Unauthenticated callback route
        this.callbackRouter.get('/callback', this.handleCallback.bind(this));
        // Success and error pages (unauthenticated - user is redirected here after OAuth)
        this.callbackRouter.get('/success', this.handleSuccessPage.bind(this));
        this.callbackRouter.get('/error', this.handleErrorPage.bind(this));
        // Authenticated routes
        this.authenticatedRouter.get('/status/:stateParameter', this.getStatus.bind(this));
        this.authenticatedRouter.post('/initiate', this.initiateFlow.bind(this));
        this.authenticatedRouter.post('/exchange', this.handleExchange.bind(this));
     * Renders a success page after OAuth authorization completes.
    private handleSuccessPage(req: express.Request, res: express.Response): void {
        const { state, connectionId } = req.query;
        res.status(200).send(`
    <title>Authorization Successful</title>
        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; display: flex; justify-content: center; align-items: center; min-height: 100vh; margin: 0; background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); }
        .container { background: white; padding: 3rem; border-radius: 1rem; box-shadow: 0 20px 60px rgba(0,0,0,0.3); text-align: center; max-width: 400px; }
        .icon { font-size: 4rem; margin-bottom: 1rem; }
        h1 { color: #22c55e; margin: 0 0 1rem; }
        p { color: #64748b; margin: 0 0 1.5rem; line-height: 1.6; }
        .details { background: #f1f5f9; padding: 1rem; border-radius: 0.5rem; font-size: 0.875rem; color: #475569; word-break: break-all; }
        <div class="icon">✅</div>
        <h1>Authorization Successful</h1>
        <p>Your MCP server connection has been authorized. You can close this window and return to MemberJunction.</p>
        ${connectionId ? `<div class="details">Connection ID: ${connectionId}</div>` : ''}
     * Renders an error page when OAuth authorization fails.
    private handleErrorPage(req: express.Request, res: express.Response): void {
        const { error, error_description } = req.query;
        const errorCode = error ? String(error) : 'unknown_error';
        const errorMessage = error_description ? String(error_description) : 'An error occurred during authorization.';
    <title>Authorization Failed</title>
        h1 { color: #ef4444; margin: 0 0 1rem; }
        .error-code { background: #fef2f2; color: #991b1b; padding: 0.5rem 1rem; border-radius: 0.5rem; font-family: monospace; display: inline-block; margin-bottom: 1rem; }
        .error-message { background: #f1f5f9; padding: 1rem; border-radius: 0.5rem; font-size: 0.875rem; color: #475569; }
        <div class="icon">❌</div>
        <h1>Authorization Failed</h1>
        <div class="error-code">${errorCode}</div>
        <div class="error-message">${errorMessage}</div>
        <p style="margin-top: 1.5rem;">Please close this window and try again.</p>
     * Handles the OAuth authorization callback.
     * This endpoint is unauthenticated - the auth server redirects here after user consent.
     * We validate the state parameter and exchange the code for tokens.
     * @param req - Express request
     * @param res - Express response
    private async handleCallback(req: express.Request, res: express.Response): Promise<void> {
        LogStatus(`[OAuth Callback] Received callback: ${req.originalUrl}`);
        const { code, state, error, error_description } = req.query;
        LogStatus(`[OAuth Callback] Parameters - state: ${state}, code: ${code ? 'present' : 'missing'}, error: ${error || 'none'}`);
        // Validate state parameter is present
        if (!state || typeof state !== 'string') {
            this.redirectToError(res, 'invalid_request', 'Missing state parameter');
            // Get system user for initial lookup
            const systemUser = this.getSystemUser();
                LogError('[OAuth Callback] System user not available');
                this.redirectToError(res, 'server_error', 'System configuration error');
            // Look up the authorization state to get the user context
            const authState = await this.loadAuthorizationState(state, systemUser);
                this.redirectToError(res, 'invalid_state', 'Authorization state not found or expired');
            // Get the actual user from cache
            const contextUser = UserCache.Users.find(u => u.ID === authState.userId);
                LogError(`[OAuth Callback] User ${authState.userId} not found in cache`);
                this.redirectToError(res, 'server_error', 'User context not found', authState.frontendReturnUrl);
            // Handle error from authorization server
                const errorMessage = error_description ? String(error_description) : 'Authorization denied';
                await this.oauthManager.handleAuthorizationError(
                    String(error),
                this.redirectToError(res, String(error), errorMessage, authState.frontendReturnUrl);
            // Validate authorization code is present
            if (!code || typeof code !== 'string') {
                this.redirectToError(res, 'invalid_request', 'Missing authorization code', authState.frontendReturnUrl);
            const result = await this.oauthManager.completeAuthorizationFlow(state, code, contextUser);
                LogStatus(`[OAuth Callback] Authorization completed for state ${state}`);
                // Notify MCPClientManager that authorization has completed
                MCPClientManager.Instance.notifyOAuthAuthorizationCompleted(authState.connectionId, {
                    stateParameter: state,
                    completedAt: new Date().toISOString()
                this.redirectToSuccess(res, state, authState.connectionId, authState.frontendReturnUrl);
                LogError(`[OAuth Callback] Authorization failed: ${result.errorMessage}`);
                this.redirectToError(
                    result.errorCode ?? 'authorization_failed',
                    result.errorMessage ?? 'Authorization failed',
                    authState.frontendReturnUrl
            LogError(`[OAuth Callback] Unexpected error: ${errorMessage}`);
            this.redirectToError(res, 'server_error', 'An unexpected error occurred');
     * Gets the status of an OAuth authorization flow.
     * Authenticated endpoint for polling authorization status.
    private async getStatus(req: express.Request, res: express.Response): Promise<void> {
        const stateParam = req.params.stateParameter;
        const stateParameter = Array.isArray(stateParam) ? stateParam[0] : stateParam || '';
        const contextUser = req['mjUser'] as UserInfo;
                errorMessage: 'Authentication required'
            const authState = await this.loadAuthorizationState(stateParameter, contextUser);
                    errorCode: 'not_found',
                    errorMessage: 'Authorization state not found'
            // Verify user owns this state
            if (authState.userId !== contextUser.ID) {
                res.status(403).json({
                    errorCode: 'forbidden',
                    errorMessage: 'Access denied'
            // Map status
            let status: 'pending' | 'completed' | 'failed' | 'expired';
            switch (authState.status) {
                    status = new Date() >= authState.expiresAt ? 'expired' : 'pending';
                    status = 'completed';
                    status = 'failed';
                case 'Expired':
                    status = 'expired';
                    status = 'pending';
                connectionId: authState.connectionId,
                completedAt: authState.completedAt?.toISOString(),
                errorCode: authState.errorCode,
                errorMessage: authState.errorDescription,
                isRetryable: status === 'failed' || status === 'expired'
            LogError(`[OAuth Status] Error: ${errorMessage}`);
                errorCode: 'server_error',
                errorMessage: 'Failed to get authorization status'
     * Initiates a new OAuth authorization flow.
     * Authenticated endpoint for starting OAuth authorization.
    private async initiateFlow(req: express.Request, res: express.Response): Promise<void> {
        const { connectionId, additionalScopes, frontendReturnUrl } = req.body;
                errorCode: 'invalid_request',
                errorMessage: 'connectionId is required'
            const config = await this.loadConnectionConfig(connectionId, contextUser);
                    errorMessage: 'Connection not found'
            // Build OAuth config
                OAuthIssuerURL: config.OAuthIssuerURL,
                OAuthScopes: additionalScopes
                    ? `${config.OAuthScopes ?? ''} ${additionalScopes}`.trim()
                    : config.OAuthScopes,
                OAuthMetadataCacheTTLMinutes: config.OAuthMetadataCacheTTLMinutes,
                OAuthClientID: config.OAuthClientID,
                OAuthClientSecretEncrypted: config.OAuthClientSecretEncrypted,
                OAuthRequirePKCE: config.OAuthRequirePKCE
                    errorCode: 'invalid_configuration',
                    errorMessage: 'OAuth is not configured for this server'
            // Initiate the flow with optional frontend return URL
            const result = await this.oauthManager.initiateAuthorizationFlow(
                config.serverId,
                this.options.publicUrl,
                frontendReturnUrl ? { frontendReturnUrl: String(frontendReturnUrl) } : undefined
                    authorizationUrl: result.authorizationUrl,
                    stateParameter: result.stateParameter,
                    expiresAt: result.expiresAt?.toISOString(),
                    message: 'Please redirect the user to the authorization URL'
                    errorCode: 'initiation_failed',
            LogError(`[OAuth Initiate] Error: ${errorMessage}`);
                errorMessage: 'Failed to initiate OAuth flow'
     * This endpoint is used when the frontend handles the OAuth callback.
     * The frontend receives the code and state from the OAuth provider redirect,
     * then calls this authenticated endpoint to complete the token exchange.
     * @param req - Express request with { code, state } in body
    private async handleExchange(req: express.Request, res: express.Response): Promise<void> {
        const { code, state } = req.body;
                errorCode: 'unauthorized',
                errorMessage: 'Missing or invalid code parameter'
                errorMessage: 'Missing or invalid state parameter'
            // Load the authorization state to verify user ownership
            const authState = await this.loadAuthorizationState(state, contextUser);
                    errorCode: 'state_not_found',
                    errorMessage: 'Authorization state not found or expired'
            // Verify the authenticated user owns this authorization state
                LogError(`[OAuth Exchange] User ${contextUser.ID} attempted to exchange code for state owned by ${authState.userId}`);
                    errorMessage: 'You are not authorized to complete this authorization flow'
            // Exchange code for tokens using OAuthManager
                LogStatus(`[OAuth Exchange] Successfully exchanged code for connection ${authState.connectionId}`);
                    connectionId: authState.connectionId
                LogError(`[OAuth Exchange] Token exchange failed: ${result.errorMessage}`);
                    errorCode: result.errorCode ?? 'exchange_failed',
                    errorMessage: result.errorMessage ?? 'Failed to exchange authorization code for tokens',
                    isRetryable: result.isRetryable ?? false
            LogError(`[OAuth Exchange] Unexpected error: ${errorMessage}`);
                errorMessage: 'An unexpected error occurred during token exchange'
     * Gets the system user from cache.
    private getSystemUser(): UserInfo | null {
            return UserCache.Instance.GetSystemUser();
                Fields: ['MCPServerConnectionID', 'UserID', 'Status', 'ErrorCode', 'ErrorDescription', 'ExpiresAt', 'CompletedAt', 'FrontendReturnURL'],
            LogError(`[OAuth Callback] Failed to load authorization state: ${error}`);
     * Loads connection and server OAuth configuration.
    private async loadConnectionConfig(
            // Get connection to get server ID
                EntityName: ENTITY_MCP_SERVER_CONNECTIONS,
            // Get server OAuth config
                EntityName: ENTITY_MCP_SERVERS,
                    'OAuthIssuerURL', 'OAuthScopes', 'OAuthMetadataCacheTTLMinutes',
                    'OAuthClientID', 'OAuthClientSecretEncrypted', 'OAuthRequirePKCE'
            LogError(`[OAuth Callback] Failed to load connection config: ${error}`);
     * Redirects to success page with state info.
     * If a frontend return URL is provided, redirects there instead of the default success page.
    private redirectToSuccess(res: express.Response, state: string, connectionId: string, frontendReturnUrl?: string): void {
        // If frontend return URL is provided, redirect there with success parameters
        if (frontendReturnUrl) {
                const url = new URL(frontendReturnUrl);
                url.searchParams.set('oauth', 'success');
                url.searchParams.set('connectionId', connectionId);
                LogStatus(`[OAuth Callback] Redirecting to frontend URL: ${url.toString()}`);
                res.redirect(302, url.toString());
                LogError(`[OAuth Callback] Invalid frontend return URL '${frontendReturnUrl}', falling back to default`);
                // Fall through to default redirect
        // Default: redirect to built-in success page
        const url = new URL(this.options.successRedirectUrl!);
     * Redirects to error page with error info.
     * If a frontend return URL is provided, redirects there instead of the default error page.
    private redirectToError(res: express.Response, errorCode: string, errorMessage: string, frontendReturnUrl?: string): void {
        // If frontend return URL is provided, redirect there with error parameters
                url.searchParams.set('oauth', 'error');
                url.searchParams.set('error', errorCode);
                url.searchParams.set('error_description', errorMessage);
                LogStatus(`[OAuth Callback] Redirecting to frontend URL with error: ${url.toString()}`);
        // Default: redirect to built-in error page
        const url = new URL(this.options.errorRedirectUrl!);
     * Gets the router for unauthenticated callback endpoint.
    public getCallbackRouter(): express.Router {
        return this.callbackRouter;
     * Gets the router for authenticated OAuth endpoints.
    public getAuthenticatedRouter(): express.Router {
        return this.authenticatedRouter;
 * Creates and configures the OAuth callback handler.
 * @param options - Handler configuration
 * @returns Object with callback and authenticated routers
export function createOAuthCallbackHandler(options: OAuthCallbackHandlerOptions): {
    callbackRouter: express.Router;
    authenticatedRouter: express.Router;
    const handler = new OAuthCallbackHandler(options);
        callbackRouter: handler.getCallbackRouter(),
        authenticatedRouter: handler.getAuthenticatedRouter()
