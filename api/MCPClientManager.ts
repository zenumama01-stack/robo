 * @fileoverview MCP Client Manager for MemberJunction
 * Provides a singleton manager for connecting to and interacting with
 * external MCP servers. Supports multiple transport types, authentication
 * methods, rate limiting, and execution logging.
 * @module @memberjunction/ai-mcp-client/MCPClientManager
import { Client } from '@modelcontextprotocol/sdk/client/index.js';
import { StreamableHTTPClientTransport } from '@modelcontextprotocol/sdk/client/streamableHttp.js';
import { SSEClientTransport } from '@modelcontextprotocol/sdk/client/sse.js';
import { StdioClientTransport } from '@modelcontextprotocol/sdk/client/stdio.js';
import { WebSocketClientTransport } from '@modelcontextprotocol/sdk/client/websocket.js';
import type { Transport } from '@modelcontextprotocol/sdk/shared/transport.js';
import type { CallToolResult, Tool } from '@modelcontextprotocol/sdk/types.js';
import { Metadata, RunView, UserInfo, LogError, LogStatus } from '@memberjunction/core';
import { CredentialEngine } from '@memberjunction/credentials';
    MJMCPServerEntity,
    MJMCPServerConnectionEntity,
    MJMCPServerToolEntity,
    MJActionEntity,
    MJActionCategoryEntity,
    MJActionParamEntity,
    MJActionResultCodeEntity
import { RateLimiterRegistry, RateLimiter } from './RateLimiter.js';
import { ExecutionLogger } from './ExecutionLogger.js';
import { OAuthManager } from './oauth/OAuthManager.js';
import { OAuthAuthorizationRequiredError, OAuthReauthorizationRequiredError } from './oauth/types.js';
import type { MCPServerOAuthConfig } from './oauth/types.js';
    MCPServerConfig,
    MCPConnectionConfig,
    MCPToolDefinition,
    MCPConnectionToolConfig,
    MCPConnectionPermission,
    MCPCredentialData,
    MCPCallToolOptions,
    MCPToolCallResult,
    MCPListToolsResult,
    MCPToolInfo,
    MCPSyncToolsResult,
    MCPSyncActionsResult,
    MCPTestConnectionResult,
    MCPServerCapabilities,
    MCPActiveConnection,
    MCPTransportType,
    MCPAuthType,
    MCPLoggingConfig,
    MCPClientOptions,
    MCPConnectOptions,
    MCPDisconnectOptions,
    MCPClientEvent,
    MCPClientEventListener,
    MCPClientEventType,
    MCPContentBlock,
    JSONSchemaProperties,
    JSONSchemaProperty
} from './types.js';
 * MCPClientManager is a singleton that manages connections to external MCP servers.
 * - Support for all MCP transport types (StreamableHTTP, SSE, Stdio, WebSocket)
 * - Multiple authentication methods (None, Bearer, APIKey, OAuth2, Basic, Custom)
 * - Integration with MJ CredentialEngine for secure credential storage
 * - Rate limiting with request queuing
 * - Comprehensive execution logging
 * - Permission-based access control
 * const manager = MCPClientManager.Instance;
 * // Connect to an MCP server
 * await manager.connect('connection-id', { contextUser });
 * // Call a tool
 * const result = await manager.callTool('connection-id', 'tool-name', {
 *     arguments: { param1: 'value1' }
 * }, { contextUser });
 * // Disconnect
 * await manager.disconnect('connection-id', { contextUser });
export class MCPClientManager {
    /** Singleton instance */
    private static _instance: MCPClientManager | null = null;
    /** Active connections */
    private readonly connections: Map<string, MCPActiveConnection> = new Map();
    /** Rate limiters per connection */
    private readonly rateLimiters: RateLimiterRegistry = new RateLimiterRegistry();
    /** Execution logger */
    private readonly logger: ExecutionLogger = new ExecutionLogger();
    /** Event listeners */
    private readonly eventListeners: Map<MCPClientEventType, Set<MCPClientEventListener>> = new Map();
    /** OAuth manager for OAuth2 authentication */
    private readonly oauthManager: OAuthManager = new OAuthManager();
    /** Whether the manager has been initialized */
    /** Public URL for OAuth callbacks (set during initialization) */
    private publicUrl: string = '';
    /** Client name for MCP handshake */
    private static readonly CLIENT_NAME = 'MemberJunction';
    /** Client version for MCP handshake */
    private static readonly CLIENT_VERSION = '3.3.0';
    /** Default connection timeout */
    private static readonly DEFAULT_CONNECTION_TIMEOUT = 30000;
    /** Default request timeout */
    private static readonly DEFAULT_REQUEST_TIMEOUT = 60000;
    /** Entity names */
    private static readonly ENTITY_MCP_SERVERS = 'MJ: MCP Servers';
    private static readonly ENTITY_MCP_CONNECTIONS = 'MJ: MCP Server Connections';
    private static readonly ENTITY_MCP_TOOLS = 'MJ: MCP Server Tools';
    private static readonly ENTITY_MCP_CONNECTION_TOOLS = 'MJ: MCP Server Connection Tools';
    private static readonly ENTITY_MCP_PERMISSIONS = 'MJ: MCP Server Connection Permissions';
        // Initialize event listener maps
        const eventTypes: MCPClientEventType[] = [
            'connected', 'disconnected', 'toolCalled', 'toolCallCompleted',
            'toolsSynced', 'connectionError', 'rateLimitExceeded'
        for (const eventType of eventTypes) {
            this.eventListeners.set(eventType, new Set());
     * Gets the singleton instance of MCPClientManager
    public static get Instance(): MCPClientManager {
        if (!MCPClientManager._instance) {
            MCPClientManager._instance = new MCPClientManager();
        return MCPClientManager._instance;
     * Initializes the manager. Should be called once at application startup.
     * @param contextUser - User context for initialization
     * @param options - Optional initialization options
    public async initialize(
        options?: { publicUrl?: string }
        if (this.initialized) {
            // Ensure CredentialEngine is configured
            await CredentialEngine.Instance.Config(false, contextUser);
            // Store public URL for OAuth callbacks
            if (options?.publicUrl) {
                this.publicUrl = options.publicUrl;
            LogStatus('[MCPClient] Manager initialized');
            LogError(`[MCPClient] Failed to initialize: ${error}`);
     * Sets the public URL for OAuth callbacks.
     * @param publicUrl - The public URL (e.g., https://api.example.com)
    public setPublicUrl(publicUrl: string): void {
        this.publicUrl = publicUrl;
     * Checks if a connection is currently active
     * @param connectionId - Connection ID to check
     * @returns true if connected
    public isConnected(connectionId: string): boolean {
        return this.connections.has(connectionId);
     * Gets the list of active connection IDs
     * @returns Array of active connection IDs
    public getActiveConnections(): string[] {
        return Array.from(this.connections.keys());
    // Connection Management
     * Connects to an MCP server using a configured connection
     * @param connectionId - The connection ID to use
     * @param options - Connection options
     * @returns Connection info on success
    public async connect(
        options: MCPConnectOptions
    ): Promise<MCPActiveConnection> {
        const { contextUser, forceReconnect = false, skipAutoSync = false } = options;
        // Check for existing connection
        if (this.connections.has(connectionId) && !forceReconnect) {
            return this.connections.get(connectionId)!;
        // Disconnect existing if forcing reconnect
        if (this.connections.has(connectionId) && forceReconnect) {
            await this.disconnect(connectionId, { contextUser, force: true });
            // Load connection configuration
            const connectionConfig = await this.loadConnectionConfig(connectionId, contextUser);
            if (!connectionConfig) {
                throw new Error(`Connection not found: ${connectionId}`);
            // Check permissions
            if (!options.skipPermissionCheck) {
                const hasPermission = await this.checkPermission(connectionId, contextUser, 'execute');
                if (!hasPermission) {
                    throw new Error(`Permission denied for connection: ${connectionId}`);
            // Load server configuration
            const serverConfig = await this.loadServerConfig(connectionConfig.MCPServerID, contextUser);
            if (!serverConfig) {
                throw new Error(`Server not found for connection: ${connectionId}`);
            // Get credentials if needed
            let credentials: MCPCredentialData | undefined;
            const authType = serverConfig.DefaultAuthType as MCPAuthType;
            if (authType === 'OAuth2') {
                // Handle OAuth2 authentication
                credentials = await this.getOAuth2Credentials(connectionId, serverConfig, contextUser);
            } else if (connectionConfig.CredentialID && authType !== 'None') {
                    credentials = await this.getCredentials(connectionConfig.CredentialID, contextUser);
                    // Log warning but proceed without credentials if auth isn't strictly required
                    LogStatus(`[MCPClient] Warning: Could not load credentials for connection ${connectionId}. Proceeding without authentication.`);
            // Create transport
            const transport = await this.createTransport(
                serverConfig,
                connectionConfig,
                credentials
            // Create MCP client
            const client = new Client({
                name: MCPClientManager.CLIENT_NAME,
                version: MCPClientManager.CLIENT_VERSION
            // Connect
            await client.connect(transport);
            // Get server capabilities
            const capabilities = this.mapServerCapabilities(client.getServerCapabilities());
            // Create active connection record
            const activeConnection: MCPActiveConnection = {
                client,
                transport,
                connectedAt: new Date(),
                lastActivityAt: new Date(),
                capabilities
            // Store connection
            this.connections.set(connectionId, activeConnection);
            // Set up rate limiter
            this.rateLimiters.getOrCreate(connectionId, {
                perMinute: serverConfig.RateLimitPerMinute,
                perHour: serverConfig.RateLimitPerHour
            // Update connection status in database
            await this.updateConnectionStatus(connectionId, 'Active', contextUser);
            // Emit connected event
            this.emitEvent({
                type: 'connected',
                data: { serverName: serverConfig.Name }
            // Auto-sync tools if enabled
            if (connectionConfig.AutoSyncTools && !skipAutoSync) {
                    await this.syncTools(connectionId, { contextUser });
                } catch (syncError) {
                    // Don't fail connection for sync errors
                    LogError(`[MCPClient] Auto-sync failed for ${connectionId}: ${syncError}`);
            LogStatus(`[MCPClient] Connected to ${serverConfig.Name} via ${serverConfig.TransportType}`);
            return activeConnection;
            // Check for OAuth-specific errors and emit appropriate events
            if (error instanceof OAuthAuthorizationRequiredError) {
                // Emit authorizationRequired event before re-throwing
                    type: 'authorizationRequired',
                        authorizationUrl: error.authorizationUrl,
                        stateParameter: error.stateParameter,
                        expiresAt: error.expiresAt.toISOString()
                // Don't update connection status to Error for auth required - it's expected flow
            if (error instanceof OAuthReauthorizationRequiredError) {
                // Emit tokenRefreshFailed event when re-authorization is needed
                    type: 'tokenRefreshFailed',
                        reason: error.reason,
                        requiresReauthorization: true,
                        stateParameter: error.stateParameter
                // Don't update connection status to Error for reauth required - it's expected flow
            // Update connection status to Error for other errors
            await this.updateConnectionStatus(connectionId, 'Error', contextUser, error);
            // Emit error event
                type: 'connectionError',
                data: { error: error instanceof Error ? error.message : String(error) }
     * Disconnects from an MCP server
     * @param connectionId - The connection ID to disconnect
     * @param options - Disconnect options
    public async disconnect(
        options: MCPDisconnectOptions
        const connection = this.connections.get(connectionId);
        if (!connection) {
            return; // Already disconnected
            // Close the transport
            const transport = connection.transport as Transport;
            await transport.close();
            // Remove from active connections
            this.connections.delete(connectionId);
            // Clean up rate limiter
            this.rateLimiters.remove(connectionId);
            // Update connection status
            await this.updateConnectionStatus(connectionId, 'Inactive', options.contextUser);
            // Emit disconnected event
                type: 'disconnected',
                timestamp: new Date()
            LogStatus(`[MCPClient] Disconnected from ${connection.serverConfig.Name}`);
            if (options.force) {
                // Force remove even on error
    // Tool Operations
     * Calls a tool on an MCP server
     * @param toolName - Name of the tool to call
     * @param toolOptions - Tool call options including arguments
     * @param options - Client options
     * @returns Tool call result
    public async callTool(
        toolOptions: MCPCallToolOptions,
        options: MCPClientOptions
    ): Promise<MCPToolCallResult> {
        const { contextUser } = options;
        LogStatus(`[MCPClient] callTool started for ${toolName} on connection ${connectionId}`);
        // Get active connection
            LogError(`[MCPClient] Not connected: ${connectionId}`);
            throw new Error(`Not connected: ${connectionId}`);
        LogStatus(`[MCPClient] [${toolName}] Got connection (${Date.now() - startTime}ms)`);
            LogStatus(`[MCPClient] [${toolName}] Checking permissions...`);
                LogError(`[MCPClient] [${toolName}] Permission denied`);
            LogStatus(`[MCPClient] [${toolName}] Permission check passed (${Date.now() - startTime}ms)`);
        // Get logging config
        const loggingConfig = this.getLoggingConfig(connection.connectionConfig);
        LogStatus(`[MCPClient] [${toolName}] Got logging config (${Date.now() - startTime}ms)`);
        // Get tool ID for logging
        LogStatus(`[MCPClient] [${toolName}] Getting tool ID...`);
        const toolId = await this.getToolId(connection.serverConfig.ID, toolName, contextUser);
        LogStatus(`[MCPClient] [${toolName}] Got tool ID: ${toolId} (${Date.now() - startTime}ms)`);
        // Start logging
        LogStatus(`[MCPClient] [${toolName}] Starting execution log...`);
        const logId = await this.logger.startLog(
            toolOptions.arguments,
            loggingConfig,
        LogStatus(`[MCPClient] [${toolName}] Execution log started: ${logId} (${Date.now() - startTime}ms)`);
        // Emit tool called event
            type: 'toolCalled',
            data: { toolName }
            // Acquire rate limit slot
            const rateLimiter = this.rateLimiters.get(connectionId);
            if (rateLimiter) {
                LogStatus(`[MCPClient] [${toolName}] Acquiring rate limit slot...`);
                    await rateLimiter.acquire();
                    LogStatus(`[MCPClient] [${toolName}] Rate limit slot acquired (${Date.now() - startTime}ms)`);
                } catch (rateLimitError) {
                    LogError(`[MCPClient] [${toolName}] Rate limit exceeded: ${rateLimitError}`);
                        type: 'rateLimitExceeded',
                        data: { toolName, error: rateLimitError instanceof Error ? rateLimitError.message : String(rateLimitError) }
                    throw rateLimitError;
            // Update last activity time
            connection.lastActivityAt = new Date();
            // Call the tool
            const client = connection.client as Client;
            const timeout = toolOptions.timeout ?? connection.serverConfig.RequestTimeoutMs ?? MCPClientManager.DEFAULT_REQUEST_TIMEOUT;
            LogStatus(`[MCPClient] [${toolName}] Calling MCP tool with timeout ${timeout}ms...`);
            LogStatus(`[MCPClient] [${toolName}] Arguments: ${JSON.stringify(toolOptions.arguments).substring(0, 200)}`);
            const mcpResult = await client.callTool({
                name: toolName,
                arguments: toolOptions.arguments
            }, undefined, {
                timeout,
                signal: toolOptions.signal,
                onprogress: toolOptions.onProgress ? (progress) => {
                    LogStatus(`[MCPClient] [${toolName}] Progress: ${progress.progress}/${progress.total}`);
                    toolOptions.onProgress?.({
                        progress: progress.progress,
                        total: progress.total
            }) as CallToolResult;
            LogStatus(`[MCPClient] [${toolName}] MCP tool call returned (${Date.now() - startTime}ms)`);
            LogStatus(`[MCPClient] [${toolName}] Result isError: ${mcpResult.isError}, content length: ${mcpResult.content?.length || 0}`);
            // Map result
            const result = this.mapToolCallResult(mcpResult, Date.now() - startTime);
            LogStatus(`[MCPClient] [${toolName}] Result mapped (${Date.now() - startTime}ms)`);
            // Complete logging
            LogStatus(`[MCPClient] [${toolName}] Completing execution log...`);
            await this.logger.completeLog(logId, result, loggingConfig, contextUser);
            LogStatus(`[MCPClient] [${toolName}] Execution log completed (${Date.now() - startTime}ms)`);
            // Emit completion event
                type: 'toolCallCompleted',
                data: { toolName, success: result.success, durationMs: result.durationMs }
            LogStatus(`[MCPClient] [${toolName}] callTool completed successfully (${Date.now() - startTime}ms)`);
            const durationMs = Date.now() - startTime;
            const errorMsg = error instanceof Error ? error.message : String(error);
            const stack = error instanceof Error ? error.stack : '';
            LogError(`[MCPClient] [${toolName}] Error after ${durationMs}ms: ${errorMsg}`);
            LogError(`[MCPClient] [${toolName}] Stack: ${stack}`);
            // Fail logging
            await this.logger.failLog(logId, error instanceof Error ? error : String(error), durationMs, contextUser);
            // Emit completion event with error
                data: { toolName, success: false, durationMs, error: errorMsg }
                durationMs,
                isToolError: false
     * Lists available tools from an MCP server
     * @returns List of available tools
    public async listTools(
    ): Promise<MCPListToolsResult> {
            const result = await client.listTools();
            const tools: MCPToolInfo[] = (result.tools || []).map((tool: Tool) => ({
                description: tool.description,
                inputSchema: tool.inputSchema as Record<string, unknown>,
                outputSchema: tool.outputSchema as Record<string, unknown> | undefined,
                    title: tool.annotations.title,
                    readOnlyHint: tool.annotations.readOnlyHint,
                    destructiveHint: tool.annotations.destructiveHint,
                    idempotentHint: tool.annotations.idempotentHint,
                    openWorldHint: tool.annotations.openWorldHint
                tools
                tools: [],
     * Syncs tool definitions from the MCP server to the database
     * @returns Sync result
    public async syncTools(
    ): Promise<MCPSyncToolsResult> {
            // List tools from server
            const listResult = await this.listTools(connectionId, options);
            if (!listResult.success) {
                    added: 0,
                    updated: 0,
                    deprecated: 0,
                    total: 0,
                    error: listResult.error
            // Load existing tools from database
            const existingTools = await this.loadServerTools(connection.serverConfig.ID, contextUser);
            const existingToolMap = new Map(existingTools.map(t => [t.ToolName, t]));
            let added = 0;
            let updated = 0;
            const seenToolNames = new Set<string>();
            // Process each tool from server
            for (const tool of listResult.tools) {
                seenToolNames.add(tool.name);
                const existing = existingToolMap.get(tool.name);
                    // Update existing tool
                    const toolEntity = await md.GetEntityObject<MJMCPServerToolEntity>(
                        MCPClientManager.ENTITY_MCP_TOOLS,
                    const loaded = await toolEntity.Load(existing.ID);
                        toolEntity.ToolTitle = tool.annotations?.title ?? tool.name;
                        toolEntity.ToolDescription = tool.description ?? null;
                        toolEntity.InputSchema = JSON.stringify(tool.inputSchema);
                        toolEntity.OutputSchema = tool.outputSchema ? JSON.stringify(tool.outputSchema) : null;
                        toolEntity.Annotations = tool.annotations ? JSON.stringify(tool.annotations) : null;
                        toolEntity.Status = 'Active';
                        toolEntity.LastSeenAt = new Date();
                        await toolEntity.Save();
                        updated++;
                    // Add new tool
                    toolEntity.NewRecord();
                    toolEntity.MCPServerID = connection.serverConfig.ID;
                    toolEntity.ToolName = tool.name;
                    toolEntity.DiscoveredAt = new Date();
                    added++;
            // Mark tools not seen as deprecated
            let deprecated = 0;
            for (const existing of existingTools) {
                if (!seenToolNames.has(existing.ToolName) && existing.Status === 'Active') {
                        toolEntity.Status = 'Deprecated';
                        deprecated++;
            // Update server LastSyncAt
            await this.updateServerLastSync(connection.serverConfig.ID, contextUser);
            // Sync Actions for the tools (creates Actions in System/MCP/{ServerName})
            const actionsResult = await this.syncActionsForServer(connection.serverConfig.ID, contextUser);
            if (!actionsResult.success) {
                LogError(`Warning: Tool sync succeeded but Actions sync failed: ${actionsResult.error}`);
            // Emit sync event
                type: 'toolsSynced',
                data: { added, updated, deprecated, total: listResult.tools.length }
                added,
                updated,
                deprecated,
                total: listResult.tools.length
     * Syncs MCP Server Tools to MJ Actions.
     * Creates the category hierarchy System/MCP/{ServerName} and an Action for each tool.
     * @param serverId - The MCP Server ID to sync actions for
     * @param contextUser - The user context for database operations
     * @returns Sync result with counts of created/updated actions and params
    public async syncActionsForServer(
        serverId: string,
    ): Promise<MCPSyncActionsResult> {
            // Load the server
            const serverResult = await rv.RunView<MJMCPServerEntity>({
                EntityName: MCPClientManager.ENTITY_MCP_SERVERS,
                ExtraFilter: `ID='${serverId}'`,
            if (!serverResult.Success || serverResult.Results.length === 0) {
                    actionsCreated: 0,
                    actionsUpdated: 0,
                    paramsCreated: 0,
                    paramsUpdated: 0,
                    paramsDeleted: 0,
                    error: `Server not found: ${serverId}`
            const server = serverResult.Results[0];
            // Get or create the server's category under System/MCP/{ServerName}
            const serverCategoryId = await this.getOrCreateServerCategory(server.Name, contextUser);
            if (!serverCategoryId) {
                    error: 'Failed to create server category'
            // Load all tools for this server
            const toolsResult = await rv.RunView<MJMCPServerToolEntity>({
                EntityName: MCPClientManager.ENTITY_MCP_TOOLS,
                ExtraFilter: `MCPServerID='${serverId}' AND Status='Active'`,
            if (!toolsResult.Success) {
                    error: `Failed to load tools: ${toolsResult.ErrorMessage}`
            let actionsCreated = 0;
            let actionsUpdated = 0;
            let paramsCreated = 0;
            let paramsUpdated = 0;
            let paramsDeleted = 0;
            // Process each tool
            for (const tool of toolsResult.Results) {
                const result = await this.syncActionForTool(tool, serverCategoryId, contextUser);
                if (result.created) {
                    actionsCreated++;
                    actionsUpdated++;
                paramsCreated += result.paramsCreated;
                paramsUpdated += result.paramsUpdated;
                paramsDeleted += result.paramsDeleted;
            LogStatus(`Synced actions for MCP Server '${server.Name}': ${actionsCreated} created, ${actionsUpdated} updated`);
                actionsCreated,
                actionsUpdated,
                paramsCreated,
                paramsUpdated,
                paramsDeleted,
                serverCategoryId
            LogError(`Error syncing actions for server ${serverId}: ${error}`);
     * Gets or creates the category hierarchy: System/MCP/{ServerName}
     * @param serverName - The MCP Server name to create category for
     * @param contextUser - The user context
     * @returns The server category ID or null if failed
    private async getOrCreateServerCategory(
        serverName: string,
            // Step 1: Find or create "System" category (root)
            const systemCategoryId = await this.findOrCreateCategory(
                'System',
                'Core system actions and utilities',
            if (!systemCategoryId) {
                LogError('Failed to find or create System category');
            // Step 2: Find or create "MCP" category under System
            const mcpCategoryId = await this.findOrCreateCategory(
                'MCP',
                systemCategoryId,
                'Model Context Protocol (MCP) server tools exposed as Actions',
            if (!mcpCategoryId) {
                LogError('Failed to find or create MCP category');
            // Step 3: Find or create server-specific category under MCP
            const serverCategoryId = await this.findOrCreateCategory(
                serverName,
                mcpCategoryId,
                `Tools from MCP Server: ${serverName}`,
                LogError(`Failed to find or create category for server: ${serverName}`);
            return serverCategoryId;
            LogError(`Error creating category hierarchy for server ${serverName}: ${error}`);
     * Finds or creates an ActionCategory with the given name and parent.
     * @param name - Category name
     * @param parentId - Parent category ID (null for root)
     * @param description - Category description
     * @returns The category ID or null if failed
    private async findOrCreateCategory(
        parentId: string | null,
        description: string,
        // Build filter based on parent
        const parentFilter = parentId
            ? `ParentID='${parentId}'`
            : 'ParentID IS NULL';
        // Try to find existing category
        const existingResult = await rv.RunView<MJActionCategoryEntity>({
            EntityName: 'MJ: Action Categories',
            ExtraFilter: `Name='${name}' AND ${parentFilter}`,
        if (existingResult.Success && existingResult.Results.length > 0) {
            return existingResult.Results[0].ID;
        // Create new category
        const category = await md.GetEntityObject<MJActionCategoryEntity>('MJ: Action Categories', contextUser);
        category.NewRecord();
        category.Name = name;
        category.Description = description;
        category.ParentID = parentId;
        category.Status = 'Active';
        const saved = await category.Save();
            LogError(`Failed to create category '${name}': ${category.LatestResult?.Message}`);
        LogStatus(`Created Action Category: ${name}`);
        return category.ID;
     * Syncs a single MCP Server Tool to an Action.
     * @param tool - The MCPServerTool entity
     * @param categoryId - The server category ID
     * @returns Result with created flag and param counts
    private async syncActionForTool(
        tool: MJMCPServerToolEntity,
        categoryId: string,
    ): Promise<{ created: boolean; paramsCreated: number; paramsUpdated: number; paramsDeleted: number }> {
        let action: MJActionEntity;
        let created = false;
        // Check if tool already has a linked action
        if (tool.GeneratedActionID) {
            const actionEntity = await md.GetEntityObject<MJActionEntity>('MJ: Actions', contextUser);
            const loaded = await actionEntity.Load(tool.GeneratedActionID);
                action = actionEntity;
                // Action was deleted, create new one
                action = await this.createActionForTool(tool, categoryId, contextUser);
                created = true;
            // Check if an action with this name already exists in the category
            const existingResult = await rv.RunView<MJActionEntity>({
                ExtraFilter: `Name='${tool.ToolName.replace(/'/g, "''")}' AND CategoryID='${categoryId}'`,
                action = existingResult.Results[0];
        // Update action properties
        action.Description = tool.ToolDescription || `MCP Tool: ${tool.ToolName}`;
        action.CategoryID = categoryId;
        // Save action if dirty
        if (action.Dirty) {
            await action.Save();
        // Update the tool's GeneratedActionID and GeneratedActionCategoryID if needed
        if (tool.GeneratedActionID !== action.ID || tool.GeneratedActionCategoryID !== categoryId) {
            await toolEntity.Load(tool.ID);
            toolEntity.GeneratedActionID = action.ID;
            toolEntity.GeneratedActionCategoryID = categoryId;
        // Sync action params from tool's InputSchema (input params)
        const paramResult = await this.syncActionParamsFromSchema(action.ID, tool.InputSchema, contextUser);
        // Sync standard output params for MCP tools
        const outputParamResult = await this.syncMCPOutputParams(action.ID, contextUser);
        // Sync standard result codes for MCP tools
        await this.syncMCPResultCodes(action.ID, contextUser);
            paramsCreated: paramResult.created + outputParamResult.created,
            paramsUpdated: paramResult.updated + outputParamResult.updated,
            paramsDeleted: paramResult.deleted
     * Creates a new Action for an MCP Server Tool.
     * @param categoryId - The category ID
     * @returns The created Action entity
    private async createActionForTool(
    ): Promise<MJActionEntity> {
        const action = await md.GetEntityObject<MJActionEntity>('MJ: Actions', contextUser);
        action.NewRecord();
        action.Name = tool.ToolName;
        action.Type = 'Custom';
        action.DriverClass = 'MCPToolAction';  // Special driver class for MCP tools
        action.Status = 'Active';
        action.CodeApprovalStatus = 'Approved';  // MCP tools are pre-approved
        action.CodeLocked = true;  // Prevent code generation
        LogStatus(`Created Action for MCP Tool: ${tool.ToolName}`);
     * Syncs ActionParams from a tool's InputSchema JSON Schema.
     * @param actionId - The Action ID
     * @param inputSchemaJson - The JSON Schema string for input parameters
     * @returns Counts of created, updated, and deleted params
    private async syncActionParamsFromSchema(
        inputSchemaJson: string,
    ): Promise<{ created: number; updated: number; deleted: number }> {
        // Parse the input schema
        let schema: JSONSchemaProperties;
            schema = JSON.parse(inputSchemaJson);
            LogError(`Failed to parse InputSchema for action ${actionId}`);
            return { created, updated, deleted };
        // Get existing params for this action
        const existingResult = await rv.RunView<MJActionParamEntity>({
            ExtraFilter: `ActionID='${actionId}'`,
        const existingParams = existingResult.Success ? existingResult.Results : [];
        const existingParamMap = new Map(existingParams.map(p => [p.Name, p]));
        const seenParamNames = new Set<string>();
        // Process schema properties
        const properties = schema.properties || {};
        const required = new Set(schema.required || []);
        for (const [paramName, paramDef] of Object.entries(properties)) {
            seenParamNames.add(paramName);
            const existing = existingParamMap.get(paramName);
            const paramDefinition = paramDef as JSONSchemaProperty;
                // Update existing param
                existing.Description = paramDefinition.description || null;
                existing.IsRequired = required.has(paramName);
                existing.ValueType = this.mapJsonSchemaTypeToValueType(paramDefinition.type);
                existing.IsArray = paramDefinition.type === 'array';
                existing.DefaultValue = paramDefinition.default !== undefined
                    ? JSON.stringify(paramDefinition.default)
                if (existing.Dirty) {
                    await existing.Save();
                // Create new param
                const param = await md.GetEntityObject<MJActionParamEntity>('MJ: Action Params', contextUser);
                param.NewRecord();
                param.ActionID = actionId;
                param.Name = paramName;
                param.Description = paramDefinition.description || null;
                param.Type = 'Input';
                param.IsRequired = required.has(paramName);
                param.ValueType = this.mapJsonSchemaTypeToValueType(paramDefinition.type);
                param.IsArray = paramDefinition.type === 'array';
                param.DefaultValue = paramDefinition.default !== undefined
                await param.Save();
        // Delete params that no longer exist in schema
        for (const existing of existingParams) {
            if (!seenParamNames.has(existing.Name)) {
                await existing.Delete();
     * Maps JSON Schema type to ActionParam ValueType.
     * @param jsonType - The JSON Schema type
     * @returns The corresponding ActionParam ValueType
    private mapJsonSchemaTypeToValueType(
        jsonType: string | string[] | undefined
    ): 'Scalar' | 'Simple Object' | 'Other' {
        const type = Array.isArray(jsonType) ? jsonType[0] : jsonType;
            case 'string':
                return 'Scalar';
                return 'Simple Object';
            case 'array':
                return 'Scalar';  // Array flag is set separately
                return 'Other';
     * Standard output parameters for all MCP tool actions.
    private static readonly MCP_OUTPUT_PARAMS = [
        { Name: 'ToolOutput', ValueType: 'Other' as const, IsArray: false, Description: 'Raw output content from the MCP tool' },
        { Name: 'StructuredOutput', ValueType: 'Simple Object' as const, IsArray: false, Description: 'Parsed/structured output if available' },
        { Name: 'DurationMs', ValueType: 'Scalar' as const, IsArray: false, Description: 'Tool execution duration in milliseconds' },
        { Name: 'IsToolError', ValueType: 'Scalar' as const, IsArray: false, Description: 'Whether the tool returned an error response' }
     * Standard result codes for all MCP tool actions.
    private static readonly MCP_RESULT_CODES = [
        { ResultCode: 'SUCCESS', IsSuccess: true, Description: 'Tool executed successfully' },
        { ResultCode: 'TOOL_NOT_FOUND', IsSuccess: false, Description: 'No MCP Server Tool linked to this action' },
        { ResultCode: 'NO_CONNECTION', IsSuccess: false, Description: 'No active connection available for the MCP server' },
        { ResultCode: 'TOOL_ERROR', IsSuccess: false, Description: 'Tool executed but returned an error response' },
        { ResultCode: 'EXECUTION_FAILED', IsSuccess: false, Description: 'Protocol or transport error during tool execution' },
        { ResultCode: 'UNEXPECTED_ERROR', IsSuccess: false, Description: 'Unhandled exception during action execution' }
     * Syncs standard output parameters for MCP tool actions.
     * @returns Counts of created and updated params
    private async syncMCPOutputParams(
    ): Promise<{ created: number; updated: number }> {
        // Get existing output params for this action
            ExtraFilter: `ActionID='${actionId}' AND Type='Output'`,
        // Process standard MCP output params
        for (const paramDef of MCPClientManager.MCP_OUTPUT_PARAMS) {
            const existing = existingParamMap.get(paramDef.Name);
                // Update existing param if needed
                let dirty = false;
                if (existing.Description !== paramDef.Description) {
                    existing.Description = paramDef.Description;
                    dirty = true;
                if (existing.ValueType !== paramDef.ValueType) {
                    existing.ValueType = paramDef.ValueType;
                if (existing.IsArray !== paramDef.IsArray) {
                    existing.IsArray = paramDef.IsArray;
                // Create new output param
                param.Name = paramDef.Name;
                param.Type = 'Output';
                param.ValueType = paramDef.ValueType;
                param.IsArray = paramDef.IsArray;
                param.IsRequired = false;
                param.Description = paramDef.Description;
        return { created, updated };
     * Syncs standard result codes for MCP tool actions.
    private async syncMCPResultCodes(
        // Get existing result codes for this action
        const existingResult = await rv.RunView<MJActionResultCodeEntity>({
            EntityName: 'MJ: Action Result Codes',
        const existingCodes = existingResult.Success ? existingResult.Results : [];
        const existingCodeMap = new Map(existingCodes.map(c => [c.ResultCode, c]));
        // Process standard MCP result codes
        for (const codeDef of MCPClientManager.MCP_RESULT_CODES) {
            const existing = existingCodeMap.get(codeDef.ResultCode);
                // Update existing code if needed
                if (existing.IsSuccess !== codeDef.IsSuccess) {
                    existing.IsSuccess = codeDef.IsSuccess;
                if (existing.Description !== codeDef.Description) {
                    existing.Description = codeDef.Description;
                // Create new result code
                const code = await md.GetEntityObject<MJActionResultCodeEntity>('MJ: Action Result Codes', contextUser);
                code.NewRecord();
                code.ActionID = actionId;
                code.ResultCode = codeDef.ResultCode;
                code.IsSuccess = codeDef.IsSuccess;
                code.Description = codeDef.Description;
                await code.Save();
     * Tests a connection to an MCP server
     * @param connectionId - The connection ID to test
     * @returns Test result
    public async testConnection(
    ): Promise<MCPTestConnectionResult> {
            // Try to connect
            const wasConnected = this.isConnected(connectionId);
            if (!wasConnected) {
                await this.connect(connectionId, { ...options, skipAutoSync: true });
                throw new Error('Failed to establish connection');
            const serverInfo = client.getServerVersion();
            const result: MCPTestConnectionResult = {
                serverName: serverInfo?.name,
                serverVersion: serverInfo?.version,
                capabilities: connection.capabilities,
                latencyMs: Date.now() - startTime
            // Disconnect if we connected just for the test
                await this.disconnect(connectionId, options);
    // Event Handling
     * Adds an event listener
     * @param eventType - Event type to listen for
     * @param listener - Listener function
    public addEventListener(eventType: MCPClientEventType, listener: MCPClientEventListener): void {
        const listeners = this.eventListeners.get(eventType);
        if (listeners) {
            listeners.add(listener);
     * Removes an event listener
     * @param eventType - Event type
     * @param listener - Listener function to remove
    public removeEventListener(eventType: MCPClientEventType, listener: MCPClientEventListener): void {
            listeners.delete(listener);
     * Gets the execution logger for accessing log data
    public get ExecutionLogger(): ExecutionLogger {
        return this.logger;
     * Gets information about an active connection
     * @param connectionId - Connection ID
     * @returns Connection info or null if not connected
    public getConnectionInfo(connectionId: string): { serverName: string; connectionName: string; connectedAt: Date } | null {
            serverName: connection.serverConfig.Name,
            connectionName: connection.connectionConfig.Name,
            connectedAt: connection.connectedAt
    // OAuth Event Notification Methods
     * Notifies listeners that OAuth authorization has completed successfully.
     * Called by OAuth callback handler after code exchange succeeds.
     * @param connectionId - The connection ID that was authorized
     * @param data - Optional additional data (token info, etc.)
    public notifyOAuthAuthorizationCompleted(connectionId: string, data?: Record<string, unknown>): void {
            type: 'authorizationCompleted',
     * Notifies listeners that an OAuth token was successfully refreshed.
     * Called by TokenManager after successful token refresh.
     * @param connectionId - The connection ID whose token was refreshed
     * @param data - Optional additional data (new expiration, etc.)
    public notifyOAuthTokenRefreshed(connectionId: string, data?: Record<string, unknown>): void {
            type: 'tokenRefreshed',
     * Notifies listeners that OAuth token refresh failed.
     * Called by TokenManager when refresh fails.
     * @param connectionId - The connection ID whose token refresh failed
     * @param data - Error details and whether re-authorization is required
    public notifyOAuthTokenRefreshFailed(connectionId: string, data: {
        error: string;
        requiresReauthorization: boolean;
        authorizationUrl?: string;
        stateParameter?: string;
    // Private Helper Methods
     * Loads connection configuration from database
    private async loadConnectionConfig(connectionId: string, contextUser: UserInfo): Promise<MCPConnectionConfig | null> {
            const result = await rv.RunView<MCPConnectionConfig>({
                EntityName: MCPClientManager.ENTITY_MCP_CONNECTIONS,
                ExtraFilter: `ID = '${connectionId}'`,
            LogError(`[MCPClient] Failed to load connection config: ${error}`);
     * Loads server configuration from database
    private async loadServerConfig(serverId: string, contextUser: UserInfo): Promise<MCPServerConfig | null> {
            const result = await rv.RunView<MCPServerConfig>({
                ExtraFilter: `ID = '${serverId}'`,
            LogError(`[MCPClient] Failed to load server config: ${error}`);
     * Loads tools for a server from database
    private async loadServerTools(serverId: string, contextUser: UserInfo): Promise<MCPToolDefinition[]> {
            const result = await rv.RunView<MCPToolDefinition>({
                ExtraFilter: `MCPServerID = '${serverId}'`,
            LogError(`[MCPClient] Failed to load server tools: ${error}`);
     * Gets tool ID by server ID and tool name
    private async getToolId(serverId: string, toolName: string, contextUser: UserInfo): Promise<string | undefined> {
                ExtraFilter: `MCPServerID = '${serverId}' AND ToolName = '${toolName}'`,
     * Gets credentials from CredentialEngine
    private async getCredentials(credentialId: string, contextUser: UserInfo): Promise<MCPCredentialData> {
            const credential = CredentialEngine.Instance.getCredentialById(credentialId);
                throw new Error(`Credential not found: ${credentialId}`);
            // Get decrypted values
            const resolved = await CredentialEngine.Instance.getCredential(credential.Name, {
                contextUser,
                credentialId,
                subsystem: 'MCPClient'
            return resolved.values as MCPCredentialData;
            LogError(`[MCPClient] Failed to get credentials: ${error}`);
     * Gets OAuth2 access token for an MCP server connection.
     * Uses OAuthManager to get a valid access token, refreshing if needed.
     * If authorization is required, throws OAuthAuthorizationRequiredError.
     * @param connectionId - MCP Server Connection ID
     * @param serverConfig - Server configuration with OAuth settings
     * @returns Credential data with the access token
     * @throws OAuthAuthorizationRequiredError if user authorization is needed
     * @throws OAuthReauthorizationRequiredError if re-authorization is needed
    private async getOAuth2Credentials(
        serverConfig: MCPServerConfig,
    ): Promise<MCPCredentialData> {
        if (!this.publicUrl) {
                'Public URL not configured. Call MCPClientManager.Instance.setPublicUrl() or ' +
                'initialize with publicUrl option before using OAuth2 authentication.'
        // Build OAuth config from server settings
        const oauthConfig: MCPServerOAuthConfig = {
            OAuthIssuerURL: serverConfig.OAuthIssuerURL,
            OAuthScopes: serverConfig.OAuthScopes,
            OAuthMetadataCacheTTLMinutes: serverConfig.OAuthMetadataCacheTTLMinutes,
            OAuthClientID: serverConfig.OAuthClientID,
            OAuthClientSecretEncrypted: serverConfig.OAuthClientSecretEncrypted,
            OAuthRequirePKCE: serverConfig.OAuthRequirePKCE
        // Get a valid access token (may throw OAuthAuthorizationRequiredError)
        const accessToken = await this.oauthManager.getAccessToken(
            serverConfig.ID,
            oauthConfig,
            this.publicUrl,
        // Return credentials in the expected format
            apiKey: accessToken  // OAuth2 uses Bearer token in Authorization header
     * Gets the OAuth connection status for a connection.
     * @returns OAuth connection status or null if not an OAuth2 connection
    public async getOAuthConnectionStatus(
        isOAuthEnabled: boolean;
        hasValidTokens: boolean;
        reauthorizationReason?: string;
        tokenExpiresAt?: Date;
    } | null> {
            // Load connection and server config
            if (!serverConfig || serverConfig.DefaultAuthType !== 'OAuth2') {
                return { isOAuthEnabled: false, hasValidTokens: false, requiresReauthorization: false };
            const status = await this.oauthManager.getConnectionStatus(connectionId, oauthConfig, contextUser);
                isOAuthEnabled: status.isOAuthEnabled,
                hasValidTokens: status.hasValidTokens,
                requiresReauthorization: status.requiresReauthorization,
                reauthorizationReason: status.reauthorizationReason,
                tokenExpiresAt: status.tokenExpiresAt
            LogError(`[MCPClient] Failed to get OAuth status: ${error}`);
     * Creates the appropriate transport based on configuration
    private async createTransport(
        connectionConfig: MCPConnectionConfig,
        credentials?: MCPCredentialData
    ): Promise<Transport> {
        const transportType = serverConfig.TransportType as MCPTransportType;
        switch (transportType) {
            case 'StreamableHTTP':
                return this.createStreamableHTTPTransport(serverConfig, connectionConfig, authType, credentials);
            case 'SSE':
                return this.createSSETransport(serverConfig, connectionConfig, authType, credentials);
            case 'Stdio':
                return this.createStdioTransport(serverConfig, connectionConfig);
            case 'WebSocket':
                return this.createWebSocketTransport(serverConfig, connectionConfig, authType, credentials);
                throw new Error(`Unsupported transport type: ${transportType}`);
     * Creates a StreamableHTTP transport
    private createStreamableHTTPTransport(
        authType: MCPAuthType,
    ): Transport {
        if (!serverConfig.ServerURL) {
            throw new Error('ServerURL is required for StreamableHTTP transport');
        const headers = this.buildAuthHeaders(authType, credentials, connectionConfig.CustomHeaderName);
        return new StreamableHTTPClientTransport(new URL(serverConfig.ServerURL), {
            requestInit: headers ? { headers } : undefined
     * Creates an SSE transport
    private createSSETransport(
            throw new Error('ServerURL is required for SSE transport');
        return new SSEClientTransport(new URL(serverConfig.ServerURL), {
     * Creates a Stdio transport
    private createStdioTransport(
        connectionConfig: MCPConnectionConfig
        if (!serverConfig.Command) {
            throw new Error('Command is required for Stdio transport');
        let args: string[] = [];
        if (serverConfig.CommandArgs) {
                args = JSON.parse(serverConfig.CommandArgs);
                LogError('[MCPClient] Failed to parse command args');
        let env: Record<string, string> = { ...process.env } as Record<string, string>;
        if (connectionConfig.EnvironmentVars) {
                const customEnv = JSON.parse(connectionConfig.EnvironmentVars);
                env = { ...env, ...customEnv };
                LogError('[MCPClient] Failed to parse environment vars');
        return new StdioClientTransport({
            command: serverConfig.Command,
            env
     * Creates a WebSocket transport
    private createWebSocketTransport(
        _connectionConfig: MCPConnectionConfig,
            throw new Error('ServerURL is required for WebSocket transport');
        // For WebSocket, credentials are typically passed in the URL or via protocol headers
        let wsUrl = serverConfig.ServerURL;
        // Add auth to URL if using Bearer or APIKey
        if (authType === 'Bearer' && credentials?.apiKey) {
            const url = new URL(wsUrl);
            url.searchParams.set('token', credentials.apiKey);
            wsUrl = url.toString();
        } else if (authType === 'APIKey' && credentials?.apiKey) {
            url.searchParams.set('api_key', credentials.apiKey);
        return new WebSocketClientTransport(new URL(wsUrl));
     * Builds authentication headers based on auth type
    private buildAuthHeaders(
        credentials?: MCPCredentialData,
        customHeaderName?: string
    ): Record<string, string> | undefined {
        if (authType === 'None' || !credentials) {
        const headers: Record<string, string> = {};
        switch (authType) {
            case 'Bearer':
                if (credentials.apiKey) {
                    headers['Authorization'] = `Bearer ${credentials.apiKey}`;
            case 'APIKey':
                    const headerName = customHeaderName || 'X-API-Key';
                    headers[headerName] = credentials.apiKey;
            case 'Basic':
                if (credentials.username && credentials.password) {
                    const encoded = Buffer.from(`${credentials.username}:${credentials.password}`).toString('base64');
                    headers['Authorization'] = `Basic ${encoded}`;
            case 'Custom':
                if (credentials.apiKey && customHeaderName) {
                    headers[customHeaderName] = credentials.apiKey;
            case 'OAuth2':
                // OAuth2 would typically use a token obtained from the token endpoint
                // For now, use the apiKey field as the access token
        return Object.keys(headers).length > 0 ? headers : undefined;
     * Maps MCP server capabilities to our interface
    private mapServerCapabilities(capabilities: ReturnType<Client['getServerCapabilities']>): MCPServerCapabilities | undefined {
        if (!capabilities) {
            logging: !!capabilities.logging,
            completions: !!capabilities.completions,
            prompts: capabilities.prompts ? {
                listChanged: !!capabilities.prompts.listChanged
            resources: capabilities.resources ? {
                subscribe: !!capabilities.resources.subscribe,
                listChanged: !!capabilities.resources.listChanged
            tools: capabilities.tools ? {
                listChanged: !!capabilities.tools.listChanged
     * Maps MCP tool call result to our interface
    private mapToolCallResult(result: CallToolResult, durationMs: number): MCPToolCallResult {
        const content: MCPContentBlock[] = (result.content || []).map(block => {
                return { type: 'text' as const, text: block.text };
            } else if (block.type === 'image') {
                    type: 'image' as const,
                    mimeType: block.mimeType,
                    data: block.data
            } else if (block.type === 'audio') {
                    type: 'audio' as const,
            } else if (block.type === 'resource') {
                    type: 'resource' as const,
                    uri: block.resource?.uri
            return { type: 'text' as const, text: JSON.stringify(block) };
            success: !result.isError,
            structuredContent: result.structuredContent as Record<string, unknown> | undefined,
            isToolError: !!result.isError,
            durationMs
     * Gets logging configuration from connection config
    private getLoggingConfig(connectionConfig: MCPConnectionConfig): MCPLoggingConfig {
            logToolCalls: connectionConfig.LogToolCalls,
            logInputParameters: connectionConfig.LogInputParameters,
            logOutputContent: connectionConfig.LogOutputContent,
            maxOutputLogSize: connectionConfig.MaxOutputLogSize ?? 102400
     * Checks if user has permission for a connection.
     * Permission logic:
     * 1. If NO permission records exist for this connection, it's "open" - anyone can use it
     * 2. If permission records exist, user must have an explicit grant (direct or via role)
     * 3. Admins always have access
    private async checkPermission(
        permission: 'execute' | 'modify' | 'viewCredentials'
            // Admins always have access
            const hasAdminRole = contextUser.UserRoles?.some(
                role => role.Role?.toLowerCase() === 'admin' || role.Role?.toLowerCase() === 'administrator'
            ) ?? false;
            if (hasAdminRole) {
            // First, check if ANY permissions exist for this connection
            const allPermissions = await rv.RunView<MCPConnectionPermission>({
                EntityName: MCPClientManager.ENTITY_MCP_PERMISSIONS,
            // If no permissions are configured for this connection, it's open to all
            if (!allPermissions.Success || !allPermissions.Results || allPermissions.Results.length === 0) {
            // Permissions exist - check if user has explicit access
            const userRolesSchema = md.EntityByName("MJ: User Roles")?.SchemaName ?? '__mj';
            const userPermissions = await rv.RunView<MCPConnectionPermission>({
                ExtraFilter: `MCPServerConnectionID = '${connectionId}' AND (UserID = '${contextUser.ID}' OR RoleID IN (SELECT RoleID FROM [${userRolesSchema}].vwUserRoles WHERE UserID = '${contextUser.ID}'))`,
            if (!userPermissions.Success || !userPermissions.Results || userPermissions.Results.length === 0) {
                // Permissions exist but user doesn't have any
            // Check if any permission grants the requested access
            for (const perm of userPermissions.Results) {
                    case 'execute':
                        if (perm.CanExecute) return true;
                    case 'modify':
                        if (perm.CanModify) return true;
                    case 'viewCredentials':
                        if (perm.CanViewCredentials) return true;
            LogError(`[MCPClient] Permission check failed: ${error}`);
     * Updates connection status in database
    private async updateConnectionStatus(
        status: 'Active' | 'Inactive' | 'Error',
        error?: unknown
            const entity = await md.GetEntityObject<MJMCPServerConnectionEntity>(
                MCPClientManager.ENTITY_MCP_CONNECTIONS,
            const loaded = await entity.Load(connectionId);
                entity.Status = status;
                if (status === 'Active') {
                    entity.LastConnectedAt = new Date();
                    entity.LastErrorMessage = null;
                } else if (status === 'Error' && error) {
                    entity.LastErrorMessage = error instanceof Error ? error.message : String(error);
                await entity.Save();
            LogError(`[MCPClient] Failed to update connection status: ${e}`);
     * Updates server LastSyncAt in database
    private async updateServerLastSync(serverId: string, contextUser: UserInfo): Promise<void> {
            const entity = await md.GetEntityObject<MJMCPServerEntity>(
                MCPClientManager.ENTITY_MCP_SERVERS,
            const loaded = await entity.Load(serverId);
                entity.LastSyncAt = new Date();
            LogError(`[MCPClient] Failed to update server last sync: ${e}`);
     * Emits an event to all listeners
    private emitEvent(event: MCPClientEvent): void {
        const listeners = this.eventListeners.get(event.type);
            for (const listener of listeners) {
                    listener(event);
                    LogError(`[MCPClient] Event listener error: ${e}`);
