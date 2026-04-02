 * @fileoverview OAuth Token Management
 * Handles token storage, refresh, and validation for OAuth-protected MCP connections.
 * Stores tokens via CredentialEngine for consistent encryption and audit logging.
 * @module @memberjunction/ai-mcp-client/oauth/TokenManager
    OAuthTokenResponse,
    TokenRefreshResult,
    OAuthClientRegistration
/** Entity name for OAuth tokens */
const ENTITY_OAUTH_TOKENS = 'MJ: O Auth Tokens';
/** Credential type name for MCP OAuth tokens */
const CREDENTIAL_TYPE_MCP_OAUTH = 'MCP OAuth Token';
/** Default token expiration threshold in seconds (5 minutes) */
const DEFAULT_EXPIRATION_THRESHOLD_SECONDS = 300;
 * Manages OAuth token storage, validation, and refresh.
 * - Secure token storage via CredentialEngine
 * - Proactive token refresh before expiration
 * - Concurrent refresh protection via mutex pattern
 * - Retry logic with exponential backoff
 * const tokenManager = new TokenManager();
 * // Store new tokens
 * await tokenManager.storeTokens(connectionId, tokens, contextUser);
 * // Check token validity
 * const isValid = await tokenManager.isTokenValid(connectionId, contextUser);
 * // Get valid token (refreshes if needed)
 * const tokens = await tokenManager.getValidTokens(connectionId, clientReg, contextUser);
export class TokenManager {
    /** Expiration threshold in seconds (tokens expiring within this time are considered expired) */
    private readonly expirationThresholdSeconds: number;
    /** Locks for concurrent refresh protection */
    private readonly refreshLocks: Map<string, Promise<TokenRefreshResult>> = new Map();
    /** In-memory token cache for faster access */
    private readonly tokenCache: Map<string, OAuthTokenSet> = new Map();
    constructor(expirationThresholdSeconds = DEFAULT_EXPIRATION_THRESHOLD_SECONDS) {
        this.expirationThresholdSeconds = expirationThresholdSeconds;
     * Stores new OAuth tokens for a connection.
     * Creates or updates a credential via CredentialEngine, then stores
     * metadata in the OAuthToken table.
     * @param tokens - Token set to store
    public async storeTokens(
        tokens: OAuthTokenSet,
            // Ensure CredentialEngine is loaded
            // Check if OAuthToken record already exists
            const existing = await rv.RunView<{ ID: string; CredentialID: string | null }>({
                EntityName: ENTITY_OAUTH_TOKENS,
                ExtraFilter: `MCPServerConnectionID='${connectionId}'`,
                Fields: ['ID', 'CredentialID'],
            let credentialId: string;
            // Prepare credential values
            const credentialValues: Record<string, string> = {
                accessToken: tokens.accessToken
            if (tokens.refreshToken) {
                credentialValues.refreshToken = tokens.refreshToken;
                const existingRecord = existing.Results[0];
                if (existingRecord.CredentialID) {
                    // Update existing credential
                    await CredentialEngine.Instance.updateCredential(
                        existingRecord.CredentialID,
                        credentialValues,
                    credentialId = existingRecord.CredentialID;
                    // Create new credential (migrating from old schema)
                    const credential = await CredentialEngine.Instance.storeCredential(
                        CREDENTIAL_TYPE_MCP_OAUTH,
                        `MCP OAuth - ${connectionId.substring(0, 8)}`,
                            description: `OAuth tokens for MCP server connection ${connectionId}`,
                            isDefault: false
                    credentialId = credential.ID;
                // Update OAuthToken metadata
                const entity = await md.GetEntityObject<BaseEntity>(ENTITY_OAUTH_TOKENS, contextUser);
                const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: existingRecord.ID }]);
                entity.Set('CredentialID', credentialId);
                entity.Set('TokenType', tokens.tokenType);
                entity.Set('ExpiresAt', new Date(tokens.expiresAt * 1000));
                entity.Set('Scope', tokens.scope ?? null);
                entity.Set('IssuerURL', tokens.issuer);
                entity.Set('LastRefreshAt', tokens.lastRefreshAt ? new Date(tokens.lastRefreshAt * 1000) : null);
                entity.Set('RefreshCount', tokens.refreshCount);
                // Create new credential
                // Create new OAuthToken record
                entity.Set('MCPServerConnectionID', connectionId);
            this.tokenCache.set(connectionId, tokens);
            LogStatus(`[OAuth] Stored tokens for connection ${connectionId}`);
            LogError(`[OAuth] Failed to store tokens: ${error}`);
     * Loads tokens for a connection.
     * @returns Token set or null if not found
    public async loadTokens(
    ): Promise<OAuthTokenSet | null> {
        const cached = this.tokenCache.get(connectionId);
            // Load OAuthToken metadata
                CredentialID: string | null;
                TokenType: string;
                LastRefreshAt: Date | null;
                RefreshCount: number;
            if (!record.CredentialID) {
                // No credential linked - token needs to be re-acquired
                LogStatus(`[OAuth] No credential linked for connection ${connectionId}`);
            // Load the credential values via CredentialEngine
            const resolvedCredential = await CredentialEngine.Instance.getCredential<{
                accessToken: string;
                refreshToken?: string;
            }>(
                    credentialId: record.CredentialID,
                accessToken: resolvedCredential.values.accessToken,
                tokenType: record.TokenType,
                expiresAt: Math.floor(new Date(record.ExpiresAt).getTime() / 1000),
                refreshToken: resolvedCredential.values.refreshToken,
                lastRefreshAt: record.LastRefreshAt
                    ? Math.floor(new Date(record.LastRefreshAt).getTime() / 1000)
                refreshCount: record.RefreshCount ?? 0
            // Log but don't throw - token might not exist yet
            LogStatus(`[OAuth] No stored tokens found for connection ${connectionId}: ${error}`);
     * Checks if stored tokens are valid (not expired or expiring soon).
     * @returns true if tokens are valid
    public async isTokenValid(connectionId: string, contextUser: UserInfo): Promise<boolean> {
        const tokens = await this.loadTokens(connectionId, contextUser);
        if (!tokens) {
        return this.isTokenSetValid(tokens);
     * Checks if a token set is valid (not expired or expiring soon).
     * @param tokens - Token set to check
    public isTokenSetValid(tokens: OAuthTokenSet): boolean {
        const threshold = now + this.expirationThresholdSeconds;
        return tokens.expiresAt > threshold;
     * Gets valid tokens for a connection, refreshing if needed.
     * @param clientRegistration - Client registration for token refresh
     * @param tokenEndpoint - Token endpoint URL
     * @returns Valid token set
     * @throws Error if tokens cannot be obtained
    public async getValidTokens(
        clientRegistration: OAuthClientRegistration,
        tokenEndpoint: string,
    ): Promise<OAuthTokenSet> {
            throw new Error('No tokens stored for this connection. Authorization required.');
        // Check if tokens are still valid
        if (this.isTokenSetValid(tokens)) {
        // Check if we have a refresh token
        if (!tokens.refreshToken) {
            throw new Error('Access token expired and no refresh token available. Re-authorization required.');
        // Refresh the tokens
        const refreshResult = await this.refreshTokens(
            tokens,
            clientRegistration,
            tokenEndpoint,
        if (!refreshResult.success || !refreshResult.tokens) {
                throw new Error(`Token refresh failed: ${refreshResult.errorMessage}. Re-authorization required.`);
            throw new Error(`Token refresh failed: ${refreshResult.errorMessage}`);
        return refreshResult.tokens;
     * Refreshes tokens using the refresh token.
     * Uses concurrent refresh protection to prevent multiple simultaneous
     * refresh operations for the same connection.
     * @param currentTokens - Current token set with refresh token
     * @param clientRegistration - Client registration for authentication
     * @returns Refresh result
    public async refreshTokens(
        currentTokens: OAuthTokenSet,
    ): Promise<TokenRefreshResult> {
        // Check for existing refresh operation
        const existingRefresh = this.refreshLocks.get(connectionId);
        if (existingRefresh) {
            LogStatus(`[OAuth] Waiting for existing refresh operation for ${connectionId}`);
            return existingRefresh;
        // Start new refresh with lock
        const refreshPromise = this.performRefresh(
            currentTokens,
        this.refreshLocks.set(connectionId, refreshPromise);
            return await refreshPromise;
            this.refreshLocks.delete(connectionId);
     * Performs the actual token refresh with retry logic.
    private async performRefresh(
        const maxRetries = 3;
        const baseDelayMs = 1000;
        for (let attempt = 1; attempt <= maxRetries; attempt++) {
                LogStatus(`[OAuth] Refreshing tokens for ${connectionId} (attempt ${attempt}/${maxRetries})`);
                const newTokens = await this.exchangeRefreshToken(
                    currentTokens.refreshToken!,
                    tokenEndpoint
                // Build updated token set
                const refreshedTokens: OAuthTokenSet = {
                    accessToken: newTokens.access_token,
                    tokenType: newTokens.token_type ?? 'Bearer',
                    expiresAt: this.calculateExpiresAt(newTokens.expires_in),
                    refreshToken: newTokens.refresh_token ?? currentTokens.refreshToken,
                    scope: newTokens.scope ?? currentTokens.scope,
                    issuer: currentTokens.issuer,
                    refreshCount: currentTokens.refreshCount + 1
                // Store the new tokens
                await this.storeTokens(connectionId, refreshedTokens, contextUser);
                LogStatus(`[OAuth] Successfully refreshed tokens for ${connectionId}`);
                // Audit log: Token refreshed (T049)
                await auditLogger.logTokenRefreshed({
                    issuerUrl: currentTokens.issuer,
                    newExpiresAt: new Date(refreshedTokens.expiresAt * 1000),
                    refreshCount: refreshedTokens.refreshCount
                    tokens: refreshedTokens
                LogError(`[OAuth] Token refresh attempt ${attempt} failed: ${errorMessage}`);
                // Check if error is retryable
                if (!OAuthErrorMessages.isRetryable(errorMessage)) {
                    // Audit log: Token refresh failed (T050)
                    await auditLogger.logTokenRefreshFailed({
                        errorMessage: OAuthErrorMessages.getUserMessage(errorMessage),
                        requiresReauthorization: OAuthErrorMessages.requiresReauthorization(errorMessage)
                // Wait before retry with exponential backoff
                if (attempt < maxRetries) {
                    const delayMs = baseDelayMs * Math.pow(2, attempt - 1);
                    await this.sleep(delayMs);
        // Audit log: Token refresh failed after all retries (T050)
            errorMessage: 'Token refresh failed after multiple attempts',
            errorMessage: 'Token refresh failed after multiple attempts. Please try reconnecting.',
     * Exchanges a refresh token for new tokens.
    private async exchangeRefreshToken(
        refreshToken: string,
        // Build request body
        body.set('grant_type', 'refresh_token');
        body.set('refresh_token', refreshToken);
        body.set('client_id', clientRegistration.clientId);
        if (clientRegistration.scope) {
            body.set('scope', clientRegistration.scope);
        // Build headers
        // Add client authentication if client secret exists
        if (clientRegistration.clientSecret) {
            const credentials = Buffer.from(
                `${clientRegistration.clientId}:${clientRegistration.clientSecret}`
            ).toString('base64');
        // Default to 1 hour if not specified
     * Revokes stored credentials for a connection.
     * Deletes both the OAuthToken metadata and the associated credential.
    public async revokeCredentials(connectionId: string, contextUser: UserInfo): Promise<void> {
                const record = existing.Results[0];
                // Delete the OAuthToken record first
                const tokenEntity = await md.GetEntityObject<BaseEntity>(ENTITY_OAUTH_TOKENS, contextUser);
                const tokenKey = new CompositeKey([{ FieldName: 'ID', Value: record.ID }]);
                const tokenLoaded = await tokenEntity.InnerLoad(tokenKey);
                if (tokenLoaded) {
                    await tokenEntity.Delete();
                // Delete the associated credential if it exists
                if (record.CredentialID) {
                        const credEntity = await md.GetEntityObject<BaseEntity>('MJ: Credentials', contextUser);
                        const credKey = new CompositeKey([{ FieldName: 'ID', Value: record.CredentialID }]);
                        const credLoaded = await credEntity.InnerLoad(credKey);
                        if (credLoaded) {
                            await credEntity.Delete();
                    } catch (credError) {
                        LogError(`[OAuth] Failed to delete credential: ${credError}`);
                        // Continue anyway - token record is deleted
            // Clear from in-memory cache
            this.tokenCache.delete(connectionId);
            LogStatus(`[OAuth] Revoked credentials for connection ${connectionId}`);
            // Audit log: Credentials revoked (T051)
            await auditLogger.logCredentialsRevoked({
                revokedBy: contextUser.ID
            LogError(`[OAuth] Failed to revoke credentials: ${error}`);
     * Handles a refresh failure by determining if re-authorization is needed.
     * @param errorMessage - The error message from the failed refresh
     * @returns Object indicating whether re-authorization is required
    public handleRefreshFailure(errorMessage: string): { requiresReauthorization: boolean; userMessage: string } {
            requiresReauthorization: mapped.requiresReauthorization,
            userMessage: mapped.userMessage
     * Sleep helper for retry delays.
    private sleep(ms: number): Promise<void> {
        return new Promise(resolve => setTimeout(resolve, ms));
