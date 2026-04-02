 * @fileoverview MCP GraphQL Resolver
 * Provides GraphQL mutations for MCP (Model Context Protocol) operations
 * including tool synchronization with progress streaming and OAuth management.
import { Resolver, Mutation, Query, Subscription, Arg, Ctx, Root, Field, ObjectType, InputType, PubSub, registerEnumType } from 'type-graphql';
import { PubSubEngine } from 'type-graphql';
import { LogError, LogStatus, UserInfo, Metadata, RunView } from '@memberjunction/core';
    MCPClientManager,
    TokenManager
} from '@memberjunction/ai-mcp-client';
import { PUSH_STATUS_UPDATES_TOPIC } from '../generic/PushStatusResolver.js';
 * Input type for syncing MCP tools
export class SyncMCPToolsInput {
     * The ID of the MCP server connection to sync tools for
     * Optional flag to force a full sync even if recently synced
    ForceSync?: boolean;
 * Output type for MCP tool sync results
export class SyncMCPToolsResult {
     * Whether the sync operation succeeded
     * Number of tools newly added
     * Number of tools updated
     * Number of tools marked as deprecated
     * Total number of tools after sync
     * Name of the MCP server that was synced
     * Connection name that was synced
     * Whether OAuth authorization is required before connecting
     * OAuth authorization URL if authorization is required
     * OAuth state parameter for tracking the authorization flow
     * Whether OAuth re-authorization is required
     * Reason for re-authorization if required
 * Input type for executing an MCP tool
export class ExecuteMCPToolInput {
     * The ID of the MCP server connection to use
     * The ID of the tool to execute (from MCP Server Tools entity)
    ToolID: string;
     * The name of the tool to execute
     * JSON string of input arguments to pass to the tool
    InputArgs?: string;
 * Output type for MCP tool execution results
export class ExecuteMCPToolResult {
     * Whether the tool execution succeeded
     * The result returned by the tool (JSON)
    Result?: Record<string, unknown> | null;
     * Execution duration in milliseconds
 * Progress message type for sync status updates
interface SyncProgressMessage {
// OAuth GraphQL Types
 * OAuth authorization status enum
enum MCPOAuthStatus {
    PENDING = 'PENDING',
    COMPLETED = 'COMPLETED',
    FAILED = 'FAILED',
    EXPIRED = 'EXPIRED'
// Register the enum with TypeGraphQL
registerEnumType(MCPOAuthStatus, {
    name: 'MCPOAuthStatus',
    description: 'Status of an OAuth authorization flow'
 * Input for initiating OAuth authorization
export class InitiateMCPOAuthInput {
    AdditionalScopes?: string;
    @Field({ nullable: true, description: 'Frontend URL to use as redirect_uri. When provided, the frontend handles the OAuth callback instead of the API server.' })
    FrontendCallbackUrl?: string;
 * Input for checking OAuth status
export class GetMCPOAuthStatusInput {
 * Input for revoking OAuth credentials
export class RevokeMCPOAuthInput {
 * Input for refreshing OAuth tokens
export class RefreshMCPOAuthTokenInput {
 * Result from initiating OAuth authorization
export class InitiateMCPOAuthResult {
    UsedDynamicRegistration?: boolean;
 * Result from checking OAuth status
export class MCPOAuthStatusResult {
    @Field(() => MCPOAuthStatus, { nullable: true })
    Status?: MCPOAuthStatus;
    ConnectionID?: string;
    AuthErrorCode?: string;
    AuthErrorDescription?: string;
    IsRetryable?: boolean;
 * Result from revoking OAuth credentials
export class RevokeMCPOAuthResult {
 * Result from refreshing OAuth tokens
export class RefreshMCPOAuthTokenResult {
 * OAuth connection status information
export class MCPOAuthConnectionStatus {
    IsOAuthEnabled: boolean;
    HasValidTokens: boolean;
    IsAccessTokenExpired?: boolean;
    TokenExpiresAt?: Date;
    HasRefreshToken?: boolean;
    RequiresReauthorization: boolean;
    IssuerUrl?: string;
    GrantedScopes?: string;
// Subscription Topics and Types
/** PubSub topic for MCP OAuth events */
export const MCP_OAUTH_EVENTS_TOPIC = 'MCP_OAUTH_EVENTS';
 * OAuth event types for subscriptions
export type MCPOAuthEventType =
 * Payload interface for OAuth subscription events
export interface MCPOAuthEventPayload {
    eventType: MCPOAuthEventType;
 * Notification type for OAuth events
export class MCPOAuthEventNotification {
    EventType: string;
 * MCP Resolver for GraphQL operations
export class MCPResolver extends ResolverBase {
     * Syncs tools from an MCP server connection to the database.
     * Publishes progress updates via the statusUpdates subscription.
     * @param input The sync input parameters
     * @param ctx The GraphQL context
     * @param pubSub PubSub engine for progress updates
     * @returns The sync result
    @Mutation(() => SyncMCPToolsResult)
    async SyncMCPTools(
        @Arg('input') input: SyncMCPToolsInput,
        @Ctx() ctx: AppContext,
    ): Promise<SyncMCPToolsResult> {
            return this.createErrorResult('User is not authenticated');
        const { ConnectionID } = input;
        const sessionId = ctx.userPayload.sessionId;
            // Check API key scope authorization
            await this.CheckAPIKeyScopeAuthorization('mcp:sync', ConnectionID, ctx.userPayload);
            // Get the MCP client manager instance and ensure it's initialized
            const publicUrl = this.getPublicUrl();
            await manager.initialize(user, { publicUrl });
            // Publish initial progress
            this.publishProgress(pubSub, sessionId, ConnectionID, 'connecting', 'Connecting to MCP server...');
            // Connect if not already connected
            const isConnected = manager.isConnected(ConnectionID);
            if (!isConnected) {
                LogStatus(`MCPResolver: Connecting to MCP server for connection ${ConnectionID}`);
                    await manager.connect(ConnectionID, { contextUser: user });
                } catch (connectError) {
                    // Check for OAuth authorization required
                    if (connectError instanceof OAuthAuthorizationRequiredError) {
                        const authError = connectError as OAuthAuthorizationRequiredError;
                        this.publishProgress(pubSub, sessionId, ConnectionID, 'error',
                            `OAuth authorization required. Please authorize at: ${authError.authorizationUrl}`);
                        return this.createOAuthRequiredResult(authError.authorizationUrl, authError.stateParameter);
                    if (connectError instanceof OAuthReauthorizationRequiredError) {
                        const reAuthError = connectError as OAuthReauthorizationRequiredError;
                            `OAuth re-authorization required: ${reAuthError.reason}`);
                        return this.createOAuthReauthorizationResult(reAuthError.reason, reAuthError.authorizationUrl, reAuthError.stateParameter);
                    const connectErrorMsg = connectError instanceof Error ? connectError.message : String(connectError);
                    this.publishProgress(pubSub, sessionId, ConnectionID, 'error', `Connection failed: ${connectErrorMsg}`);
                    return this.createErrorResult(`Failed to connect to MCP server: ${connectErrorMsg}`);
            // Get connection info for the result
            const connectionInfo = manager.getConnectionInfo(ConnectionID);
            const serverName = connectionInfo?.serverName || 'Unknown Server';
            const connectionName = connectionInfo?.connectionName || 'Unknown Connection';
            // Publish listing progress
            this.publishProgress(pubSub, sessionId, ConnectionID, 'listing', 'Discovering tools from MCP server...');
            // Perform the sync with event listening for granular progress
            LogStatus(`MCPResolver: Starting tool sync for connection ${ConnectionID}`);
            // Subscribe to manager events for this sync
            const eventHandler = (event: { type: string; data?: Record<string, unknown> }) => {
                if (event.type === 'toolsSynced') {
                    const data = event.data as { added: number; updated: number; deprecated: number; total: number } | undefined;
                    this.publishProgress(pubSub, sessionId, ConnectionID, 'complete', 'Tool sync complete', {
                        added: data?.added || 0,
                        updated: data?.updated || 0,
                        deprecated: data?.deprecated || 0,
                        total: data?.total || 0
            manager.addEventListener('toolsSynced', eventHandler);
            // Publish syncing progress
            this.publishProgress(pubSub, sessionId, ConnectionID, 'syncing', 'Synchronizing tools to database...');
            // Perform the sync
            const syncResult: MCPSyncToolsResult = await manager.syncTools(ConnectionID, { contextUser: user });
            // Remove event listener
            manager.removeEventListener('toolsSynced', eventHandler);
            if (!syncResult.success) {
                this.publishProgress(pubSub, sessionId, ConnectionID, 'error', `Sync failed: ${syncResult.error}`);
                return this.createErrorResult(syncResult.error || 'Tool sync failed');
            // Publish final completion
            this.publishProgress(pubSub, sessionId, ConnectionID, 'complete',
                `Sync complete: ${syncResult.added} added, ${syncResult.updated} updated, ${syncResult.deprecated} deprecated`,
                    added: syncResult.added,
                    updated: syncResult.updated,
                    deprecated: syncResult.deprecated,
                    total: syncResult.total
            LogStatus(`MCPResolver: Tool sync complete for ${ConnectionID} - Added: ${syncResult.added}, Updated: ${syncResult.updated}, Deprecated: ${syncResult.deprecated}, Total: ${syncResult.total}`);
                Added: syncResult.added,
                Updated: syncResult.updated,
                Deprecated: syncResult.deprecated,
                Total: syncResult.total,
                ServerName: serverName,
                ConnectionName: connectionName
            LogError(`MCPResolver: Error syncing tools for ${ConnectionID}: ${errorMsg}`);
            this.publishProgress(pubSub, sessionId, ConnectionID, 'error', `Error: ${errorMsg}`);
            return this.createErrorResult(errorMsg);
     * Executes an MCP tool and returns the result.
     * @param input The execution input parameters
     * @returns The execution result
    @Mutation(() => ExecuteMCPToolResult)
    async ExecuteMCPTool(
        @Arg('input') input: ExecuteMCPToolInput,
    ): Promise<ExecuteMCPToolResult> {
                ErrorMessage: 'User is not authenticated'
        const { ConnectionID, ToolID, ToolName, InputArgs } = input;
            LogStatus(`MCPResolver: [${ToolName}] Step 1 - Checking API key authorization...`);
            await this.CheckAPIKeyScopeAuthorization('mcp:execute', ConnectionID, ctx.userPayload);
            LogStatus(`MCPResolver: [${ToolName}] Step 1 complete - Authorization passed (${Date.now() - startTime}ms)`);
            LogStatus(`MCPResolver: [${ToolName}] Step 2 - Initializing MCP client manager...`);
            LogStatus(`MCPResolver: [${ToolName}] Step 2 complete - Manager initialized (${Date.now() - startTime}ms)`);
            LogStatus(`MCPResolver: [${ToolName}] Step 3 - Connection status: ${isConnected ? 'already connected' : 'needs connection'}`);
                LogStatus(`MCPResolver: [${ToolName}] Connecting to MCP server for connection ${ConnectionID}...`);
                    LogStatus(`MCPResolver: [${ToolName}] Step 3 complete - Connected (${Date.now() - startTime}ms)`);
                        LogError(`MCPResolver: [${ToolName}] OAuth authorization required`);
                            ErrorMessage: `OAuth authorization required. Please authorize at: ${authError.authorizationUrl}`,
                            Result: {
                                requiresOAuth: true,
                                authorizationUrl: authError.authorizationUrl,
                                stateParameter: authError.stateParameter
                        LogError(`MCPResolver: [${ToolName}] OAuth re-authorization required: ${reAuthError.reason}`);
                            ErrorMessage: `OAuth re-authorization required: ${reAuthError.reason}`,
                                reason: reAuthError.reason,
                                authorizationUrl: reAuthError.authorizationUrl,
                                stateParameter: reAuthError.stateParameter
                    LogError(`MCPResolver: [${ToolName}] Connection failed: ${connectErrorMsg}`);
                        ErrorMessage: `Failed to connect to MCP server: ${connectErrorMsg}`
            // Parse input arguments
            LogStatus(`MCPResolver: [${ToolName}] Step 4 - Parsing input arguments...`);
            let parsedArgs: Record<string, unknown> = {};
            if (InputArgs) {
                    parsedArgs = JSON.parse(InputArgs);
                    LogStatus(`MCPResolver: [${ToolName}] Parsed args: ${JSON.stringify(parsedArgs).substring(0, 200)}...`);
                    LogError(`MCPResolver: [${ToolName}] Failed to parse InputArgs: ${parseError}`);
                        ErrorMessage: 'Invalid JSON in InputArgs'
            LogStatus(`MCPResolver: [${ToolName}] Step 4 complete - Args parsed (${Date.now() - startTime}ms)`);
            LogStatus(`MCPResolver: [${ToolName}] Step 5 - Calling tool on connection ${ConnectionID}...`);
            LogStatus(`MCPResolver: [${ToolName}] Tool ID: ${ToolID}`);
            const result: MCPToolCallResult = await manager.callTool(
                ConnectionID,
                ToolName,
                { arguments: parsedArgs },
                { contextUser: user }
            LogStatus(`MCPResolver: [${ToolName}] Step 5 complete - Tool call returned (${Date.now() - startTime}ms)`);
            // Format the result for the response - wrap in object for GraphQLJSONObject
            let formattedResult: Record<string, unknown> | null = null;
                // If there's only one text content block, try to parse as JSON object
                if (result.content.length === 1 && result.content[0].type === 'text') {
                    const textContent = result.content[0].text;
                    // Try to parse as JSON object
                    if (textContent && (textContent.startsWith('{') || textContent.startsWith('['))) {
                            const parsed = JSON.parse(textContent);
                            // Wrap arrays in an object
                                formattedResult = { items: parsed };
                            } else if (typeof parsed === 'object' && parsed !== null) {
                                formattedResult = parsed as Record<string, unknown>;
                                formattedResult = { value: parsed };
                            // Keep as wrapped string if not valid JSON
                            formattedResult = { text: textContent };
                        // Wrap plain text in object
                    // Return all content blocks wrapped in object
                    formattedResult = { content: result.content };
            // Use structuredContent if available (already an object)
            if (result.structuredContent) {
                formattedResult = result.structuredContent as Record<string, unknown>;
            LogStatus(`MCPResolver: [${ToolName}] Step 6 complete - Result formatted (${Date.now() - startTime}ms)`);
            LogStatus(`MCPResolver: [${ToolName}] Tool execution complete - Success: ${result.success}, Duration: ${result.durationMs}ms, Total time: ${Date.now() - startTime}ms`);
                Success: result.success,
                ErrorMessage: result.error,
                Result: formattedResult,
                DurationMs: result.durationMs
            LogError(`MCPResolver: [${ToolName}] Error after ${Date.now() - startTime}ms: ${errorMsg}`);
            LogError(`MCPResolver: [${ToolName}] Stack: ${stack}`);
                ErrorMessage: errorMsg
     * Gets OAuth connection status for an MCP connection.
     * @param connectionId - The MCP Server Connection ID
     * @param ctx - GraphQL context
     * @returns OAuth connection status
    @Query(() => MCPOAuthConnectionStatus)
    async GetMCPOAuthConnectionStatus(
        @Arg('ConnectionID') connectionId: string,
    ): Promise<MCPOAuthConnectionStatus> {
                IsOAuthEnabled: false,
                HasValidTokens: false,
                RequiresReauthorization: false,
                ReauthorizationReason: 'User is not authenticated'
            const config = await this.loadConnectionOAuthConfig(connectionId, user);
                    ReauthorizationReason: 'Connection not found'
            if (!config.OAuthIssuerURL) {
                    RequiresReauthorization: false
            // Get status from OAuthManager
            const oauthManager = new OAuthManager();
            const status = await oauthManager.getConnectionStatus(connectionId, config, user);
                ConnectionID: status.connectionId,
                IsOAuthEnabled: status.isOAuthEnabled,
                HasValidTokens: status.hasValidTokens,
                IsAccessTokenExpired: status.isAccessTokenExpired,
                TokenExpiresAt: status.tokenExpiresAt,
                HasRefreshToken: status.hasRefreshToken,
                RequiresReauthorization: status.requiresReauthorization,
                ReauthorizationReason: status.reauthorizationReason,
                IssuerUrl: status.issuerUrl,
                GrantedScopes: status.grantedScopes
            LogError(`MCPResolver: GetMCPOAuthConnectionStatus failed: ${errorMsg}`);
                RequiresReauthorization: true,
                ReauthorizationReason: errorMsg
     * Gets OAuth authorization flow status by state parameter.
     * @param input - Input containing state parameter
     * @returns OAuth status result
    @Query(() => MCPOAuthStatusResult)
    async GetMCPOAuthStatus(
        @Arg('input') input: GetMCPOAuthStatusInput,
    ): Promise<MCPOAuthStatusResult> {
                ExtraFilter: `StateParameter='${input.StateParameter.replace(/'/g, "''")}'`,
            }, user);
                    ErrorMessage: 'Authorization state not found',
                    IsRetryable: true
            const state = result.Results[0];
            const statusMap: Record<string, MCPOAuthStatus> = {
                'Pending': MCPOAuthStatus.PENDING,
                'Completed': MCPOAuthStatus.COMPLETED,
                'Failed': MCPOAuthStatus.FAILED,
                'Expired': MCPOAuthStatus.EXPIRED
                Status: statusMap[state.Status] ?? MCPOAuthStatus.PENDING,
                ConnectionID: state.MCPServerConnectionID,
                InitiatedAt: new Date(state.InitiatedAt),
                CompletedAt: state.CompletedAt ? new Date(state.CompletedAt) : undefined,
                AuthErrorCode: state.ErrorCode ?? undefined,
                AuthErrorDescription: state.ErrorDescription ?? undefined,
                IsRetryable: state.Status === 'Failed' || state.Status === 'Expired'
            LogError(`MCPResolver: GetMCPOAuthStatus failed: ${errorMsg}`);
     * Initiates an OAuth authorization flow for an MCP connection.
     * @param input - Input containing connection ID and optional scopes
     * @returns Initiation result with authorization URL
    @Mutation(() => InitiateMCPOAuthResult)
    async InitiateMCPOAuth(
        @Arg('input') input: InitiateMCPOAuthInput,
    ): Promise<InitiateMCPOAuthResult> {
            await this.CheckAPIKeyScopeAuthorization('mcp:oauth', input.ConnectionID, ctx.userPayload);
            const config = await this.loadConnectionOAuthConfig(input.ConnectionID, user);
                    ErrorMessage: 'Connection not found'
                    ErrorMessage: 'OAuth is not configured for this connection'
            // Merge additional scopes if provided
            let scopes = config.OAuthScopes;
            if (input.AdditionalScopes) {
                scopes = scopes
                    ? `${scopes} ${input.AdditionalScopes}`
                    : input.AdditionalScopes;
            const oauthConfig = {
                OAuthScopes: scopes
            // Initiate the OAuth flow
            // Build options for the OAuth flow
            const oauthOptions: { frontendReturnUrl?: string; frontendCallbackUrl?: string } = {};
            if (input.FrontendCallbackUrl) {
                oauthOptions.frontendCallbackUrl = input.FrontendCallbackUrl;
            const result = await oauthManager.initiateAuthorizationFlow(
                input.ConnectionID,
                config.MCPServerID,
                Object.keys(oauthOptions).length > 0 ? oauthOptions : undefined
            LogStatus(`MCPResolver: Initiated OAuth flow for connection ${input.ConnectionID}`);
                ErrorMessage: result.errorMessage,
                AuthorizationUrl: result.authorizationUrl,
                StateParameter: result.stateParameter,
                ExpiresAt: result.expiresAt,
                UsedDynamicRegistration: result.usedDynamicRegistration
            LogError(`MCPResolver: InitiateMCPOAuth failed: ${errorMsg}`);
     * Revokes OAuth credentials for an MCP connection.
     * @param input - Input containing connection ID and optional reason
     * @returns Revocation result
    @Mutation(() => RevokeMCPOAuthResult)
    async RevokeMCPOAuth(
        @Arg('input') input: RevokeMCPOAuthInput,
    ): Promise<RevokeMCPOAuthResult> {
            // Revoke the credentials
            const tokenManager = new TokenManager();
            await tokenManager.revokeCredentials(input.ConnectionID, user);
            LogStatus(`MCPResolver: Revoked OAuth credentials for connection ${input.ConnectionID}${input.Reason ? ` (reason: ${input.Reason})` : ''}`);
                ConnectionID: input.ConnectionID
            LogError(`MCPResolver: RevokeMCPOAuth failed: ${errorMsg}`);
     * Manually refreshes OAuth tokens for an MCP connection.
     * @param input - Input containing connection ID
    @Mutation(() => RefreshMCPOAuthTokenResult)
    async RefreshMCPOAuthToken(
        @Arg('input') input: RefreshMCPOAuthTokenInput,
    ): Promise<RefreshMCPOAuthTokenResult> {
            // Load connection config
            // Get the MCP client manager instance
            // Try to get access token (will refresh if needed)
                await oauthManager.getAccessToken(
                // Get updated status
                const status = await oauthManager.getConnectionStatus(input.ConnectionID, config, user);
                LogStatus(`MCPResolver: Refreshed OAuth tokens for connection ${input.ConnectionID}`);
                    ExpiresAt: status.tokenExpiresAt,
                if (error instanceof OAuthAuthorizationRequiredError ||
                    error instanceof OAuthReauthorizationRequiredError) {
                        ErrorMessage: error.message,
                        RequiresReauthorization: true
            LogError(`MCPResolver: RefreshMCPOAuthToken failed: ${errorMsg}`);
    // Subscriptions
     * Subscribes to OAuth events for MCP connections.
     * Clients can subscribe to receive real-time notifications when:
     * - Authorization is required for a connection
     * - Authorization completes successfully
     * - Token is refreshed
     * - Token refresh fails
     * @param payload - The OAuth event payload
     * @returns OAuth event notification
    @Subscription(() => MCPOAuthEventNotification, { topics: MCP_OAUTH_EVENTS_TOPIC })
    onMCPOAuthEvent(
        @Root() payload: MCPOAuthEventPayload
    ): MCPOAuthEventNotification {
            EventType: payload.eventType,
            ConnectionID: payload.connectionId,
            Timestamp: new Date(payload.timestamp),
            AuthorizationUrl: payload.authorizationUrl,
            StateParameter: payload.stateParameter,
            ErrorMessage: payload.errorMessage,
            RequiresReauthorization: payload.requiresReauthorization
     * Loads OAuth configuration for a connection
    private async loadConnectionOAuthConfig(
                OAuthMetadataCacheTTLMinutes: number | null;
                OAuthRequirePKCE: boolean | null;
                    'OAuthIssuerURL',
                    'OAuthScopes',
                    'OAuthMetadataCacheTTLMinutes',
                    'OAuthClientID',
                    'OAuthClientSecretEncrypted',
                    'OAuthRequirePKCE'
                OAuthMetadataCacheTTLMinutes: server.OAuthMetadataCacheTTLMinutes ?? undefined,
                OAuthClientSecretEncrypted: server.OAuthClientSecretEncrypted ?? undefined,
                OAuthRequirePKCE: server.OAuthRequirePKCE ?? undefined
            LogError(`MCPResolver: Failed to load connection OAuth config: ${error}`);
     * Publishes a progress update to the statusUpdates subscription
    private publishProgress(
        pubSub: PubSubEngine,
        sessionId: string,
        phase: SyncProgressMessage['phase'],
        result?: { added: number; updated: number; deprecated: number; total: number }
        const progressMessage: SyncProgressMessage = {
            status: phase === 'error' ? 'error' : 'ok',
            message: JSON.stringify(progressMessage),
     * Creates an error result with default values
    private createErrorResult(errorMessage: string): SyncMCPToolsResult {
     * Creates a result indicating OAuth authorization is required
    private createOAuthRequiredResult(authorizationUrl: string, stateParameter: string): SyncMCPToolsResult {
            ErrorMessage: 'OAuth authorization required',
            Total: 0,
            RequiresOAuth: true,
            AuthorizationUrl: authorizationUrl,
            StateParameter: stateParameter
     * Creates a result indicating OAuth re-authorization is required
    private createOAuthReauthorizationResult(
    ): SyncMCPToolsResult {
            ErrorMessage: `OAuth re-authorization required: ${reason}`,
            ReauthorizationReason: reason,
     * Gets the public URL for OAuth callbacks
    private getPublicUrl(): string {
        // Use publicUrl from config, falling back to constructed URL
        if (configInfo.publicUrl) {
            return configInfo.publicUrl;
        // Construct from baseUrl and graphqlPort
        const baseUrl = configInfo.baseUrl || 'http://localhost';
        const port = configInfo.graphqlPort || 4000;
        const rootPath = configInfo.graphqlRootPath || '/';
        // Construct full URL
        let url = `${baseUrl}:${port}`;
        if (rootPath && rootPath !== '/') {
            url += rootPath;
 * Publishes an OAuth event to the subscription topic.
 * Can be called from other resolvers or handlers to notify clients of OAuth events.
 * @param pubSub - PubSub engine
 * @param event - OAuth event details
export async function publishMCPOAuthEvent(
    event: {
    const payload: MCPOAuthEventPayload = {
        connectionId: event.connectionId,
        authorizationUrl: event.authorizationUrl,
        stateParameter: event.stateParameter,
        errorMessage: event.errorMessage,
        requiresReauthorization: event.requiresReauthorization
    await pubSub.publish(MCP_OAUTH_EVENTS_TOPIC, payload);
