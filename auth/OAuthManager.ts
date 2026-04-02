 * @fileoverview OAuth Manager - Main orchestrator for OAuth flows
 * Coordinates OAuth 2.1 authorization code flow with PKCE, including
 * metadata discovery, client registration, authorization initiation,
 * token exchange, and refresh.
 * @module @memberjunction/ai-mcp-client/oauth/OAuthManager
import { AuthServerDiscovery } from './AuthServerDiscovery.js';
import { ClientRegistration } from './ClientRegistration.js';
import { TokenManager } from './TokenManager.js';
import { PKCEGenerator } from './PKCEGenerator.js';
import { OAuthErrorMessages } from './ErrorMessages.js';
    OAuthTokenSet,
    OAuthAuthorizationState,
    OAuthAuthorizationStatus,
    InitiateAuthorizationResult,
    CompleteAuthorizationResult,
    OAuthConnectionStatus,
    MCPServerOAuthConfig,
    OAuthTokenResponse
    OAuthReauthorizationRequiredError
import { getOAuthAuditLogger } from './OAuthAuditLogger.js';
/** Entity name for OAuth authorization states */
const ENTITY_OAUTH_AUTHORIZATION_STATES = 'MJ: O Auth Authorization States';
/** Default authorization timeout in minutes */
const DEFAULT_AUTH_TIMEOUT_MINUTES = 5;
 * Main orchestrator for OAuth 2.1 flows in MCP connections.
 * Coordinates all OAuth operations including:
 * - Authorization server metadata discovery
 * - Dynamic client registration (DCR)
 * - Authorization code flow with PKCE
 * - Token storage and refresh
 * const oauth = new OAuthManager();
 * // Try to get an access token (may throw OAuthAuthorizationRequiredError)
 *     const accessToken = await oauth.getAccessToken(connectionId, serverConfig, contextUser);
 *     // Use access token for API calls
 *     if (error instanceof OAuthAuthorizationRequiredError) {
 *         // Redirect user to error.authorizationUrl
export class OAuthManager {
    /** Discovery service for authorization server metadata */
    private readonly discovery: AuthServerDiscovery;
    /** Client registration service */
    private readonly registration: ClientRegistration;
    /** Token management service */
    private readonly tokenManager: TokenManager;
    /** PKCE generator */
    private readonly pkceGenerator: PKCEGenerator;
        this.discovery = new AuthServerDiscovery();
        this.registration = new ClientRegistration();
        this.tokenManager = new TokenManager();
        this.pkceGenerator = new PKCEGenerator();
     * Gets a valid access token for an OAuth-protected connection.
     * If valid tokens exist, returns the access token.
     * If tokens are expired but refresh token is valid, refreshes and returns.
     * If no valid tokens, throws OAuthAuthorizationRequiredError with authorization URL.
     * @param oauthConfig - OAuth configuration from MCP Server
     * @param publicUrl - MJAPI public URL for callbacks
     * @returns Valid access token
     * @throws OAuthReauthorizationRequiredError if refresh failed and re-auth is needed
    public async getAccessToken(
        oauthConfig: MCPServerOAuthConfig,
        publicUrl: string,
        if (!oauthConfig.OAuthIssuerURL) {
            throw new Error('OAuth issuer URL not configured');
        // Check for existing valid tokens
        const existingTokens = await this.tokenManager.loadTokens(connectionId, contextUser);
        if (existingTokens) {
            // Check if access token is still valid
            if (this.tokenManager.isTokenSetValid(existingTokens)) {
                return existingTokens.accessToken;
            // Try to refresh if we have a refresh token
            if (existingTokens.refreshToken) {
                    const metadata = await this.discovery.getMetadata(
                        oauthConfig.OAuthIssuerURL,
                        { cacheTTLMinutes: oauthConfig.OAuthMetadataCacheTTLMinutes }
                    const clientReg = await this.registration.getOrRegisterClient(
                            redirectUri: this.buildRedirectUri(publicUrl),
                            scopes: oauthConfig.OAuthScopes,
                            preConfiguredClientId: oauthConfig.OAuthClientID,
                            preConfiguredClientSecret: oauthConfig.OAuthClientSecretEncrypted
                    const refreshResult = await this.tokenManager.refreshTokens(
                        existingTokens,
                        clientReg,
                        metadata.token_endpoint,
                    if (refreshResult.success && refreshResult.tokens) {
                        return refreshResult.tokens.accessToken;
                    if (refreshResult.requiresReauthorization) {
                        // Initiate a new auth flow for re-authorization
                        const reAuthResult = await this.initiateAuthorizationFlow(
                            publicUrl,
                        throw new OAuthReauthorizationRequiredError(
                            refreshResult.errorMessage ?? 'Token refresh failed, re-authorization required',
                            refreshResult.errorMessage ?? 'Token refresh failed',
                            refreshResult.errorMessage,
                            reAuthResult.authorizationUrl,
                            reAuthResult.stateParameter
                    LogError(`[OAuth] Token refresh failed: ${error}`);
        // Need user authorization - initiate the flow
        const result = await this.initiateAuthorizationFlow(
        if (!result.success || !result.authorizationUrl || !result.stateParameter || !result.expiresAt) {
            throw new Error(`Failed to initiate OAuth flow: ${result.errorMessage}`);
        throw new OAuthAuthorizationRequiredError(
            'OAuth authorization required. Please complete the authorization flow.',
            result.authorizationUrl,
            result.stateParameter,
            result.expiresAt
     * Initiates an OAuth authorization flow.
     * Creates authorization URL with PKCE and stores state in database.
     * The user should be redirected to the returned URL to complete consent.
     * @param oauthConfig - OAuth configuration
     * @param options - Additional options
     * @param options.frontendReturnUrl - URL to redirect to after OAuth completion
     * @param options.frontendCallbackUrl - URL to use as redirect_uri (frontend handles the callback)
     * @returns Authorization initiation result with URL
    public async initiateAuthorizationFlow(
        options?: { frontendReturnUrl?: string; frontendCallbackUrl?: string }
    ): Promise<InitiateAuthorizationResult> {
                    errorMessage: 'OAuth issuer URL not configured'
            // Discover authorization server metadata
            // Get or register client
            // Use frontend callback URL if provided, otherwise use server callback URL
            const redirectUri = options?.frontendCallbackUrl || this.buildRedirectUri(publicUrl);
                    redirectUri,
            // Generate PKCE challenge
            const pkce = this.pkceGenerator.generate();
            // Generate state parameter
            const stateParameter = this.pkceGenerator.generateState();
            // Build authorization URL
            const authUrl = new URL(metadata.authorization_endpoint);
            authUrl.searchParams.set('response_type', 'code');
            authUrl.searchParams.set('client_id', clientReg.clientId);
            authUrl.searchParams.set('redirect_uri', redirectUri);
            authUrl.searchParams.set('state', stateParameter);
            authUrl.searchParams.set('code_challenge', pkce.codeChallenge);
            authUrl.searchParams.set('code_challenge_method', pkce.codeChallengeMethod);
            if (oauthConfig.OAuthScopes) {
                authUrl.searchParams.set('scope', oauthConfig.OAuthScopes);
            const authorizationUrl = authUrl.toString();
            const expiresAt = new Date(now.getTime() + DEFAULT_AUTH_TIMEOUT_MINUTES * 60 * 1000);
            // Create authorization state record
            const state: OAuthAuthorizationState = {
                stateParameter,
                pkce,
                requestedScopes: oauthConfig.OAuthScopes,
                status: 'Pending',
                authorizationUrl,
                initiatedAt: now,
                expiresAt,
                frontendReturnUrl: options?.frontendReturnUrl
            await this.saveAuthorizationState(state, contextUser);
            LogStatus(`[OAuth] Initiated authorization flow for connection ${connectionId}`);
            // Audit log: Authorization initiated (T047)
            const auditLogger = getOAuthAuditLogger();
            await auditLogger.logAuthorizationInitiated({
                issuerUrl: oauthConfig.OAuthIssuerURL!,
                usedDynamicRegistration: !oauthConfig.OAuthClientID,
                stateParameter
                usedDynamicRegistration: !oauthConfig.OAuthClientID
            LogError(`[OAuth] Failed to initiate authorization: ${errorMessage}`);
                errorMessage: OAuthErrorMessages.getUserMessage(errorMessage)
     * Completes an OAuth authorization flow by exchanging the code for tokens.
     * Called by the OAuth callback handler after user consent.
     * @param stateParameter - The state parameter from the callback
     * @param code - The authorization code
     * @returns Completion result
    public async completeAuthorizationFlow(
        stateParameter: string,
        code: string,
    ): Promise<CompleteAuthorizationResult> {
            // Load authorization state
            const state = await this.loadAuthorizationState(stateParameter, contextUser);
            if (!state) {
                    errorMessage: 'Authorization state not found or expired',
                    errorCode: 'state_mismatch',
                    isRetryable: true
            // Check state hasn't expired
            if (new Date() >= state.expiresAt) {
                await this.updateAuthorizationState(state.id!, 'Expired', contextUser);
                    errorMessage: 'Authorization flow has expired. Please try again.',
                    errorCode: 'authorization_timeout',
            // Load server config to get OAuth settings
            const serverConfig = await this.loadServerConfig(state.connectionId, contextUser);
            if (!serverConfig || !serverConfig.OAuthIssuerURL) {
                    errorMessage: 'OAuth configuration not found',
                    errorCode: 'invalid_client',
                    isRetryable: false
            // Get metadata and client registration
            const metadata = await this.discovery.getMetadata(serverConfig.OAuthIssuerURL, contextUser);
                state.connectionId,
                serverConfig.MCPServerID,
                    redirectUri: state.redirectUri,
                    scopes: state.requestedScopes,
                    preConfiguredClientId: serverConfig.OAuthClientID,
                    preConfiguredClientSecret: serverConfig.OAuthClientSecretEncrypted
            // Exchange code for tokens
            const tokenResponse = await this.exchangeCodeForTokens(
                code,
                state.pkce.codeVerifier,
                state.redirectUri,
                clientReg.clientId,
                clientReg.clientSecret,
                metadata.token_endpoint
            // Build token set
            const tokens: OAuthTokenSet = {
                accessToken: tokenResponse.access_token,
                tokenType: tokenResponse.token_type ?? 'Bearer',
                expiresAt: this.calculateExpiresAt(tokenResponse.expires_in),
                refreshToken: tokenResponse.refresh_token,
                scope: tokenResponse.scope ?? state.requestedScopes,
                issuer: serverConfig.OAuthIssuerURL,
                lastRefreshAt: Math.floor(Date.now() / 1000),
                refreshCount: 0
            // Store tokens
            await this.tokenManager.storeTokens(state.connectionId, tokens, contextUser);
            // Update authorization state
            await this.updateAuthorizationState(state.id!, 'Completed', contextUser);
            LogStatus(`[OAuth] Completed authorization flow for connection ${state.connectionId}`);
            // Audit log: Authorization completed (T048)
            await auditLogger.logAuthorizationCompleted({
                connectionId: state.connectionId,
                issuerUrl: serverConfig.OAuthIssuerURL!,
                grantedScopes: tokens.scope,
                tokenExpiresAt: new Date(tokens.expiresAt * 1000),
                hasRefreshToken: !!tokens.refreshToken
                tokens
            const mapped = OAuthErrorMessages.mapError(errorMessage);
            LogError(`[OAuth] Failed to complete authorization: ${errorMessage}`);
            // Audit log: Authorization failed (part of T048)
                await auditLogger.logAuthorizationFailed({
                    connectionId: stateParameter, // Use state param as connection ID may not be available
                    errorCode: errorMessage,
                    errorDescription: mapped.userMessage,
                    isRetryable: mapped.isRetryable
                // Ignore audit logging errors
                errorMessage: mapped.userMessage,
     * Handles an OAuth callback error.
     * @param stateParameter - The state parameter
     * @param errorCode - OAuth error code
     * @param errorDescription - Error description
    public async handleAuthorizationError(
        errorCode: string,
        errorDescription: string | undefined,
            if (state?.id) {
                await this.updateAuthorizationState(
                    state.id,
                    errorCode,
                    errorDescription
            LogError(`[OAuth] Failed to handle authorization error: ${error}`);
     * Gets the OAuth connection status.
     * @returns Connection status
    public async getConnectionStatus(
    ): Promise<OAuthConnectionStatus> {
        const status: OAuthConnectionStatus = {
            isOAuthEnabled: !!oauthConfig.OAuthIssuerURL,
            hasValidTokens: false,
            issuerUrl: oauthConfig.OAuthIssuerURL
        const tokens = await this.tokenManager.loadTokens(connectionId, contextUser);
        if (tokens) {
            status.hasValidTokens = this.tokenManager.isTokenSetValid(tokens);
            status.isAccessTokenExpired = !status.hasValidTokens;
            status.tokenExpiresAt = new Date(tokens.expiresAt * 1000);
            status.hasRefreshToken = !!tokens.refreshToken;
            status.grantedScopes = tokens.scope;
            if (!status.hasValidTokens && !tokens.refreshToken) {
                status.requiresReauthorization = true;
                status.reauthorizationReason = 'Access token expired and no refresh token available';
            status.reauthorizationReason = 'No stored credentials';
     * Validates OAuth configuration for an MCP server.
     * @param oauthConfig - OAuth configuration to validate
     * @returns Validation result
    public async validateOAuthConfiguration(
    ): Promise<{ valid: boolean; error?: string }> {
            return { valid: false, error: 'OAuth issuer URL is required' };
            // Try to discover metadata
            const metadata = await this.discovery.getMetadata(oauthConfig.OAuthIssuerURL, contextUser);
            // Check if DCR is available or pre-configured credentials are provided
            if (!metadata.registration_endpoint && !oauthConfig.OAuthClientID) {
                    valid: false,
                    error: 'Authorization server does not support DCR and no pre-configured client credentials are provided'
            // Check PKCE support if required
            if (oauthConfig.OAuthRequirePKCE !== false) {
                const supportsPKCE = await this.discovery.supportsPKCE(oauthConfig.OAuthIssuerURL, contextUser);
                if (!supportsPKCE) {
                        error: 'Authorization server does not support PKCE with S256 method'
            return { valid: true };
                error: `Failed to validate OAuth configuration: ${errorMessage}`
     * Marks a connection as requiring re-authorization.
     * Clears stored tokens and returns a message for the user.
     * @param reason - Reason for re-authorization
    public async markRequiresReauthorization(
            await this.tokenManager.revokeCredentials(connectionId, contextUser);
            LogStatus(`[OAuth] Marked connection ${connectionId} as requiring re-authorization: ${reason}`);
            LogError(`[OAuth] Failed to mark requires reauthorization: ${error}`);
     * Clears expired authorization states for cleanup.
     * @returns Number of states cleared
    public async clearExpiredAuthorizationStates(contextUser: UserInfo): Promise<number> {
            const expired = await rv.RunView<{ ID: string }>({
                EntityName: ENTITY_OAUTH_AUTHORIZATION_STATES,
                ExtraFilter: `Status='Pending' AND ExpiresAt < '${now}'`,
            if (!expired.Success || !expired.Results) {
            let cleared = 0;
            for (const record of expired.Results) {
                    const entity = await md.GetEntityObject<BaseEntity>(ENTITY_OAUTH_AUTHORIZATION_STATES, contextUser);
                    const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: record.ID }]);
                        entity.Set('Status', 'Expired');
                        cleared++;
                    LogError(`[OAuth] Failed to expire authorization state ${record.ID}: ${e}`);
            if (cleared > 0) {
                LogStatus(`[OAuth] Cleared ${cleared} expired authorization states`);
            return cleared;
            LogError(`[OAuth] Failed to clear expired states: ${error}`);
     * Builds the OAuth callback redirect URI.
     * OAuth routes are registered at /oauth (not under /api/v1)
    private buildRedirectUri(publicUrl: string): string {
        const base = publicUrl.replace(/\/$/, '');
        return `${base}/oauth/callback`;
     * Exchanges an authorization code for tokens.
    private async exchangeCodeForTokens(
        codeVerifier: string,
        tokenEndpoint: string
    ): Promise<OAuthTokenResponse> {
        const body = new URLSearchParams();
        body.set('grant_type', 'authorization_code');
        body.set('code', code);
        body.set('redirect_uri', redirectUri);
        body.set('client_id', clientId);
        body.set('code_verifier', codeVerifier);
        const headers: Record<string, string> = {
            'Content-Type': 'application/x-www-form-urlencoded',
        if (clientSecret) {
            const credentials = Buffer.from(`${clientId}:${clientSecret}`).toString('base64');
            headers['Authorization'] = `Basic ${credentials}`;
        const response = await fetch(tokenEndpoint, {
            headers,
            body: body.toString()
            let errorCode = `HTTP ${response.status}`;
                if (errorJson.error) {
                    errorCode = errorJson.error;
                        errorCode += `: ${errorJson.error_description}`;
                errorCode += `: ${errorBody}`;
            throw new Error(errorCode);
        return await response.json() as OAuthTokenResponse;
     * Calculates expiration timestamp from expires_in.
    private calculateExpiresAt(expiresIn?: number): number {
        const now = Math.floor(Date.now() / 1000);
        return now + (expiresIn ?? 3600);
     * Saves authorization state to database.
    private async saveAuthorizationState(
        state: OAuthAuthorizationState,
            entity.Set('MCPServerConnectionID', state.connectionId);
            entity.Set('UserID', state.userId);
            entity.Set('StateParameter', state.stateParameter);
            entity.Set('CodeVerifier', state.pkce.codeVerifier);
            entity.Set('CodeChallenge', state.pkce.codeChallenge);
            entity.Set('RedirectURI', state.redirectUri);
            entity.Set('RequestedScopes', state.requestedScopes ?? null);
            entity.Set('Status', state.status);
            entity.Set('AuthorizationURL', state.authorizationUrl);
            entity.Set('InitiatedAt', state.initiatedAt);
            entity.Set('ExpiresAt', state.expiresAt);
            entity.Set('FrontendReturnURL', state.frontendReturnUrl ?? null);
            state.id = entity.Get('ID');
            LogError(`[OAuth] Failed to save authorization state: ${error}`);
     * Loads authorization state from database.
    private async loadAuthorizationState(
    ): Promise<OAuthAuthorizationState | null> {
                UserID: string;
                StateParameter: string;
                CodeVerifier: string;
                CodeChallenge: string;
                RedirectURI: string;
                RequestedScopes: string | null;
                Status: OAuthAuthorizationStatus;
                AuthorizationURL: string;
                ErrorCode: string | null;
                ErrorDescription: string | null;
                InitiatedAt: Date;
                CompletedAt: Date | null;
                FrontendReturnURL: string | null;
                ExtraFilter: `StateParameter='${stateParameter.replace(/'/g, "''")}'`,
                userId: record.UserID,
                stateParameter: record.StateParameter,
                pkce: {
                    codeVerifier: record.CodeVerifier,
                    codeChallenge: record.CodeChallenge,
                    codeChallengeMethod: 'S256'
                redirectUri: record.RedirectURI,
                requestedScopes: record.RequestedScopes ?? undefined,
                authorizationUrl: record.AuthorizationURL,
                errorCode: record.ErrorCode ?? undefined,
                errorDescription: record.ErrorDescription ?? undefined,
                initiatedAt: new Date(record.InitiatedAt),
                expiresAt: new Date(record.ExpiresAt),
                completedAt: record.CompletedAt ? new Date(record.CompletedAt) : undefined,
                frontendReturnUrl: record.FrontendReturnURL ?? undefined
            LogError(`[OAuth] Failed to load authorization state: ${error}`);
     * Updates authorization state in database.
    private async updateAuthorizationState(
        stateId: string,
        status: OAuthAuthorizationStatus,
        errorCode?: string,
        errorDescription?: string
            const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: stateId }]);
                if (status === 'Completed' || status === 'Failed' || status === 'Expired') {
                    entity.Set('CompletedAt', new Date());
                if (errorCode) {
                    entity.Set('ErrorCode', errorCode);
                if (errorDescription) {
                    entity.Set('ErrorDescription', errorDescription);
            LogError(`[OAuth] Failed to update authorization state: ${error}`);
     * Loads MCP Server connection config with OAuth settings.
    private async loadServerConfig(
            // First get connection to get server ID
            const connResult = await rv.RunView<{ MCPServerID: string }>({
                EntityName: 'MJ: MCP Server Connections',
                ExtraFilter: `ID='${connectionId}'`,
                Fields: ['MCPServerID'],
            if (!connResult.Success || !connResult.Results || connResult.Results.length === 0) {
            const serverId = connResult.Results[0].MCPServerID;
            // Then get server OAuth config
            const serverResult = await rv.RunView<{
                OAuthIssuerURL: string | null;
                OAuthScopes: string | null;
                OAuthClientID: string | null;
                OAuthClientSecretEncrypted: string | null;
                EntityName: 'MJ: MCP Servers',
                Fields: ['OAuthIssuerURL', 'OAuthScopes', 'OAuthClientID', 'OAuthClientSecretEncrypted'],
            if (!serverResult.Success || !serverResult.Results || serverResult.Results.length === 0) {
                MCPServerID: serverId,
                OAuthIssuerURL: server.OAuthIssuerURL ?? undefined,
                OAuthScopes: server.OAuthScopes ?? undefined,
                OAuthClientID: server.OAuthClientID ?? undefined,
                OAuthClientSecretEncrypted: server.OAuthClientSecretEncrypted ?? undefined
            LogError(`[OAuth] Failed to load server config: ${error}`);
