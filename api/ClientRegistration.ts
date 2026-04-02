 * @fileoverview OAuth 2.0 Dynamic Client Registration (RFC 7591)
 * Handles dynamic client registration with authorization servers that support DCR.
 * Falls back to pre-configured client credentials when DCR is not available.
 * @module @memberjunction/ai-mcp-client/oauth/ClientRegistration
    AuthServerMetadata,
    DCRRequest,
    DCRResponse,
    OAuthClientRegistration,
    OAuthClientRegistrationStatus
/** Entity name for OAuth client registrations */
const ENTITY_OAUTH_CLIENT_REGISTRATIONS = 'MJ: O Auth Client Registrations';
 * Handles OAuth Dynamic Client Registration (DCR) for MCP connections.
 * Implements RFC 7591 DCR with fallback to pre-configured client credentials.
 * Stores registrations in the database for reuse across connections.
 * - Automatic DCR when supported by authorization server
 * - Fallback to pre-configured credentials
 * - Registration persistence and reuse
 * - Client secret expiration tracking
 * const registration = new ClientRegistration();
 * // Get or create client credentials
 * const client = await registration.getOrRegisterClient(
 *     connectionId,
 *     serverId,
 *     metadata,
 *         redirectUri: 'https://api.example.com/oauth/callback',
 *         scopes: 'read write'
 *     contextUser
 * // Use client credentials in authorization request
 * authUrl.searchParams.set('client_id', client.clientId);
export class ClientRegistration {
    /** Default client name prefix */
    private static readonly CLIENT_NAME_PREFIX = 'MemberJunction MCP Client';
     * Gets existing client registration or registers a new one.
     * If a valid registration exists for this connection, returns it.
     * Otherwise, attempts DCR if supported, or uses pre-configured credentials.
     * @param serverId - MCP Server ID
     * @param metadata - Authorization server metadata
     * @param options - Registration options
     * @returns Client registration with credentials
     * @throws Error if no valid client credentials can be obtained
    public async getOrRegisterClient(
            redirectUri: string;
            scopes?: string;
            preConfiguredClientId?: string;
            preConfiguredClientSecret?: string;
    ): Promise<OAuthClientRegistration> {
        // Check for existing valid registration
        const existing = await this.loadRegistration(connectionId, metadata.issuer, contextUser);
        if (existing && existing.status === 'Active') {
            // Check if client secret has expired
            if (existing.clientSecretExpiresAt && new Date() >= existing.clientSecretExpiresAt) {
                LogStatus(`[OAuth] Client secret expired for connection ${connectionId}, re-registering`);
                await this.deleteRegistration(existing.id!, contextUser);
            // Check if the requested redirect_uri is in the registered redirect_uris
            else if (!existing.redirectUris.includes(options.redirectUri)) {
                LogStatus(`[OAuth] Redirect URI changed for connection ${connectionId} (was: ${existing.redirectUris.join(', ')}, now: ${options.redirectUri}), re-registering`);
                LogStatus(`[OAuth] Using existing client registration for connection ${connectionId}`);
                return existing;
        // Check for pre-configured credentials
        if (options.preConfiguredClientId) {
            LogStatus(`[OAuth] Using pre-configured client credentials for connection ${connectionId}`);
            return this.createPreConfiguredRegistration(
                serverId,
                metadata.issuer,
                options.preConfiguredClientId,
                options.preConfiguredClientSecret,
                options.redirectUri,
                options.scopes,
        // Attempt DCR if supported
        if (metadata.registration_endpoint) {
            LogStatus(`[OAuth] Attempting DCR at ${metadata.registration_endpoint}`);
                return await this.registerClient(
                LogError(`[OAuth] DCR failed: ${errorMsg}`);
                    `Dynamic Client Registration failed and no pre-configured credentials available. ` +
                    `Error: ${errorMsg}. ` +
                    `Please configure OAuthClientID and OAuthClientSecretEncrypted on the MCP Server.`
        // No DCR and no pre-configured credentials
            `Authorization server does not support Dynamic Client Registration and no pre-configured ` +
            `credentials are available. Please configure OAuthClientID and OAuthClientSecretEncrypted ` +
            `on the MCP Server.`
     * Registers a new client with the authorization server via DCR.
     * @returns New client registration
    private async registerClient(
        if (!metadata.registration_endpoint) {
            throw new Error('Authorization server does not support Dynamic Client Registration');
        // Build DCR request
        const clientName = options.serverName
            ? `${ClientRegistration.CLIENT_NAME_PREFIX} - ${options.serverName}`
            : ClientRegistration.CLIENT_NAME_PREFIX;
        const dcrRequest: DCRRequest = {
            client_name: clientName,
            redirect_uris: [options.redirectUri],
            grant_types: ['authorization_code', 'refresh_token'],
            response_types: ['code'],
            token_endpoint_auth_method: this.selectAuthMethod(metadata),
            scope: options.scopes
        // Send DCR request
        const response = await fetch(metadata.registration_endpoint, {
                'Content-Type': 'application/json',
            body: JSON.stringify(dcrRequest)
            const errorBody = await response.text();
            let errorMessage = `HTTP ${response.status}`;
                const errorJson = JSON.parse(errorBody);
                if (errorJson.error_description) {
                    errorMessage = `${errorJson.error}: ${errorJson.error_description}`;
                } else if (errorJson.error) {
                    errorMessage = errorJson.error;
                errorMessage += `: ${errorBody}`;
        const dcrResponse = await response.json() as DCRResponse;
        // Validate response has required fields
        if (!dcrResponse.client_id) {
            throw new Error('DCR response missing required client_id');
        // Create and save registration
        const registration: OAuthClientRegistration = {
            issuer: metadata.issuer,
            clientId: dcrResponse.client_id,
            clientSecret: dcrResponse.client_secret,
            clientIdIssuedAt: dcrResponse.client_id_issued_at
                ? new Date(this.normalizeTimestamp(dcrResponse.client_id_issued_at))
            clientSecretExpiresAt: dcrResponse.client_secret_expires_at
                ? dcrResponse.client_secret_expires_at === 0
                    : new Date(this.normalizeTimestamp(dcrResponse.client_secret_expires_at))
            registrationAccessToken: dcrResponse.registration_access_token,
            registrationClientUri: dcrResponse.registration_client_uri,
            redirectUris: dcrResponse.redirect_uris ?? [options.redirectUri],
            grantTypes: dcrResponse.grant_types ?? ['authorization_code', 'refresh_token'],
            responseTypes: dcrResponse.response_types ?? ['code'],
            scope: dcrResponse.scope ?? options.scopes,
            registrationResponse: JSON.stringify(dcrResponse)
        // Save to database
        const savedRegistration = await this.saveRegistration(registration, contextUser);
        LogStatus(`[OAuth] Successfully registered client for connection ${connectionId}`);
        return savedRegistration;
     * Creates a registration record for pre-configured credentials.
    private async createPreConfiguredRegistration(
        issuer: string,
        clientId: string,
        clientSecret: string | undefined,
        redirectUri: string,
        scopes: string | undefined,
            issuer,
            clientId,
            clientSecret,
            redirectUris: [redirectUri],
            grantTypes: ['authorization_code', 'refresh_token'],
            responseTypes: ['code'],
            scope: scopes,
            registrationResponse: JSON.stringify({ pre_configured: true })
        return await this.saveRegistration(registration, contextUser);
     * Selects the best token endpoint authentication method.
    private selectAuthMethod(metadata: AuthServerMetadata): string {
        const supported = metadata.token_endpoint_auth_methods_supported ?? ['client_secret_basic'];
        // Prefer client_secret_basic, then client_secret_post
        if (supported.includes('client_secret_basic')) {
            return 'client_secret_basic';
        if (supported.includes('client_secret_post')) {
            return 'client_secret_post';
        // If only none is supported, return that (public client)
        if (supported.includes('none')) {
            return 'none';
        // Default to basic
     * Normalizes a timestamp that may be in seconds or milliseconds.
     * OAuth specs (RFC 7591) specify timestamps in seconds since epoch,
     * but some providers (e.g., Context7/Clerk) return milliseconds.
     * This method detects and handles both formats.
     * @param timestamp - Unix timestamp in seconds or milliseconds
     * @returns Timestamp in milliseconds suitable for Date constructor
    private normalizeTimestamp(timestamp: number): number {
        // Timestamps in seconds for years 2000-3000 are roughly 10 digits (946684800 to 32503680000)
        // Timestamps in milliseconds are 13 digits
        // If the value is greater than a reasonable seconds value (year ~2286), it's likely milliseconds
        const maxReasonableSeconds = 10000000000; // Year 2286 in seconds
        return timestamp > maxReasonableSeconds ? timestamp : timestamp * 1000;
     * Loads an existing registration from the database.
    private async loadRegistration(
    ): Promise<OAuthClientRegistration | null> {
                IssuerURL: string;
                ClientID: string;
                ClientSecretEncrypted: string | null;
                ClientIDIssuedAt: Date | null;
                ClientSecretExpiresAt: Date | null;
                RegistrationAccessToken: string | null;
                RegistrationClientURI: string | null;
                RedirectURIs: string;
                GrantTypes: string;
                ResponseTypes: string;
                Scope: string | null;
                Status: OAuthClientRegistrationStatus;
                RegistrationResponse: string;
                EntityName: ENTITY_OAUTH_CLIENT_REGISTRATIONS,
                ExtraFilter: `MCPServerConnectionID='${connectionId}' AND IssuerURL='${issuer.replace(/'/g, "''")}'`,
                id: record.ID,
                connectionId: record.MCPServerConnectionID,
                serverId: record.MCPServerID,
                issuer: record.IssuerURL,
                clientId: record.ClientID,
                clientSecret: record.ClientSecretEncrypted ?? undefined,
                clientIdIssuedAt: record.ClientIDIssuedAt ? new Date(record.ClientIDIssuedAt) : undefined,
                clientSecretExpiresAt: record.ClientSecretExpiresAt ? new Date(record.ClientSecretExpiresAt) : undefined,
                registrationAccessToken: record.RegistrationAccessToken ?? undefined,
                registrationClientUri: record.RegistrationClientURI ?? undefined,
                redirectUris: JSON.parse(record.RedirectURIs),
                grantTypes: JSON.parse(record.GrantTypes),
                responseTypes: JSON.parse(record.ResponseTypes),
                scope: record.Scope ?? undefined,
                status: record.Status,
                registrationResponse: record.RegistrationResponse
            LogError(`[OAuth] Failed to load client registration: ${error}`);
     * Saves a registration to the database.
    private async saveRegistration(
        registration: OAuthClientRegistration,
            // Check for existing record
                ExtraFilter: `MCPServerConnectionID='${registration.connectionId}'`,
            // Use BaseEntity with Set() method for dynamic field access
            const entity = await md.GetEntityObject<BaseEntity>(ENTITY_OAUTH_CLIENT_REGISTRATIONS, contextUser);
                entity.Set('MCPServerConnectionID', registration.connectionId);
            entity.Set('MCPServerID', registration.serverId);
            entity.Set('IssuerURL', registration.issuer);
            entity.Set('ClientID', registration.clientId);
            entity.Set('ClientSecretEncrypted', registration.clientSecret ?? null);
            entity.Set('ClientIDIssuedAt', registration.clientIdIssuedAt ?? null);
            entity.Set('ClientSecretExpiresAt', registration.clientSecretExpiresAt ?? null);
            entity.Set('RegistrationAccessToken', registration.registrationAccessToken ?? null);
            entity.Set('RegistrationClientURI', registration.registrationClientUri ?? null);
            entity.Set('RedirectURIs', JSON.stringify(registration.redirectUris));
            entity.Set('GrantTypes', JSON.stringify(registration.grantTypes));
            entity.Set('ResponseTypes', JSON.stringify(registration.responseTypes));
            entity.Set('Scope', registration.scope ?? null);
            entity.Set('Status', registration.status);
            entity.Set('RegistrationResponse', registration.registrationResponse);
            registration.id = entity.Get('ID');
            return registration;
            LogError(`[OAuth] Failed to save client registration: ${error}`);
     * Deletes a registration from the database.
    private async deleteRegistration(registrationId: string, contextUser: UserInfo): Promise<void> {
            const compositeKey = new CompositeKey([{ FieldName: 'ID', Value: registrationId }]);
            LogError(`[OAuth] Failed to delete client registration: ${error}`);
     * Updates a registration's status.
    public async updateRegistrationStatus(
        registrationId: string,
        status: OAuthClientRegistrationStatus,
                entity.Set('Status', status);
            LogError(`[OAuth] Failed to update registration status: ${error}`);
