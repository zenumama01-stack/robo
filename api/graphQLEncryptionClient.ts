 * Result of creating an API key
    /** The raw API key - show once and cannot be recovered */
    /** The database ID of the created key */
    APIKeyID?: string;
    /** Error message if operation failed */
 * Parameters for creating an API key
    /** Human-readable label for the key */
    /** Optional scope IDs to assign */
    ScopeIDs?: string[];
 * Result of revoking an API key
export interface RevokeAPIKeyResult {
 * Client for encryption-related GraphQL operations.
 * This client provides methods for operations that require server-side
 * cryptographic processing, such as API key generation. These operations
 * cannot be performed client-side because they require secure random
 * number generation and cryptographic hashing that must match the
 * server's validation logic.
 * const encryptionClient = new GraphQLEncryptionClient(graphQLProvider);
 * const result = await encryptionClient.CreateAPIKey({
 *   Label: 'My Integration Key',
 *   Description: 'Used for external service access',
 *   ExpiresAt: new Date('2025-12-31'),
 *   ScopeIDs: ['scope-id-1', 'scope-id-2']
 *   // Show rawKey to user ONCE - cannot be recovered
 *   console.log('Save this key:', result.RawKey);
export class GraphQLEncryptionClient {
     * Creates a new GraphQLEncryptionClient instance.
     * Creates a new API key with secure server-side cryptographic hashing.
     * This method calls the server to:
     * 1. Generate a cryptographically secure random API key
     * 2. Hash the key using SHA-256 for secure storage
     * 3. Store only the hash in the database
     * 4. Return the raw key ONCE
     * **CRITICAL**: The raw key is returned only once and cannot be recovered.
     * Instruct users to save it immediately in a secure location.
     * @param params Configuration for the new API key
     * @returns Result with raw key (show once!) and database ID
     * const result = await client.CreateAPIKey({
     *   Label: 'Production Integration',
     *   Description: 'API access for our CRM system',
     *   ScopeIDs: ['entities:read', 'entities:write']
     *   alert(`Save this key now! It won't be shown again:\n${result.RawKey}`);
     *   console.error('Failed to create key:', result.Error);
    public async CreateAPIKey(params: CreateAPIKeyParams): Promise<CreateAPIKeyResult> {
            const variables = this.createAPIKeyVariables(params);
            const result = await this.executeCreateAPIKeyMutation(variables);
            return this.processCreateAPIKeyResult(result);
            return this.handleCreateAPIKeyError(e);
     * Creates the variables for the CreateAPIKey mutation
     * @param params The API key creation parameters
     * @returns The mutation variables
    private createAPIKeyVariables(params: CreateAPIKeyParams): Record<string, unknown> {
                Label: params.Label,
                Description: params.Description,
                ExpiresAt: params.ExpiresAt?.toISOString(),
                ScopeIDs: params.ScopeIDs
     * Executes the CreateAPIKey mutation
     * @param variables The mutation variables
    private async executeCreateAPIKeyMutation(variables: Record<string, unknown>): Promise<Record<string, unknown>> {
            mutation CreateAPIKey($input: CreateAPIKeyInput!) {
                CreateAPIKey(input: $input) {
                    RawKey
                    APIKeyID
     * Processes the result of the CreateAPIKey mutation
     * @returns The processed result
    private processCreateAPIKeyResult(result: Record<string, unknown>): CreateAPIKeyResult {
        const data = result as { CreateAPIKey?: CreateAPIKeyResult };
        if (!data?.CreateAPIKey) {
                Error: "Invalid response from server"
            Success: data.CreateAPIKey.Success,
            RawKey: data.CreateAPIKey.RawKey,
            APIKeyID: data.CreateAPIKey.APIKeyID,
            Error: data.CreateAPIKey.Error
     * Handles errors in the CreateAPIKey operation
    private handleCreateAPIKeyError(e: unknown): CreateAPIKeyResult {
        LogError(`Error creating API key: ${error.message}`);
            Error: `Error: ${error.message}`
     * Once revoked, an API key cannot be reactivated. Users must create a new key.
     * @param apiKeyId The database ID of the API key to revoke
     * @returns Result indicating success or failure
     * const result = await client.RevokeAPIKey('key-uuid-here');
     *   console.log('API key has been revoked');
     *   console.error('Failed to revoke:', result.Error);
    public async RevokeAPIKey(apiKeyId: string): Promise<RevokeAPIKeyResult> {
                mutation RevokeAPIKey($apiKeyId: String!) {
                    RevokeAPIKey(apiKeyId: $apiKeyId) {
            const result = await this._dataProvider.ExecuteGQL(mutation, { apiKeyId });
            const data = result as { RevokeAPIKey?: RevokeAPIKeyResult };
            if (!data?.RevokeAPIKey) {
                Success: data.RevokeAPIKey.Success,
                Error: data.RevokeAPIKey.Error
            LogError(`Error revoking API key: ${error.message}`);
