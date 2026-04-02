import * as crypto from "crypto";
 * Action that handles OAuth 2.0 authentication flows
 * // Authorization Code flow - Step 1: Get auth URL
 * const authResult = await runAction({
 *   ActionName: 'OAuth Flow',
 *     Value: 'GetAuthorizationURL'
 *     Name: 'Provider',
 *     Value: 'github'
 *     Name: 'ClientID',
 *     Value: 'your-client-id'
 *     Name: 'RedirectURI',
 *     Value: 'http://localhost:3000/callback'
 *     Name: 'Scopes',
 *     Value: ['user', 'repo']
 * // Step 2: Exchange code for token
 * const tokenResult = await runAction({
 *     Value: 'ExchangeCodeForToken'
 *     Name: 'ClientSecret',
 *     Value: 'your-client-secret'
 *     Name: 'Code',
 *     Value: 'auth-code-from-callback'
 * // Client Credentials flow
 *     Value: 'ClientCredentials'
 *     Value: 'custom'
 *     Name: 'TokenEndpoint',
 *     Value: 'https://api.example.com/oauth/token'
@RegisterClass(BaseAction, "OAuth Flow")
export class OAuthFlowAction extends BaseAction {
    // Common OAuth provider configurations
    private readonly providers: Record<string, any> = {
        github: {
            authorizationEndpoint: 'https://github.com/login/oauth/authorize',
            tokenEndpoint: 'https://github.com/login/oauth/access_token',
            scopeSeparator: ' '
            authorizationEndpoint: 'https://accounts.google.com/o/oauth2/v2/auth',
            tokenEndpoint: 'https://oauth2.googleapis.com/token',
        microsoft: {
            authorizationEndpoint: 'https://login.microsoftonline.com/common/oauth2/v2.0/authorize',
            tokenEndpoint: 'https://login.microsoftonline.com/common/oauth2/v2.0/token',
        linkedin: {
            authorizationEndpoint: 'https://www.linkedin.com/oauth/v2/authorization',
            tokenEndpoint: 'https://www.linkedin.com/oauth/v2/accessToken',
     * Handles OAuth 2.0 authentication flows
     *   - Operation: "GetAuthorizationURL" | "ExchangeCodeForToken" | "RefreshToken" | "ClientCredentials" (required)
     *   - Provider: Provider name or "custom" (required)
     *   - ClientID: OAuth client ID (required)
     *   - ClientSecret: OAuth client secret (required for token operations)
     *   - RedirectURI: Callback URL (required for auth code flow)
     *   - Scopes: Array of scopes (optional)
     *   - State: State parameter for CSRF protection (optional, auto-generated if not provided)
     *   - Code: Authorization code (for ExchangeCodeForToken)
     *   - RefreshToken: Refresh token (for RefreshToken operation)
     *   - AuthorizationEndpoint: Custom auth endpoint (for custom provider)
     *   - TokenEndpoint: Custom token endpoint (for custom provider)
     *   - ScopeSeparator: Custom scope separator (default: space)
     * @returns OAuth flow result (authorization URL or tokens)
            const operation = this.getParamValue(params, 'operation');
            if (!operation) {
                    Message: "Operation parameter is required",
                    ResultCode: "MISSING_OPERATION"
            switch (operation.toLowerCase()) {
                case 'getauthorizationurl':
                    return await this.getAuthorizationURL(params);
                case 'exchangecodefortoken':
                    return await this.exchangeCodeForToken(params);
                case 'refreshtoken':
                    return await this.refreshToken(params);
                case 'clientcredentials':
                    return await this.clientCredentials(params);
                        Message: `Invalid operation: ${operation}. Must be GetAuthorizationURL, ExchangeCodeForToken, RefreshToken, or ClientCredentials`,
                Message: `OAuth flow failed: ${error instanceof Error ? error.message : String(error)}`,
                ResultCode: "OAUTH_FAILED"
     * Generate authorization URL
    private async getAuthorizationURL(params: RunActionParams): Promise<ActionResultSimple> {
        const provider = this.getParamValue(params, 'provider');
        const clientId = this.getParamValue(params, 'clientid');
        const redirectUri = this.getParamValue(params, 'redirecturi');
        const scopes = this.getParamValue(params, 'scopes');
        let state = this.getParamValue(params, 'state');
        const responseType = this.getParamValue(params, 'responsetype') || 'code';
                Message: "Provider parameter is required",
                ResultCode: "MISSING_PROVIDER"
                Message: "ClientID parameter is required",
                ResultCode: "MISSING_CLIENT_ID"
                Message: "RedirectURI parameter is required",
                ResultCode: "MISSING_REDIRECT_URI"
        // Get provider config
        const providerConfig = this.getProviderConfig(provider, params);
        if (!providerConfig.authorizationEndpoint) {
                Message: "Authorization endpoint not configured for provider",
                ResultCode: "MISSING_AUTH_ENDPOINT"
        // Generate state if not provided
            state = crypto.randomBytes(16).toString('hex');
        const authUrl = new URL(providerConfig.authorizationEndpoint);
        authUrl.searchParams.set('client_id', clientId);
        authUrl.searchParams.set('response_type', responseType);
        authUrl.searchParams.set('state', state);
        // Add scopes if provided
        if (scopes) {
            const scopeString = Array.isArray(scopes) 
                ? scopes.join(providerConfig.scopeSeparator || ' ')
                : scopes;
            authUrl.searchParams.set('scope', scopeString);
        // Add provider-specific parameters
        this.addProviderSpecificParams(authUrl, provider, params);
            Name: 'AuthorizationURL',
            Value: authUrl.toString()
            Value: state
                message: "Authorization URL generated successfully",
                authorizationUrl: authUrl.toString(),
                state: state,
                provider: provider
     * Exchange authorization code for tokens
    private async exchangeCodeForToken(params: RunActionParams): Promise<ActionResultSimple> {
        const clientSecret = this.getParamValue(params, 'clientsecret');
        const code = this.getParamValue(params, 'code');
        if (!provider || !clientId || !clientSecret || !code) {
                Message: "Provider, ClientID, ClientSecret, and Code are required",
        if (!providerConfig.tokenEndpoint) {
                Message: "Token endpoint not configured for provider",
                ResultCode: "MISSING_TOKEN_ENDPOINT"
        // Prepare token request
        const tokenData: any = {
            grant_type: 'authorization_code',
            code: code
        if (redirectUri) {
            tokenData.redirect_uri = redirectUri;
        // Make token request
            const response = await axios.post(providerConfig.tokenEndpoint, tokenData, {
                transformRequest: [(data) => {
                    return Object.entries(data)
                        .map(([key, value]) => `${encodeURIComponent(key)}=${encodeURIComponent(value as string)}`)
                        .join('&');
            const tokens = response.data;
                Name: 'AccessToken',
                Value: tokens.access_token
            if (tokens.refresh_token) {
                    Name: 'RefreshToken',
                    Value: tokens.refresh_token
            if (tokens.expires_in) {
                    Name: 'ExpiresIn',
                    Value: tokens.expires_in
            if (tokens.token_type) {
                    Name: 'TokenType',
                    Value: tokens.token_type
            if (tokens.scope) {
                    Name: 'Scope',
                    Value: tokens.scope
                    message: "Token exchange successful",
                    tokenType: tokens.token_type || 'Bearer',
                    expiresIn: tokens.expires_in,
                    scope: tokens.scope,
                    hasRefreshToken: !!tokens.refresh_token
            const errorMessage = error.response?.data?.error_description || 
                               error.response?.data?.error || 
                               error.message;
                Message: `Token exchange failed: ${errorMessage}`,
                ResultCode: "TOKEN_EXCHANGE_FAILED"
     * Refresh access token
    private async refreshToken(params: RunActionParams): Promise<ActionResultSimple> {
        const refreshToken = this.getParamValue(params, 'refreshtoken');
        if (!provider || !clientId || !clientSecret || !refreshToken) {
                Message: "Provider, ClientID, ClientSecret, and RefreshToken are required",
        // Prepare refresh request
        const tokenData = {
        // Make refresh request
                    message: "Token refresh successful",
                    newRefreshToken: !!tokens.refresh_token
                Message: `Token refresh failed: ${errorMessage}`,
                ResultCode: "TOKEN_REFRESH_FAILED"
     * Client credentials flow
    private async clientCredentials(params: RunActionParams): Promise<ActionResultSimple> {
        if (!provider || !clientId || !clientSecret) {
                Message: "Provider, ClientID, and ClientSecret are required",
            grant_type: 'client_credentials',
            client_secret: clientSecret
            tokenData.scope = scopeString;
                    message: "Client credentials flow successful",
                    scope: tokens.scope
                Message: `Client credentials flow failed: ${errorMessage}`,
                ResultCode: "CLIENT_CREDENTIALS_FAILED"
     * Get provider configuration
    private getProviderConfig(provider: string, params: RunActionParams): any {
        if (provider.toLowerCase() === 'custom') {
                authorizationEndpoint: this.getParamValue(params, 'authorizationendpoint'),
                tokenEndpoint: this.getParamValue(params, 'tokenendpoint'),
                scopeSeparator: this.getParamValue(params, 'scopeseparator') || ' '
        return this.providers[provider.toLowerCase()] || {};
     * Add provider-specific parameters to authorization URL
    private addProviderSpecificParams(url: URL, provider: string, params: RunActionParams): void {
        switch (provider.toLowerCase()) {
            case 'google':
                // Add access_type for refresh token
                const accessType = this.getParamValue(params, 'accesstype');
                if (accessType) {
                    url.searchParams.set('access_type', accessType);
                // Add prompt for re-consent
                if (prompt) {
                    url.searchParams.set('prompt', prompt);
            case 'github':
                // Add login for re-authentication
                const login = this.getParamValue(params, 'login');
                if (login) {
                    url.searchParams.set('login', login);
            case 'microsoft':
                // Add prompt for behavior
                const msPrompt = this.getParamValue(params, 'prompt');
                if (msPrompt) {
                    url.searchParams.set('prompt', msPrompt);
