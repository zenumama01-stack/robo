import { EntityInfo, LogError, LogStatus, Metadata, UserInfo } from "@memberjunction/core";
import { setupSQLServerClient, SQLServerProviderConfigData, UserCache } from "@memberjunction/sqlserver-dataprovider";
import { GetAPIKeyEngine } from "@memberjunction/api-keys";
import express, { Request, Response, NextFunction } from 'express';
import sql from 'mssql';
import { z } from "zod";
import { configInfo, dbDatabase, dbHost, dbPassword, dbPort, dbUsername, dbInstanceName, dbTrustServerCertificate, a2aServerSettings } from './config.js';
import { EntityOperations, OperationResult } from './EntityOperations.js';
import { AgentOperations } from './AgentOperations.js';
import { AIAgentEntityExtended } from "@memberjunction/ai-core-plus";
// A2A Server Configuration
const a2aServerPort = a2aServerSettings?.port || 3200;
// Database Configuration
const poolConfig: sql.config = {
    server: dbHost,
    port: dbPort,
    user: dbUsername,
    password: dbPassword,
    database: dbDatabase,
    requestTimeout: configInfo.databaseSettings.requestTimeout,
    connectionTimeout: configInfo.databaseSettings.connectionTimeout,
        encrypt: true,
        enableArithAbort: true,
        trustServerCertificate: dbTrustServerCertificate === 'Y'
if (dbInstanceName !== null && dbInstanceName !== undefined && dbInstanceName.trim().length > 0) {
    poolConfig.options!.instanceName = dbInstanceName;
// A2A Server Classes and Types
type TaskStatus = 'pending' | 'in_progress' | 'completed' | 'cancelled' | 'failed';
interface Task {
    id: string;
    status: TaskStatus;
    messages: Message[];
    artifacts: Artifact[];
    created: Date;
    updated: Date;
interface Message {
    taskId: string;
    role: 'user' | 'agent';
    parts: Part[];
interface Part {
    type: 'text' | 'file' | 'data';
    content: string | object;
    metadata?: object;
interface Artifact {
    name: string;
interface AgentCard {
    version: string;
    endpoints: {
        tasks: string;
        agentCard: string;
    authentication?: {
        type: string;
        scheme: string;
    capabilities: {
        streaming: boolean;
        asynchronous: boolean;
        multimedia: boolean;
        entities: {
            schema: string;
            operations: string[];
        agents?: {
// In-memory storage for tasks (in production, this would use a database)
const tasks = new Map<string, Task>();
// Express application
const app = express();
app.use(express.json());
 * Extract request context from an Express request for API key logging.
function extractRequestContext(req: Request): {
    endpoint: string;
    method: string;
    ipAddress: string | null;
    userAgent: string | null;
} {
        endpoint: req.path || req.url || '/a2a',
        method: req.method || 'POST',
        ipAddress: req.ip || req.socket?.remoteAddress || null,
        userAgent: req.headers['user-agent'] as string || null,
 * Authorization context for task processing
interface AuthorizationContext {
    apiKeyHash: string;
    user: UserInfo;
 * Authorize an operation against the API key's scope permissions.
 * Uses the two-level scope evaluation (application ceiling + key scopes).
 * @param authContext - The authorization context with API key hash and user
 * @param scopePath - The scope path (e.g., 'action:execute', 'agent:execute')
 * @param resource - The specific resource being accessed
 * @returns Object with allowed flag and error message if denied
async function authorizeOperation(
    authContext: AuthorizationContext,
    scopePath: string,
    resource: string
): Promise<{ allowed: boolean; error?: string }> {
    const systemUser = UserCache.Instance.Users[0];
    if (!systemUser) {
        return { allowed: false, error: 'System user not available for authorization' };
        const apiKeyEngine = GetAPIKeyEngine();
        const result = await apiKeyEngine.Authorize(
            authContext.apiKeyHash,
            'A2AServer',
            scopePath,
            systemUser,
                endpoint: '/a2a/tasks',
                method: 'POST',
                operation: scopePath,
        if (!result.Allowed) {
            LogStatus(`A2A Server: Authorization denied - ${result.Reason}`);
            return { allowed: false, error: result.Reason };
        return { allowed: true };
        LogError('A2A Server: Authorization error', undefined, error);
        return { allowed: false, error: error instanceof Error ? error.message : 'Authorization failed' };
 * Authentication middleware for A2A endpoints.
 * Validates MJ API keys (X-API-Key header with mj_sk_* format).
async function authenticateRequest(req: Request, res: Response, next: NextFunction): Promise<void> {
    const apiKey = req.headers['x-api-key'] as string | undefined;
    if (!apiKey) {
        res.status(401).json({
            error: {
                code: 401,
                message: 'Missing API key. Provide X-API-Key header with a valid MJ API key.'
        // Get system user for validation context
            LogError('A2A Server: No system user available for API key validation');
            res.status(500).json({
                    code: 500,
                    message: 'Server configuration error'
        const requestContext = extractRequestContext(req);
        const validationResult = await apiKeyEngine.ValidateAPIKey(
                RawKey: apiKey,
                ApplicationName: 'A2AServer', // Check if key is bound to this application
                Endpoint: requestContext.endpoint,
                Method: requestContext.method,
                Operation: undefined, // Could extract from request body if available
                StatusCode: 200, // Auth succeeded if we get past validation
                ResponseTimeMs: undefined, // Not available at auth time
                IPAddress: requestContext.ipAddress ?? undefined,
                UserAgent: requestContext.userAgent ?? undefined,
            systemUser
        if (!validationResult.IsValid) {
            LogStatus(`A2A Server: Invalid API key attempt from ${requestContext.ipAddress}`);
                    message: 'Invalid API key'
        // Store the authenticated user on the request for use in handlers
        (req as RequestWithUser).authenticatedUser = validationResult.User!;
        (req as RequestWithUser).apiKeyId = validationResult.APIKeyId;
        (req as RequestWithUser).apiKeyHash = validationResult.APIKeyHash;
        next();
        LogError('A2A Server: API key validation error', undefined, error);
                message: 'Authentication error'
 * Extended Request type with authenticated user information.
interface RequestWithUser extends Request {
    authenticatedUser?: UserInfo;
    apiKeyId?: string;
    apiKeyHash?: string;
// Initialize A2A server
export async function initializeA2AServer() {
        if (!a2aServerSettings?.enableA2AServer) {
            console.log("A2A Server is disabled in the configuration.");
            throw new Error("A2A Server is disabled in the configuration.");
        // Initialize database connection
        const pool = new sql.ConnectionPool(poolConfig);
        await pool.connect();
        // Setup SQL Server client
        const config = new SQLServerProviderConfigData(pool, configInfo.mjCoreSchema);
        await setupSQLServerClient(config);
        console.log("Database connection setup completed.");
        // Set up routes
        setupRoutes();
        // Start the server
        app.listen(a2aServerPort, () => {
            console.log(`MemberJunction A2A Server running on port ${a2aServerPort}`);
            console.log(`Agent card available at: http://localhost:${a2aServerPort}/a2a/agent-card`);
        console.error("Failed to initialize A2A server:", error);
function setupRoutes() {
    // Agent Card endpoint (public - for discovery)
    app.get('/a2a/agent-card', async (req, res) => {
        const agentCard = await generateAgentCard();
        res.json(agentCard);
    // Apply authentication middleware to all /a2a/tasks routes
    app.use('/a2a/tasks', authenticateRequest);
    // Send a message to a task
    app.post('/a2a/tasks/send', (req: Request, res: Response) => {
        const reqWithUser = req as RequestWithUser;
        const authContext = reqWithUser.apiKeyHash && reqWithUser.authenticatedUser
            ? { apiKeyHash: reqWithUser.apiKeyHash, user: reqWithUser.authenticatedUser }
            : undefined;
        const result = handleTaskSend(req.body, reqWithUser.authenticatedUser, authContext);
        res.json(result);
    // Send a message to a task with streaming response
    app.post('/a2a/tasks/sendSubscribe', (req: Request, res: Response) => {
        if (!a2aServerSettings?.streamingEnabled) {
            res.status(400).json({
                    code: 400,
                    message: "Streaming is not enabled for this agent"
        // Set up SSE connection
        res.setHeader('Content-Type', 'text/event-stream');
        res.setHeader('Cache-Control', 'no-cache');
        res.setHeader('Connection', 'keep-alive');
        handleTaskSendSubscribe(req.body, res, reqWithUser.authenticatedUser, authContext);
    // Get a task's status
    app.get('/a2a/tasks/:taskId', (req: Request, res: Response) => {
        const taskId = req.params.taskId as string;
        const task = tasks.get(taskId);
        if (!task) {
            res.status(404).json({
                    code: 404,
                    message: `Task with ID ${taskId} not found`
        res.json(task);
    // Cancel a task
    app.post('/a2a/tasks/:taskId/cancel', (req: Request, res: Response) => {
        task.status = 'cancelled';
        task.updated = new Date();
        res.json({ success: true });
async function generateAgentCard(): Promise<AgentCard> {
    const contextUser = UserCache.Instance.Users[0];
    const entityCapabilities = getEntityCapabilities(md.Entities, contextUser);
    const agentCapabilities = await getAgentCapabilities(contextUser);
        name: a2aServerSettings?.agentName || "MemberJunction",
        description: a2aServerSettings?.agentDescription || "MemberJunction A2A Agent",
        version: "1.0.0",
            tasks: `/a2a/tasks`,
            agentCard: `/a2a/agent-card`
        authentication: {
            type: "apiKey",
            scheme: "X-API-Key"
            streaming: !!a2aServerSettings?.streamingEnabled,
            asynchronous: false,
            multimedia: false,
            entities: entityCapabilities,
            agents: agentCapabilities
function getEntityCapabilities(allEntities: EntityInfo[], contextUser: UserInfo) {
    const capabilities = [];
    const entityCapabilitiesConfig = a2aServerSettings?.entityCapabilities || [];
    for (const config of entityCapabilitiesConfig) {
        const matchingEntities = getMatchingEntitiesForConfig(allEntities, config);
        for (const entity of matchingEntities) {
            const operations = [];
            if (config.get) operations.push('get');
            if (config.create) operations.push('create');
            if (config.update) operations.push('update');
            if (config.delete) operations.push('delete');
            if (config.runView) operations.push('query');
            if (operations.length > 0) {
                capabilities.push({
                    name: entity.Name,
                    schema: entity.SchemaName,
                    operations: operations
    return capabilities;
async function getAgentCapabilities(contextUser: UserInfo) {
    const agentCapabilitiesConfig = a2aServerSettings?.agentCapabilities || [];
    await aiEngine.Config(false, contextUser);
    for (const config of agentCapabilitiesConfig) {
        const agentPattern = config.agentName || "*";
            if (agentPattern === '*') {
                const isWildcardPattern = agentPattern.includes('*');
                    agents = allAgents.filter(a => a.Name === agentPattern);
                    const regexPattern = agentPattern
            for (const agent of agents) {
                if (config.discover) operations.push('discover');
                if (config.execute) operations.push('execute');
                if (config.monitor) operations.push('monitor');
                if (config.cancel) operations.push('cancel');
                if (operations.length > 0 && agent.Name) {
            LogError('Failed to discover agents', '', error);
    // Add general agent operations if any capability is enabled
    const hasAnyCapability = agentCapabilitiesConfig.some(c => c.discover || c.execute || c.monitor || c.cancel);
    if (hasAnyCapability) {
        const generalOps = [];
        if (agentCapabilitiesConfig.some(c => c.discover)) generalOps.push('discoverAgents');
        if (agentCapabilitiesConfig.some(c => c.monitor)) generalOps.push('getAgentRunStatus');
        if (agentCapabilitiesConfig.some(c => c.cancel)) generalOps.push('cancelAgentRun');
        if (generalOps.length > 0) {
                name: '_general',
                operations: generalOps
function getMatchingEntitiesForConfig(allEntities: EntityInfo[], config: any) {
    return allEntities.filter((entity) => {
        const entityName = entity.Name.toLowerCase();
        const schemaName = entity.SchemaName.toLowerCase();
        const configEntityName = config.entityName?.trim().toLowerCase() || "*";
        const configSchemaName = config.schemaName?.trim().toLowerCase() || "*";
        let schemaMatch = false;
        let entityMatch = false;
        // Schema matching
        if (configSchemaName === "*") {
            schemaMatch = true;
        } else if (configSchemaName.startsWith("*") && configSchemaName.endsWith("*")) {
            schemaMatch = schemaName.includes(configSchemaName.slice(1, -1));
        } else if (configSchemaName.endsWith("*")) {
            schemaMatch = schemaName.startsWith(configSchemaName.slice(0, -1));
        } else if (configSchemaName.startsWith("*")) {
            schemaMatch = schemaName.endsWith(configSchemaName.slice(1));
            schemaMatch = schemaName === configSchemaName;
        // Entity matching (only checked if schema matches)
        if (schemaMatch) {
            if (configEntityName === "*") {
                entityMatch = true;
            } else if (configEntityName.startsWith("*") && configEntityName.endsWith("*")) {
                entityMatch = entityName.includes(configEntityName.slice(1, -1));
            } else if (configEntityName.endsWith("*")) {
                entityMatch = entityName.startsWith(configEntityName.slice(0, -1));
            } else if (configEntityName.startsWith("*")) {
                entityMatch = entityName.endsWith(configEntityName.slice(1));
                entityMatch = entityName === configEntityName;
        return schemaMatch && entityMatch;
function handleTaskSend(requestBody: Record<string, unknown>, authenticatedUser?: UserInfo, authContext?: AuthorizationContext) {
    const { taskId, message } = requestBody as { taskId?: string; message?: { parts?: Part[] } };
    if (!taskId) {
        // Create a new task
        const newTaskId = generateId();
        const task: Task = {
            id: newTaskId,
            status: 'pending',
            messages: [],
            artifacts: [],
            created: new Date(),
            updated: new Date()
        if (message) {
            const newMessage: Message = {
                id: generateId(),
                taskId: newTaskId,
                role: 'user',
                parts: message.parts || [],
                created: new Date()
            task.messages.push(newMessage);
        tasks.set(newTaskId, task);
        // Process the task asynchronously with the authenticated user
        processTask(task, authenticatedUser, authContext);
            status: task.status
        // Update an existing task
            throw {
        if (task.status === 'completed' || task.status === 'cancelled' || task.status === 'failed') {
                    message: `Cannot update a task with status: ${task.status}`
                taskId,
        // Process the updated task with the authenticated user
            taskId: task.id,
function handleTaskSendSubscribe(requestBody: Record<string, unknown>, res: Response, authenticatedUser?: UserInfo, authContext?: AuthorizationContext) {
    const result = handleTaskSend(requestBody, authenticatedUser, authContext);
    const task = tasks.get(result.taskId);
        res.write(`data: ${JSON.stringify({ error: { code: 404, message: "Task not found" } })}\n\n`);
        res.end();
    // Send initial task state
    res.write(`data: ${JSON.stringify({ 
    })}\n\n`);
    // Set up a listener for task updates
    const updateInterval = setInterval(() => {
        const updatedTask = tasks.get(task.id);
        if (!updatedTask) {
            clearInterval(updateInterval);
        // Send task status updates
            taskId: updatedTask.id,
            status: updatedTask.status 
        // If task is completed, send final message and end the stream
        if (updatedTask.status === 'completed' || updatedTask.status === 'cancelled' || updatedTask.status === 'failed') {
            // Send final artifacts
            if (updatedTask.artifacts.length > 0) {
                    artifacts: updatedTask.artifacts
    // Handle client disconnect
    res.on('close', () => {
// Process a task using MemberJunction APIs
async function processTask(task: Task, authenticatedUser?: UserInfo, authContext?: AuthorizationContext) {
        task.status = 'in_progress';
        // Get the last user message
        const lastMessage = task.messages.filter(m => m.role === 'user').pop();
        if (!lastMessage) {
            throw new Error("No user message found");
        // Use authenticated user or fall back to first cached user (for backwards compatibility)
        const contextUser = authenticatedUser || UserCache.Instance.Users[0];
        if (!contextUser) {
            throw new Error("No user context available for processing task");
        // Initialize operations handlers with the authenticated user
        const entityOps = new EntityOperations();
        const agentOps = new AgentOperations(contextUser);
        // Extract text content and parse operation
        const textParts = lastMessage.parts.filter(p => p.type === 'text');
        const textContent = textParts.map(p => typeof p.content === 'string' ? p.content : JSON.stringify(p.content)).join(" ");
        // Extract structured data if any
        const dataParts = lastMessage.parts.filter(p => p.type === 'data');
        const dataContent = dataParts.length > 0 ? dataParts[0].content : null;
        // Parse the command and parameters
        let parameters: Record<string, unknown> = {};
        // Try to extract command and parameters from structured data first
        if (dataContent && typeof dataContent === 'object') {
            operation = (dataContent as Record<string, unknown>).operation as string || 'unknown';
            entityName = (dataContent as Record<string, unknown>).entity as string || '';
            parameters = (dataContent as Record<string, unknown>).parameters as Record<string, unknown> || {};
        // Otherwise parse from text
        else if (textContent) {
            const parsedCommand = entityOps.parseCommandFromText(textContent);
            operation = parsedCommand.operation;
            entityName = parsedCommand.entityName;
            parameters = parsedCommand.parameters;
        // Perform the operation
        let operationResult: OperationResult;
            // Check if this is an agent operation
            const agentOperations = ['discoverAgents', 'executeAgent', 'getAgentRunStatus', 'cancelAgentRun'];
            if (agentOperations.includes(operation)) {
                // Authorize agent operations if authContext is provided
                if (authContext) {
                    let scopePath: string | undefined;
                    let resource: string = '*';
                    switch (operation) {
                            scopePath = 'metadata:agents:read';
                            resource = (parameters.pattern as string) || '*';
                            scopePath = 'agent:execute';
                            resource = (parameters.agentName as string) || (parameters.agentId as string) || '*';
                            scopePath = 'agent:monitor';
                            resource = (parameters.runId as string) || '*';
                            scopePath = 'agent:cancel';
                    if (scopePath) {
                        const authResult = await authorizeOperation(authContext, scopePath, resource);
                        if (!authResult.allowed) {
                            operationResult = {
                                errorMessage: `Authorization denied: ${authResult.error}`
                            throw new Error(operationResult.errorMessage);
                // Agent operation
                operationResult = await agentOps.processOperation(operation, parameters);
                // Authorize entity operation if authContext is provided
                if (authContext && entityName) {
                    // Map entity operations to scopes
                    const operationScopeMap: Record<string, string> = {
                        'get': 'entity:read',
                        'create': 'entity:create',
                        'update': 'entity:update',
                        'delete': 'entity:delete',
                        'query': 'view:run',
                        'runView': 'view:run'
                    const scopePath = operationScopeMap[operation];
                        const authResult = await authorizeOperation(authContext, scopePath, entityName);
                // Regular entity operation
                if (!entityName) {
                    throw new Error("Entity name not specified");
                operationResult = await entityOps.processOperation(operation, entityName, parameters);
        // Create response message
        const responseMessage: Message = {
            role: 'agent',
            parts: [
                    type: 'text',
                    content: operationResult.success
                        ? `Successfully performed ${operation} operation on ${entityName}`
                        : `Failed to perform ${operation} operation on ${entityName}: ${operationResult.errorMessage}`
        // Create artifact with result data
        const artifact: Artifact = {
            name: `${operation}_result`,
                    type: 'data',
                        success: operationResult.success,
                        entity: entityName,
                        result: operationResult.result,
                        error: operationResult.success ? undefined : operationResult.errorMessage,
                        timestamp: new Date().toISOString()
        // Update the task
        task.messages.push(responseMessage);
        task.artifacts.push(artifact);
        task.status = 'completed';
        console.error("Error processing task:", error);
        // Create error response
        const errorMessage: Message = {
                    content: `An error occurred while processing your request: ${error instanceof Error ? error.message : String(error)}`
        // Create error artifact
        const errorArtifact: Artifact = {
            name: 'error',
                        error: error instanceof Error ? error.message : String(error),
        task.messages.push(errorMessage);
        task.artifacts.push(errorArtifact);
        task.status = 'failed';
// Utility function to generate a random ID
function generateId() {
    return Math.random().toString(36).substring(2, 15) + Math.random().toString(36).substring(2, 15);
 * @fileoverview MemberJunction MCP Server
 * This module implements a Model Context Protocol (MCP) server that exposes MemberJunction
 * entities, agents, and actions as tools that can be consumed by AI models and other MCP clients.
 * Key features:
 * - API key authentication with user context
 * - Dynamic tool generation from entity metadata
 * - Agent discovery and execution
 * - Configurable tool filtering
 * @module @memberjunction/ai-mcp-server
import { BaseEntity, CompositeKey, EntityFieldInfo, EntityInfo, Metadata, RunView, RunQuery, UserInfo } from "@memberjunction/core";
import { McpServer } from "@modelcontextprotocol/sdk/server/mcp.js";
import { SSEServerTransport } from "@modelcontextprotocol/sdk/server/sse.js";
import { StreamableHTTPServerTransport } from "@modelcontextprotocol/sdk/server/streamableHttp.js";
import { randomUUID } from 'crypto';
import sql from "mssql";
import { initConfig, ConfigInfo, MCPServerActionToolInfo, MCPServerPromptToolInfo, MCPServerAgentToolInfo, MCPServerEntityToolInfo } from './config.js';
import { AIAgentEntityExtended, AIAgentRunEntityExtended, AIAgentRunStepEntityExtended, AIPromptEntityExtended } from "@memberjunction/ai-core-plus";
import * as fs from 'fs/promises';
import * as path from 'path';
import { CredentialEngine } from "@memberjunction/credentials";
import * as http from 'http';
import { ActionEntityExtended, RunActionParams } from "@memberjunction/actions-base";
import { ActionEngineServer } from "@memberjunction/actions";
import { AIPromptRunner } from "@memberjunction/ai-prompts";
import { AIPromptParams } from "@memberjunction/ai-core-plus";
import { MJActionParamEntity } from "@memberjunction/core-entities";
// OAuth authentication imports
    MCPSessionContext as OAuthMCPSessionContext,
    AuthMode,
    AuthResult,
    ProtectedResourceMetadata,
} from './auth/types.js';
    getAuthMode,
    getResourceIdentifier,
    isOAuthEnabled,
    validateOAuthConfig,
    logAuthConfig,
} from './auth/OAuthConfig.js';
    authenticateRequest as oauthAuthenticateRequest,
    toSessionContext,
    sendAuthErrorResponse,
    AuthGateConfig,
} from './auth/AuthGate.js';
    buildProtectedResourceMetadata,
    extractAuthorizationServers,
} from './auth/ProtectedResourceMetadata.js';
import { hasAuthProviders, setProxyTokenConfig } from './auth/TokenValidator.js';
import { send401Response } from './auth/WWWAuthenticate.js';
// OAuth Proxy imports
import { createOAuthProxyRouter } from './auth/OAuthProxyRouter.js';
import type { OAuthProxyConfig } from './auth/OAuthProxyTypes.js';
/*******************************************************************************
 * TYPES AND INTERFACES
 ******************************************************************************/
 * Options for filtering which tools are exposed by the MCP server.
 * Supports glob-style patterns for flexible tool selection.
export interface ToolFilterOptions {
    /** Patterns that tools must match to be included (supports wildcards: *, prefix*, *suffix, *contains*) */
    includePatterns?: string[];
    /** Patterns that exclude matching tools (supports wildcards: *, prefix*, *suffix, *contains*) */
    excludePatterns?: string[];
 * Session context stored for each authenticated MCP connection.
 * Extended to support both API key and OAuth authentication.
interface MCPSessionContext {
    /** The raw API key used for authentication (present when authMethod='apiKey') */
    /** The database ID of the API key record (present when authMethod='apiKey') */
    /** The SHA-256 hash of the API key (present when authMethod='apiKey') */
    /** The MemberJunction user associated with the authenticated session */
    /** Authentication method used for this session */
    authMethod: 'apiKey' | 'oauth' | 'none';
     * Granted scopes for this session.
     * Populated from OAuth JWT 'scopes' claim or APIKeyScope entity.
     * Used for authorization checks.
    scopes?: string[];
    /** OAuth-specific context (present when authMethod='oauth') */
    oauth?: {
        subject: string;
        email: string;
        /** Granted scopes from the JWT token */
 * MODULE STATE
/** Registry of all tool names (used for --list-tools CLI option) */
const registeredToolNames: string[] = [];
/** Currently active filter options for tool registration */
let activeFilterOptions: ToolFilterOptions = {};
/** Configuration loaded from initConfig() - populated in initializeServer()
 * Uses definite assignment assertion (!) because it's assigned before use in initializeServer() */
let _config!: ConfigInfo;
/** MCP server port - populated after config is loaded */
let mcpServerPort!: number;
/** Map of SSE transports keyed by session ID for message endpoint routing */
const transports: Map<string, SSEServerTransport> = new Map();
 * DATABASE CONFIGURATION
 * Builds the SQL Server connection pool configuration from the loaded config.
 * @returns The mssql connection pool configuration object
function buildPoolConfig(): sql.config {
    const config: sql.config = {
        server: _config.dbHost,
        port: _config.dbPort,
        user: _config.dbUsername,
        password: _config.dbPassword,
        database: _config.dbDatabase,
        requestTimeout: _config.databaseSettings.requestTimeout,
        connectionTimeout: _config.databaseSettings.connectionTimeout,
            trustServerCertificate: _config.dbTrustServerCertificate === 'Y'
    if (_config.dbInstanceName !== null && _config.dbInstanceName !== undefined && _config.dbInstanceName.trim().length > 0) {
        config.options!.instanceName = _config.dbInstanceName;
 * AUTHENTICATION
 * Extracts an API key from an incoming HTTP request.
 * Checks multiple sources in order of preference:
 * 1. x-api-key header
 * 2. x-mj-api-key header
 * 3. Authorization: Bearer <token> header
 * 4. URL query parameter (apiKey or api_key)
 * @param request - The incoming HTTP request
 * @returns The extracted API key string, or null if not found
function extractAPIKeyFromRequest(request: Request | http.IncomingMessage): string | null {
    // Check dedicated API key headers first
    let apiKey = request.headers['x-api-key'] as string
        || request.headers['x-mj-api-key'] as string;
    // Check Authorization header (Bearer token format)
    if (!apiKey && request.headers['authorization']) {
        const authHeader = request.headers['authorization'] as string;
        if (authHeader.startsWith('Bearer ')) {
            apiKey = authHeader.substring(7); // Remove "Bearer " prefix
    // Check URL query parameters as fallback
    if (!apiKey && request.url) {
        const url = new URL(request.url, `http://${request.headers.host}`);
        const queryKey = url.searchParams.get('apiKey') || url.searchParams.get('api_key');
        if (queryKey) {
            apiKey = queryKey;
    return apiKey || null;
 * Extracts request context from an HTTP request for API key usage logging.
 * @returns Object containing endpoint, method, IP address, and user agent
function extractRequestContext(request: Request | http.IncomingMessage): {
    // Extract endpoint from URL (without query params)
    let endpoint = '/mcp';
    if (request.url) {
            endpoint = url.pathname || '/mcp';
            endpoint = request.url.split('?')[0] || '/mcp';
    // Extract client IP address
    // Check X-Forwarded-For header first (for proxied requests)
    const forwardedFor = request.headers['x-forwarded-for'];
    let ipAddress: string | null = null;
    if (forwardedFor) {
        // X-Forwarded-For can be a comma-separated list; take the first one
        ipAddress = (Array.isArray(forwardedFor) ? forwardedFor[0] : forwardedFor).split(',')[0].trim();
    } else if ('socket' in request && request.socket) {
        ipAddress = request.socket.remoteAddress || null;
        endpoint,
        method: request.method || 'POST',
        ipAddress,
        userAgent: request.headers['user-agent'] as string || null,
 * Validates an API key and returns the validation result.
 * This is used by the AuthGate middleware for API key authentication.
 * @param apiKey - The raw API key to validate
 * @param request - The incoming HTTP request (for logging context)
 * @returns Validation result with user if valid
async function validateApiKey(
    apiKey: string,
    request: Request | http.IncomingMessage
    valid: boolean;
    user?: UserInfo;
        const systemUser = UserCache.Instance.GetSystemUser();
            return { valid: false, error: 'System user not found for API key validation' };
        // Check for system API key first
        // Priority: mcpServerSettings.systemApiKey (MCP-specific) > apiKey (from MJ_API_KEY env var)
        // This authenticates as the system user for system-level operations
        const mcpSystemKey = _config.mcpServerSettings?.systemApiKey;
        const sharedApiKey = _config.apiKey;
        const configuredSystemApiKey = mcpSystemKey || sharedApiKey;
        if (configuredSystemApiKey && apiKey === configuredSystemApiKey) {
                valid: true,
                user: systemUser,
                // System API key doesn't have an ID/hash in the database
                apiKeyId: 'system',
                apiKeyHash: 'system',
        // Fall back to user-level API key validation through the API Key Engine
        const requestContext = extractRequestContext(request);
        const validation = await apiKeyEngine.ValidateAPIKey(
                ApplicationName: 'MCPServer',
                Operation: undefined,
                StatusCode: 200,
                ResponseTimeMs: undefined,
        if (!validation.IsValid) {
            return { valid: false, error: validation.Error || 'Invalid API key' };
        // Get user from UserCache to ensure EntityPermissions are loaded
        const cachedUser = UserCache.Instance.Users.find(u => u.ID === validation.User?.ID);
        if (!cachedUser) {
            return { valid: false, error: 'User not found in cache' };
            user: cachedUser,
            apiKeyId: validation.APIKeyId!,
            apiKeyHash: validation.APIKeyHash!,
            error: error instanceof Error ? error.message : 'API key validation failed',
/** AuthGate configuration - populated during server initialization */
let authGateConfig: AuthGateConfig;
 * Authenticates an incoming MCP request using the configured auth mode.
 * Supports API key, OAuth Bearer token, both, or none (development).
 * @param request - The incoming HTTP request from the MCP client
 * @returns Promise resolving to the session context with authenticated user
 * @throws Error if authentication fails
async function authenticateRequest(request: Request | http.IncomingMessage): Promise<MCPSessionContext> {
    const result = await oauthAuthenticateRequest(request, authGateConfig);
    if (!result.authenticated) {
        const error = result.error;
        throw new Error(error?.message || 'Authentication failed');
    return toSessionContext(result);
 * Authenticates a request and returns the full AuthResult for error handling.
 * Use this when you need to send proper WWW-Authenticate headers on failure.
 * @returns The AuthResult with success/failure details
async function authenticateRequestWithResult(
): Promise<AuthResult> {
    return oauthAuthenticateRequest(request, authGateConfig);
 * TOOL AUTHORIZATION
 * Scope information for a tool call
interface ToolScopeInfo {
    /** The scope path (e.g., 'action:execute', 'entity:read') */
    scopePath: string;
    /** The specific resource being accessed (e.g., action name, entity name) */
    resource: string;
 * Authorizes a tool call based on the session's authentication method.
 * For API key auth: Uses the two-level scope evaluation (application ceiling + key scopes).
 * For OAuth auth: Uses scope-based authorization from JWT token with hierarchical matching.
 * For no auth: Allows the call (authentication is disabled).
 * @param sessionContext - The authenticated session context
 * @param scopeInfo - The scope information for the tool call
async function authorizeToolCall(
    sessionContext: MCPSessionContext,
    scopeInfo: ToolScopeInfo
    // Handle different authentication methods
    switch (sessionContext.authMethod) {
        case 'oauth':
            // OAuth authentication uses scope-based authorization from the JWT token.
            // The user consented to specific scopes during the OAuth flow.
            return authorizeOAuthToolCall(sessionContext, scopeInfo);
        case 'none':
            // No authentication configured - allow all tool calls
        case 'apiKey':
            // API key authentication uses the API Key Engine for scope-based authorization
            return authorizeApiKeyToolCall(sessionContext, scopeInfo);
            // Unknown auth method - deny by default
            return { allowed: false, error: `Unknown authentication method: ${sessionContext.authMethod}` };
 * Authorizes an API key-authenticated tool call against the key's scope permissions.
 * @param sessionContext - The authenticated session context (must have apiKeyHash)
async function authorizeApiKeyToolCall(
    if (!sessionContext.apiKeyHash) {
        return { allowed: false, error: 'API key hash not available for authorization' };
            sessionContext.apiKeyHash,
            'MCPServer',
            scopeInfo.scopePath,
            scopeInfo.resource,
                endpoint: '/mcp',
                operation: scopeInfo.scopePath,
            console.log(`[Auth] Tool authorization denied: ${result.Reason}`);
        console.error('[Auth] Authorization error:', error);
 * Checks if a granted scope matches a required scope using hierarchical matching.
 * A parent scope grants access to all child scopes.
 * - "entity" grants access to "entity:read", "entity:write", etc.
 * - "entity:read" grants access only to "entity:read" (exact match)
 * - "admin" grants access to "admin:users", "admin:settings", etc.
 * @param grantedScope - A scope the user has been granted
 * @param requiredScope - The scope required for the operation
 * @returns true if the granted scope satisfies the requirement
function scopeMatchesHierarchically(grantedScope: string, requiredScope: string): boolean {
    if (grantedScope === requiredScope) {
    // Hierarchical match: granted scope is a parent of required scope
    // e.g., "entity" matches "entity:read" or "entity:read:all"
    if (requiredScope.startsWith(grantedScope + ':')) {
 * Checks if a user's granted scopes satisfy a required scope.
 * Supports hierarchical scope matching where parent scopes grant child permissions.
 * @param grantedScopes - Array of scopes the user has been granted
 * @returns true if any granted scope satisfies the requirement
function hasRequiredScope(grantedScopes: string[], requiredScope: string): boolean {
    return grantedScopes.some((granted) => scopeMatchesHierarchically(granted, requiredScope));
 * Authorizes an OAuth-authenticated tool call against the token's scope permissions.
 * Uses hierarchical scope matching where parent scopes grant access to child scopes.
 * @param sessionContext - The authenticated session context (must have scopes)
function authorizeOAuthToolCall(
): { allowed: boolean; error?: string } {
    const grantedScopes = sessionContext.scopes ?? [];
    // If no scopes granted, deny access (user didn't consent to any scopes)
    if (grantedScopes.length === 0) {
        console.log(`[Auth] OAuth user ${sessionContext.oauth?.email} denied for ${scopeInfo.scopePath}: no scopes granted`);
            error: `Access denied: no scopes granted. Required scope: ${scopeInfo.scopePath}`,
    // Check if any granted scope satisfies the required scope
    if (hasRequiredScope(grantedScopes, scopeInfo.scopePath)) {
        console.log(`[Auth] OAuth user ${sessionContext.oauth?.email} authorized for ${scopeInfo.scopePath}`);
    // Scope not granted
        `[Auth] OAuth user ${sessionContext.oauth?.email} denied for ${scopeInfo.scopePath}: ` +
            `granted scopes [${grantedScopes.join(', ')}] do not include required scope`
        error: `Access denied: scope '${scopeInfo.scopePath}' not granted. ` +
            `Your granted scopes: [${grantedScopes.join(', ')}]`,
 * TOOL FILTERING
 * Checks if a tool name matches a glob-style pattern.
 * Supported patterns:
 * - `*` - Matches all tools
 * - `prefix*` - Matches tools starting with "prefix"
 * - `*suffix` - Matches tools ending with "suffix"
 * - `*contains*` - Matches tools containing "contains"
 * - `exact` - Exact match (case-insensitive)
 * @param toolName - The tool name to test
 * @param pattern - The glob-style pattern to match against
 * @returns True if the tool name matches the pattern
 * matchesPattern("Get_Users_Record", "Get_*") // true
 * matchesPattern("Get_Users_Record", "*_Record") // true
 * matchesPattern("Get_Users_Record", "*Users*") // true
function matchesPattern(toolName: string, pattern: string): boolean {
    const lowerName = toolName.toLowerCase();
    const lowerPattern = pattern.trim().toLowerCase();
    if (lowerPattern === '*') {
    const startsWithWildcard = lowerPattern.startsWith('*');
    const endsWithWildcard = lowerPattern.endsWith('*');
    if (startsWithWildcard && endsWithWildcard) {
        // *contains*
        const searchTerm = lowerPattern.slice(1, -1);
        return lowerName.includes(searchTerm);
    } else if (startsWithWildcard) {
        // *suffix
        const suffix = lowerPattern.slice(1);
        return lowerName.endsWith(suffix);
    } else if (endsWithWildcard) {
        // prefix*
        const prefix = lowerPattern.slice(0, -1);
        return lowerName.startsWith(prefix);
        // exact match
        return lowerName === lowerPattern;
 * Determines if a tool should be included based on the current filter options.
 * Filter logic:
 * 1. If includePatterns are specified, tool must match at least one pattern
 * 2. If excludePatterns are specified, tool must not match any pattern
 * 3. If no patterns are specified, all tools are included
 * @param toolName - The name of the tool to check
 * @param filterOptions - The filter configuration with include/exclude patterns
 * @returns True if the tool should be included, false if it should be filtered out
function shouldIncludeTool(toolName: string, filterOptions: ToolFilterOptions): boolean {
    const { includePatterns, excludePatterns } = filterOptions;
    // If include patterns are specified, tool must match at least one
    if (includePatterns && includePatterns.length > 0) {
        const matchesInclude = includePatterns.some(pattern => matchesPattern(toolName, pattern));
        if (!matchesInclude) {
    // If exclude patterns are specified, tool must not match any
    if (excludePatterns && excludePatterns.length > 0) {
        const matchesExclude = excludePatterns.some(pattern => matchesPattern(toolName, pattern));
        if (matchesExclude) {
 * Performs smart text truncation that preserves both the beginning and end of content.
 * This is useful for debugging large I/O data where both the start and end are important.
 * The truncation preserves 70% from the start and 30% from the end, with a clear
 * indicator of how many characters were removed.
 * @param text - The text to truncate (null/undefined returns empty string)
 * @param maxChars - Maximum characters to keep (0 = no truncation)
 * @returns Object with the truncated value and a flag indicating if truncation occurred
 * truncateText("Hello World", 5) // { value: "Hel...[4 chars]...ld", truncated: true }
 * truncateText("Hi", 100) // { value: "Hi", truncated: false }
function truncateText(text: string | null | undefined, maxChars: number): { value: string; truncated: boolean } {
    if (!text) {
        return { value: '', truncated: false };
    if (maxChars === 0 || text.length <= maxChars) {
        return { value: text, truncated: false };
    // Keep 70% from start, 30% from end
    const startChars = Math.floor(maxChars * 0.7);
    const endChars = maxChars - startChars;
    const truncatedCount = text.length - startChars - endChars;
    const truncated = text.substring(0, startChars) +
        `\n\n[... ${truncatedCount} characters truncated ...]\n\n` +
        text.substring(text.length - endChars);
    return { value: truncated, truncated: true };
 * TOOL REGISTRATION HELPERS
 * Helper type for tool configuration used during registration
interface ToolConfig {
    parameters: z.ZodObject<z.ZodRawShape>;
    execute: (props: Record<string, unknown>, sessionContext: MCPSessionContext) => Promise<string>;
     * Scope information for authorization. Can be:
     * - Static object: { scopePath: 'action:execute', resource: 'MyAction' }
     * - Function: (props) => ({ scopePath: 'entity:read', resource: props.entityName })
     * If not provided, no authorization check is performed.
    scopeInfo?: ToolScopeInfo | ((props: Record<string, unknown>) => ToolScopeInfo);
 * Registers all tools with the MCP server for a specific authenticated session.
 * This function is called per-connection to create user-scoped tool handlers.
 * @param server - The McpServer instance to register tools on
 * @param sessionContext - The authenticated session context with user info
 * @param systemUser - System user for server initialization tasks
async function registerAllTools(
    server: McpServer,
    systemUser: UserInfo
    // Helper to register a tool with filter check and authorization
    const addToolWithFilter = (config: ToolConfig): void => {
        registeredToolNames.push(config.name);
        if (!shouldIncludeTool(config.name, activeFilterOptions)) {
        // Use registerTool with explicit type assertions to avoid infinite type inference
        (server as any).registerTool(
            config.name,
                description: config.description,
                inputSchema: config.parameters.shape
            async (params: Record<string, unknown>) => {
                // Check authorization if scopeInfo is provided
                if (config.scopeInfo) {
                    const scopeInfo = typeof config.scopeInfo === 'function'
                        ? config.scopeInfo(params)
                        : config.scopeInfo;
                    const authResult = await authorizeToolCall(sessionContext, scopeInfo);
                            content: [{ type: "text" as const, text: JSON.stringify({
                                error: 'Authorization denied',
                                reason: authResult.error,
                                scope: scopeInfo.scopePath,
                                resource: scopeInfo.resource
                            }) }]
                const result = await config.execute(params, sessionContext);
                    content: [{ type: "text" as const, text: result }]
    // Register Get_Entity_List tool - ultra-lightweight list of all entities
    addToolWithFilter({
        name: "Get_Entity_List",
        description: "Retrieves a list of all entity names. Use Get_Single_Entity(entityName) to get full details including description, fields, and relationships for a specific entity.",
        parameters: z.object({}),
        scopeInfo: { scopePath: 'entity:read', resource: '*' },
        async execute() {
            // Just return entity names - minimal payload
            const entityNames = md.Entities.map(e => e.Name);
            return JSON.stringify(entityNames);
    // Register Get_Single_Entity tool - full details for one entity
        name: "Get_Single_Entity",
        description: "Retrieves complete details for a single entity including all fields, relationships, and metadata. Use Get_Entity_List first to find entity names.",
        parameters: z.object({
            entityName: z.string().describe("The exact name of the entity to retrieve (e.g., 'Users', 'MJ: AI Models')")
        scopeInfo: (props) => ({ scopePath: 'entity:read', resource: props.entityName as string || '*' }),
        async execute(params: Record<string, unknown>) {
            const entityName = params.entityName as string;
            const entity = md.Entities.find(e =>
                e.Name.toLowerCase() === entityName.toLowerCase()
                return JSON.stringify({
                    error: `Entity '${entityName}' not found`,
                    suggestion: "Use Get_Entity_List to see available entity names"
            return JSON.stringify(entity, null, 2);
    // Load all tool categories
    await loadEntityTools(addToolWithFilter);
    await loadActionTools(addToolWithFilter, systemUser, sessionContext);
    await loadAgentTools(addToolWithFilter, systemUser, sessionContext);
    loadAgentRunDiagnosticTools(addToolWithFilter, sessionContext);
    loadQueryTools(addToolWithFilter, sessionContext);
    await loadPromptTools(addToolWithFilter, systemUser, sessionContext);
    loadCommunicationTools(addToolWithFilter, sessionContext);
 * SERVER INITIALIZATION
 * Initializes and starts the MemberJunction MCP server.
 * This function performs the following setup:
 * 1. Establishes database connection using configured credentials
 * 2. Sets up SQL Server client and MemberJunction metadata
 * 3. Loads API keys into the CredentialEngine cache for fast validation
 * 4. Sets up Express server with SSE transport for MCP protocol
 * 5. Creates per-connection MCP server instances with authenticated user context
 * The server uses API key authentication. Each authenticated request gets a session
 * with the user context from the API key, which is used for all tool executions.
 * @param filterOptions - Optional tool filtering configuration to limit which tools are exposed
 * @throws Error if MCP server is disabled in configuration or database connection fails
 * // Start with all tools
 * await initializeServer();
 * // Start with filtered tools
 * await initializeServer({
 *   includePatterns: ['Get_*', 'Run_Agent'],
 *   excludePatterns: ['*_AuditLog_*']
export async function initializeServer(filterOptions: ToolFilterOptions = {}): Promise<void> {
        // Initialize configuration (loads .env and mj.config.cjs)
        _config = await initConfig();
        mcpServerPort = _config.mcpServerSettings?.port || 3100;
        // Store filter options for use by tool registration
        activeFilterOptions = filterOptions;
        // Clear any previously registered tool names
        registeredToolNames.length = 0;
        if (!_config.mcpServerSettings?.enableMCPServer) {
            console.log("MCP Server is disabled in the configuration.");
            throw new Error("MCP Server is disabled in the configuration.");
        const poolConfig = buildPoolConfig();
        const sqlConfig = new SQLServerProviderConfigData(pool, _config.mjCoreSchema);
        await setupSQLServerClient(sqlConfig);
        // Use system user for server initialization tasks (loading credentials, discovering agents)
        // Note: Individual tool executions use the authenticated session user, not this system user
            throw new Error('System user not found in UserCache - required for server initialization');
        // Load credentials for server operations
        console.log('Loading credentials into cache...');
        await CredentialEngine.Instance.Config(false, systemUser);
        console.log('Credentials loaded successfully.');
        // Initialize API Key engine for scope-based authorization
        console.log('Initializing API Key engine...');
        await apiKeyEngine.Config(false, systemUser);
        console.log(`API Key engine initialized. Scopes loaded: ${apiKeyEngine.Scopes.length}`);
        // Initialize AuthGate configuration for unified authentication
        authGateConfig = {
            validateApiKey: validateApiKey,
            getSystemUser: () => UserCache.Instance.GetSystemUser(),
        // Validate OAuth configuration if OAuth is enabled
        const authMode = getAuthMode();
        let effectiveAuthMode = authMode;
        let configuredProviderNames: string[] = [];
        if (authMode === 'oauth' || authMode === 'both') {
            // Check if auth providers are configured
            const providersConfigured = await hasAuthProviders();
            const validationResult = validateOAuthConfig(providersConfigured);
            if (validationResult.errors.length > 0) {
                console.error(`[Auth] OAuth configuration errors: ${validationResult.errors.join('; ')}`);
                if (authMode === 'oauth') {
                    throw new Error(`OAuth configuration invalid: ${validationResult.errors.join('; ')}`);
            if (validationResult.warnings.length > 0) {
                for (const warning of validationResult.warnings) {
                    console.warn(`[Auth] ${warning}`);
            effectiveAuthMode = validationResult.effectiveMode;
            // Get provider names for logging
            if (providersConfigured) {
                    const { AuthProviderFactory } = await import('@memberjunction/server');
                    const factory = AuthProviderFactory.getInstance();
                    configuredProviderNames = factory.getAllProviders().map((p: { name: string }) => p.name);
                    // Ignore errors getting provider names - just for logging
        // Log auth configuration
        logAuthConfig(effectiveAuthMode, configuredProviderNames);
        // Create Express app for SSE transport
        // CORS must be FIRST - browser preflight OPTIONS requests need immediate handling
        // before any other middleware touches the request
        app.use((req: Request, res: Response, next: NextFunction) => {
            res.header('Access-Control-Allow-Origin', '*');
            res.header('Access-Control-Allow-Methods', 'GET, POST, OPTIONS');
            res.header('Access-Control-Allow-Headers', 'Content-Type, Authorization, Accept, x-api-key, x-mj-api-key');
            res.header('Access-Control-Expose-Headers', 'Content-Type');
            // Handle preflight requests immediately - don't let other middleware process them
            if (req.method === 'OPTIONS') {
                res.sendStatus(200);
        // Enable JSON parsing for POST requests EXCEPT MCP endpoints
        // Both SSE and Streamable HTTP transports handle their own body parsing
            if (req.path === '/mcp/messages' || req.path === '/mcp') {
                // Skip JSON parsing - the MCP transports handle it
                express.json()(req, res, next);
        // =====================================================================
        // OAUTH PROXY AUTHORIZATION SERVER (RFC 7591, RFC 8414)
        // When enabled, the MCP Server acts as an OAuth Authorization Server
        // that supports dynamic client registration and proxies auth to upstream.
        const oauthProxyEnabled = _config.mcpServerSettings?.auth?.proxy?.enabled ?? false;
        let oauthProxyBaseUrl: string | undefined;
        if (isOAuthEnabled() && oauthProxyEnabled) {
                // Get the upstream provider configuration
                const providers = factory.getAllProviders();
                if (providers.length === 0) {
                    console.warn('[OAuth Proxy] No auth providers configured - OAuth proxy disabled');
                    // Find the upstream provider (use configured name or first available)
                    const upstreamProviderName = _config.mcpServerSettings?.auth?.proxy?.upstreamProvider;
                    const upstreamProvider = upstreamProviderName
                        ? providers.find((p: { name: string }) => p.name === upstreamProviderName)
                        : providers[0];
                    if (!upstreamProvider) {
                        console.error(`[OAuth Proxy] Upstream provider '${upstreamProviderName}' not found`);
                        // Build OAuth proxy configuration
                        oauthProxyBaseUrl = getResourceIdentifier();
                        // Detect Azure AD v2.0 endpoints from issuer
                        // Azure AD issuer: https://login.microsoftonline.com/{tenant}/v2.0
                        // Cast to access provider properties (IAuthProvider interface)
                        const provider = upstreamProvider as {
                            audience: string;
                        const issuer = provider.issuer;
                        const isAzureAD = issuer?.includes('microsoftonline.com') || issuer?.includes('sts.windows.net');
                        let authorizationEndpoint: string;
                        let tokenEndpoint: string;
                        if (isAzureAD) {
                            // Azure AD v2.0 endpoints
                            const baseUrl = issuer.replace(/\/v2\.0\/?$/, '');
                            authorizationEndpoint = `${baseUrl}/oauth2/v2.0/authorize`;
                            tokenEndpoint = `${baseUrl}/oauth2/v2.0/token`;
                            // Generic OIDC - assume standard paths (Auth0, Okta, etc.)
                            // Most providers use /.well-known/openid-configuration but we need direct endpoints
                            // Strip trailing slash from issuer to avoid double slashes in URL
                            const issuerBase = issuer.replace(/\/+$/, '');
                            authorizationEndpoint = `${issuerBase}/authorize`;
                            tokenEndpoint = `${issuerBase}/oauth/token`;
                        // Build scopes for upstream - use standard OIDC scopes only
                        // Note: We don't include api://.../.default because that would cause
                        // AADSTS90009 "app requesting token for itself" when using a single app registration.
                        // The OAuth proxy only needs to authenticate the user, not access an API resource.
                        const upstreamScopes: string[] = ['openid', 'profile', 'email'];
                        if (!isAzureAD) {
                            // For non-Azure providers, we might need additional scopes
                            // (Azure AD doesn't need offline_access for refresh tokens in v2.0)
                            upstreamScopes.push('offline_access');
                        // For OAuth proxy, we need client ID from environment or config
                        // Priority: provider.clientId → WEB_CLIENT_ID env var → audience (Azure AD uses audience as clientId)
                        // NOTE: Provider-specific clientId takes priority because WEB_CLIENT_ID may be for a different provider
                        // Client secret is OPTIONAL - the proxy uses PKCE for upstream token exchange
                        // (same technique as MJExplorer SPA), so no secret is required
                        const upstreamClientId = provider.clientId || process.env.WEB_CLIENT_ID || provider.audience || '';
                        const upstreamClientSecret = process.env.WEB_CLIENT_SECRET || undefined;
                        // Log warning if no valid client_id found
                        if (!upstreamClientId) {
                            console.error('[OAuth Proxy] ERROR: No upstream client_id configured!');
                            console.error('[OAuth Proxy] Set WEB_CLIENT_ID env var or add clientId to your auth provider config');
                            console.log(`[OAuth Proxy] Upstream client_id: ${upstreamClientId}`);
                        // Note: clientSecret is optional - proxy uses PKCE for upstream auth
                        if (upstreamClientSecret) {
                            console.log('[OAuth Proxy] Using client secret for upstream authentication');
                            console.log('[OAuth Proxy] Using PKCE-only for upstream authentication (no client secret)');
                        // Build JWT config from proxy settings
                        // Falls back to MCP_JWT_SECRET env var if not configured in mj.config.cjs
                        const proxySettings = _config.mcpServerSettings?.auth?.proxy;
                        const jwtSigningSecret = proxySettings?.jwtSigningSecret ?? process.env.MCP_JWT_SECRET;
                        const jwtConfig = jwtSigningSecret ? {
                            signingSecret: jwtSigningSecret,
                            expiresIn: proxySettings?.jwtExpiresIn ?? '1h',
                            issuer: proxySettings?.jwtIssuer ?? 'urn:mj:mcp-server',
                        const proxyConfig: OAuthProxyConfig = {
                            baseUrl: oauthProxyBaseUrl,
                            upstream: {
                                authorizationEndpoint,
                                clientId: upstreamClientId,
                                clientSecret: upstreamClientSecret,
                                scopes: upstreamScopes,
                                providerName: provider.name,
                            enableDynamicRegistration: true,
                            stateTtlMs: proxySettings?.stateTtlMs,
                            jwt: jwtConfig,
                            enableConsentScreen: proxySettings?.enableConsentScreen ?? false,
                        // Create and mount OAuth proxy router
                        const oauthProxyRouter = createOAuthProxyRouter(proxyConfig);
                        app.use(oauthProxyRouter);
                        console.log(`[OAuth Proxy] Enabled with upstream provider: ${provider.name}`);
                        console.log(`[OAuth Proxy] Authorization endpoint: ${authorizationEndpoint}`);
                        console.log(`[OAuth Proxy] Token endpoint: ${tokenEndpoint}`);
                        console.log(`[OAuth Proxy] Base URL: ${oauthProxyBaseUrl}`);
                        if (jwtConfig) {
                            console.log(`[OAuth Proxy] JWT signing enabled (issuer: ${jwtConfig.issuer})`);
                            // Configure TokenValidator to accept proxy-signed JWTs
                            setProxyTokenConfig({
                                signingSecret: jwtConfig.signingSecret,
                                issuer: jwtConfig.issuer,
                                audience: oauthProxyBaseUrl,
                            console.log('[OAuth Proxy] JWT signing disabled - passing through upstream tokens');
                console.error('[OAuth Proxy] Failed to initialize:', error);
        // PROTECTED RESOURCE METADATA ENDPOINT (RFC 9728)
        // This endpoint allows MCP clients to discover how to authenticate.
        // It's required by the MCP Authorization specification when OAuth is enabled.
        if (isOAuthEnabled()) {
            app.get('/.well-known/oauth-protected-resource', async (_req: Request, res: Response) => {
                    // Dynamically import AuthProviderFactory to get configured providers
                    // Extract issuer URLs from configured auth providers
                    const authorizationServers = extractAuthorizationServers(
                        providers.map((p: { issuer: string }) => ({ issuer: p.issuer }))
                    if (authorizationServers.length === 0 && !oauthProxyBaseUrl) {
                        console.warn('[OAuth] No authorization servers configured - metadata endpoint returning empty list');
                    const metadata: ProtectedResourceMetadata = buildProtectedResourceMetadata({
                        authorizationServers,
                        resourceName: 'MemberJunction MCP Server',
                        providers, // Pass providers for automatic scope generation
                        // When OAuth proxy is enabled, point to ourselves as the authorization server
                        useOAuthProxy: oauthProxyEnabled && !!oauthProxyBaseUrl,
                        oauthProxyBaseUrl,
                    res.json(metadata);
                    console.error('[OAuth] Error building Protected Resource Metadata:', error);
                        error: 'internal_error',
                        message: 'Failed to generate protected resource metadata',
            console.log('[OAuth] Protected Resource Metadata endpoint registered at /.well-known/oauth-protected-resource');
        // SSE endpoint for establishing MCP connections
        app.get('/mcp/sse', async (req: Request, res: Response) => {
            // Authenticate the request with full result for proper error handling
            const authResult = await authenticateRequestWithResult(req);
            if (!authResult.authenticated) {
                sendAuthErrorResponse(res, authResult);
            const sessionContext = toSessionContext(authResult);
                // Create a new MCP server for this connection
                const mcpServer = new McpServer({
                    name: "MemberJunction",
                    version: "1.0.0"
                // Register all tools with user-scoped context
                await registerAllTools(mcpServer, sessionContext, systemUser);
                // Create SSE transport for this connection
                const transport = new SSEServerTransport('/mcp/messages', res);
                // Store transport for message routing
                const sessionId = transport.sessionId;
                transports.set(sessionId, transport);
                console.log(`[SSE] New session for ${sessionContext.user.Email}: ${sessionId}`);
                // Set up keepalive ping to prevent connection timeout (every 15 seconds)
                const keepaliveInterval = setInterval(() => {
                    if (!res.writableEnded) {
                        // SSE comment line (starts with colon) - keeps connection alive
                        res.write(':ping\n\n');
                }, 15000);
                // Clean up on connection close
                    clearInterval(keepaliveInterval);
                    transports.delete(sessionId);
                res.on('error', (err: NodeJS.ErrnoException) => {
                    // ECONNRESET is normal when client disconnects - don't log as error
                    if (err.code !== 'ECONNRESET') {
                        console.error(`[SSE] Connection error for session ${sessionId}:`, err);
                req.on('error', (err: NodeJS.ErrnoException) => {
                        console.error(`[SSE] Request error for session ${sessionId}:`, err);
                // Connect the server to the transport
                await mcpServer.connect(transport);
                console.error('[SSE] Connection error:', error);
                if (!res.headersSent) {
                        message: error instanceof Error ? error.message : 'Connection failed'
        // Message endpoint for receiving MCP messages from clients
        // Note: Authentication happens when establishing the SSE connection (/mcp/sse).
        // This endpoint routes messages to existing authenticated sessions.
        // The sessionId acts as a session token - clients must first authenticate via SSE.
        app.post('/mcp/messages', async (req: Request, res: Response) => {
            const sessionId = req.query.sessionId as string;
            if (!sessionId) {
                // Protocol error - client didn't follow MCP protocol
                    error: 'invalid_request',
                    message: 'Missing sessionId query parameter. Establish a session via /mcp/sse first.'
            const transport = transports.get(sessionId);
            if (!transport) {
                // Session expired or invalid - client needs to re-authenticate
                // When OAuth is enabled, include WWW-Authenticate header to guide client
                    send401Response(res, 'Session expired or invalid. Please re-authenticate via /mcp/sse endpoint.', {
                        errorDescription: 'Session not found - establish a new authenticated session'
                        error: 'session_expired',
                        message: 'Session not found. Please re-establish connection via /mcp/sse endpoint.'
            // Buffer the body then pass to transport
            const chunks: Buffer[] = [];
            for await (const chunk of req) {
                chunks.push(Buffer.isBuffer(chunk) ? chunk : Buffer.from(chunk as string));
            const bodyBuffer = Buffer.concat(chunks as unknown as Uint8Array[]);
            // Create a new readable stream from the buffered body for the transport
            const { Readable } = await import('stream');
            const bodyStream = new Readable({
                read() {
                    this.push(bodyBuffer);
                    this.push(null);
            // Create a mock request with the buffered body stream
            const mockReq = Object.assign(bodyStream, {
                headers: req.headers,
                method: req.method,
                url: req.url,
                query: req.query
                await transport.handlePostMessage(mockReq as unknown as Request, res);
                console.error('[Messages] Error handling message:', error);
                        error: error instanceof Error ? error.message : 'Internal server error'
        // Health check endpoint
        app.get('/health', (_req: Request, res: Response) => {
            res.json({ status: 'ok', timestamp: new Date().toISOString() });
        // STREAMABLE HTTP TRANSPORT (newer MCP protocol - single endpoint)
        // Store for Streamable HTTP transports (session ID -> transport)
        const streamableTransports = new Map<string, StreamableHTTPServerTransport>();
        // Streamable HTTP endpoint - handles both GET (SSE stream) and POST (messages)
        // This is the newer, recommended MCP transport
        app.all('/mcp', async (req: Request, res: Response) => {
            // Get or validate session ID from header
            const sessionId = req.headers['mcp-session-id'] as string | undefined;
            // For existing sessions, route to the existing transport
            if (sessionId && streamableTransports.has(sessionId)) {
                const transport = streamableTransports.get(sessionId)!;
                    // Pass parsed body to handleRequest - required for SDK to properly parse the message
                    await transport.handleRequest(req, res, req.body);
                    console.error(`[StreamableHTTP] Error on session ${sessionId}:`, error);
                        res.status(500).json({ error: 'Internal server error' });
            // For new sessions (no session ID or unknown session ID with initialization request)
            // We need to authenticate and create a new transport
                // Create a new MCP server for this session
                // Create Streamable HTTP transport with session ID generation
                const transport = new StreamableHTTPServerTransport({
                    sessionIdGenerator: () => randomUUID()
                // Store for future requests (after first request establishes session)
                transport.onclose = () => {
                    const sid = transport.sessionId;
                    if (sid) {
                        streamableTransports.delete(sid);
                // Handle the current request
                // Pass the parsed body to help the SDK recognize the initialize request
                // Store the transport after successful request handling
                const newSessionId = transport.sessionId;
                if (newSessionId) {
                    streamableTransports.set(newSessionId, transport);
                    console.log(`[StreamableHTTP] New session for ${sessionContext.user.Email}: ${newSessionId}`);
                console.error('[StreamableHTTP] Connection error:', error);
        // Start the Express server
        app.listen(mcpServerPort, () => {
            console.log(`MemberJunction MCP Server running on port ${mcpServerPort}`);
            console.log(`SSE endpoint: http://localhost:${mcpServerPort}/mcp/sse`);
            console.log(`Messages endpoint: http://localhost:${mcpServerPort}/mcp/messages`);
            console.log(`Streamable HTTP endpoint: http://localhost:${mcpServerPort}/mcp`);
            // Log OAuth-specific endpoints if enabled
                console.log(`Protected Resource Metadata: http://localhost:${mcpServerPort}/.well-known/oauth-protected-resource`);
        console.error("Failed to initialize MCP server:", error);
 * TOOL LOADERS
type AddToolFn = (config: ToolConfig) => void;
 * Loads and registers action tools based on configuration.
 * Creates the following tools based on configuration:
 * - `Discover_Actions` - Lists available actions matching a pattern
 * - `Run_Action` - General tool to execute any action by name/ID
 * - `Execute_[ActionName]_Action` - Specific tools for each configured action
 * @param addToolWithFilter - Function to register tools with filter check
 * @param systemUser - System user for context when discovering and configuring actions
async function loadActionTools(
    addToolWithFilter: AddToolFn,
    systemUser: UserInfo,
    sessionContext: MCPSessionContext
    const actionTools = _config.mcpServerSettings?.actionTools;
    if (actionTools && actionTools.length > 0) {
        // Ensure ActionEngine is configured
        await actionEngine.Config(false, systemUser);
        // Add discovery tool if any action tool has discover enabled
        const hasDiscovery = actionTools.some((tool: MCPServerActionToolInfo) => tool.discover);
        if (hasDiscovery) {
                name: "Discover_Actions",
                description: "List available Actions based on a name pattern and/or category (* for all)",
                    pattern: z.string().optional().describe("Name pattern to match actions (supports wildcards: *, *Action, Action*, *Action*)"),
                    category: z.string().optional().describe("Category name to filter actions")
                scopeInfo: { scopePath: 'action:read', resource: '*' },
                async execute(props) {
                    const sessionUser = sessionContext.user;
                    const actions = await discoverActions(props.pattern as string || '*', props.category as string | undefined, sessionUser);
                        category: action.Category,
                        categoryId: action.CategoryID,
                        type: action.Type,
                        status: action.Status,
                        paramCount: actionEngine.ActionParams.filter((p: MJActionParamEntity) => p.ActionID === action.ID).length
                    })));
        // Add general action execution tool if any tool has execute enabled
        const hasExecute = actionTools.some((tool: MCPServerActionToolInfo) => tool.execute);
        if (hasExecute) {
                name: "Run_Action",
                description: "Execute any Action by name or ID with the specified parameters",
                    actionName: z.string().optional().describe("Name of the action to execute"),
                    actionId: z.string().optional().describe("ID of the action to execute"),
                    params: z.record(z.unknown()).optional().describe("Parameters for the action as key-value pairs")
                scopeInfo: (props) => ({
                    scopePath: 'action:execute',
                    resource: (props.actionName as string) || (props.actionId as string) || '*'
                        await actionEngine.Config(false, sessionUser);
                        let action: ActionEntityExtended | null = null;
                        if (props.actionId) {
                            action = actionEngine.Actions.find((a: ActionEntityExtended) => a.ID === props.actionId) || null;
                                    error: `Action not found with ID: ${props.actionId}`
                        } else if (props.actionName) {
                            action = actionEngine.Actions.find((a: ActionEntityExtended) => a.Name?.toLowerCase() === (props.actionName as string)?.toLowerCase()) || null;
                                    error: `Action not found with name: ${props.actionName}`
                                error: "Either actionName or actionId must be provided"
                        // Build action params
                        const actionParams = actionEngine.ActionParams.filter((p: MJActionParamEntity) => p.ActionID === action!.ID);
                        const paramsRecord = props.params as Record<string, unknown> | undefined;
                        const runParams: RunActionParams = {
                            ContextUser: sessionUser,
                            Params: actionParams.map((p: MJActionParamEntity) => ({
                                Name: p.Name,
                                Value: paramsRecord?.[p.Name] ?? p.DefaultValue,
                                Type: (p.Type as 'Input' | 'Output' | 'Both') || 'Input'
                        const result = await actionEngine.RunAction(runParams);
                            resultCode: result.Result?.ResultCode,
                            message: result.Message,
                            runId: result.LogEntry?.ID
            // Add Get_Action_Params tool to help discover action parameters
                name: "Get_Action_Params",
                description: "Get the parameter definitions for a specific action",
                    actionName: z.string().optional().describe("Name of the action"),
                    actionId: z.string().optional().describe("ID of the action")
                scopeInfo: (props) => ({ scopePath: 'action:read', resource: (props.actionName as string) || (props.actionId as string) || '*' }),
                        return JSON.stringify({ error: "Action not found" });
                    const params = actionEngine.ActionParams.filter((p: MJActionParamEntity) => p.ActionID === action!.ID);
                        actionName: action.Name,
                        description: action.Description,
                        params: params.map((p: MJActionParamEntity) => ({
                            name: p.Name,
                            description: p.Description,
                            type: p.Type,
                            isRequired: p.IsRequired,
                            defaultValue: p.DefaultValue
        // Process each action tool configuration for specific action tools
        for (const tool of actionTools) {
            if (tool.execute) {
                const actionPattern = tool.actionName || '*';
                const actions = await discoverActions(actionPattern, tool.actionCategory, systemUser);
                // Add specific execution tools for each matching action
                    addActionExecuteTool(addToolWithFilter, action, sessionContext);
 * Discovers actions matching a given name pattern and optional category.
 * @param pattern - The name pattern to match (supports wildcards)
 * @param category - Optional category name to filter actions
 * @param userContext - User context for ActionEngine configuration
 * @returns Array of matching action entities
async function discoverActions(pattern: string, category: string | undefined, userContext: UserInfo): Promise<ActionEntityExtended[]> {
    await actionEngine.Config(false, userContext);
    let actions = actionEngine.Actions.filter((a: ActionEntityExtended) => a.Status === 'Active');
    // Filter by category if specified
    if (category && category !== '*') {
        const categoryLower = category.toLowerCase();
        actions = actions.filter((a: ActionEntityExtended) => a.Category?.toLowerCase().includes(categoryLower));
    // Filter by pattern
        return actions;
        return actions.filter((a: ActionEntityExtended) => a.Name === pattern);
        .replace(/[.+?^${}()|[\]\\]/g, '\\$&') // Escape special chars except *
    return actions.filter((a: ActionEntityExtended) => a.Name && regex.test(a.Name));
 * Creates and registers an execution tool for a specific Action.
 * @param action - The action entity to create an execution tool for
function addActionExecuteTool(
    action: ActionEntityExtended,
    const actionParams = actionEngine.ActionParams.filter((p: MJActionParamEntity) => p.ActionID === action.ID);
    // Build Zod schema for action parameters
    const paramSchema: Record<string, z.ZodTypeAny> = {};
        let zodType: z.ZodTypeAny;
        switch (param.Type?.toLowerCase()) {
            case 'decimal':
            case 'float':
                zodType = z.number();
                zodType = z.boolean();
                zodType = z.record(z.unknown());
                zodType = z.array(z.unknown());
                zodType = z.string();
        if (!param.IsRequired) {
            zodType = zodType.optional();
        paramSchema[param.Name] = zodType.describe(param.Description || param.Name);
    const safeName = (action.Name || 'Unknown').replace(/\s+/g, '_').replace(/[^a-zA-Z0-9_]/g, '');
        name: `Execute_${safeName}_Action`,
        description: `Execute the ${action.Name || 'Unknown'} action. ${action.Description || ''}`,
        parameters: z.object(paramSchema),
        scopeInfo: { scopePath: 'action:execute', resource: action.Name || '*' },
                        Value: props[p.Name] ?? p.DefaultValue,
                const result = await ActionEngineServer.Instance.RunAction(runParams);
 * Loads and registers agent tools based on configuration.
 * - `Discover_Agents` - Lists available agents matching a pattern
 * - `Run_Agent` - General tool to execute any agent by name/ID
 * - `Execute_[AgentName]_Agent` - Specific tools for each configured agent
 * - `Get_Agent_Run_Status` - Check status of agent executions
 * - `Cancel_Agent_Run` - Cancel running agent executions
 * @param systemUser - System user for context when discovering and configuring agents
async function loadAgentTools(
    const agentTools = _config.mcpServerSettings?.agentTools;
    if (agentTools && agentTools.length > 0) {
        await aiEngine.Config(false, systemUser);
        // Add discovery tool if any agent tool has discover enabled
        const hasDiscovery = agentTools.some((tool: MCPServerAgentToolInfo) => tool.discover);
                name: "Discover_Agents",
                description: "List available AI agents based on a name pattern (* for all agents)",
                    pattern: z.string().describe("Name pattern to match agents (supports wildcards: *, *Agent, Agent*, *Agent*)")
                scopeInfo: { scopePath: 'agent:read', resource: '*' },
                    const agents = await discoverAgents(props.pattern as string, sessionUser);
                    return JSON.stringify(agents.map(agent => ({
        // Add general agent execution tool if any tool has execute enabled
        const hasExecute = agentTools.some(tool => tool.execute);
                name: "Run_Agent",
                description: "Execute any AI agent by name or ID",
                    agentName: z.string().optional().describe("Name of the agent to execute"),
                    agentId: z.string().optional().describe("ID of the agent to execute"),
                    conversationHistory: z.array(z.object({
                        role: z.enum(['user', 'assistant', 'system']),
                        content: z.string()
                    })).optional().describe("Conversation history for context"),
                    data: z.record(z.unknown()).optional().describe("Template data for the agent"),
                    waitForCompletion: z.boolean().optional().default(true).describe("Wait for agent to complete before returning")
                    scopePath: 'agent:execute',
                    resource: (props.agentName as string) || (props.agentId as string) || '*'
                        await aiEngine.Config(false, sessionUser);
                        if (props.agentId) {
                            agent = aiEngine.Agents.find(a => a.ID === props.agentId) || null;
                                    error: `Agent not found with ID: ${props.agentId}`
                        } else if (props.agentName) {
                            agent = aiEngine.Agents.find(a => a.Name?.toLowerCase() === (props.agentName as string).toLowerCase()) || null;
                                    error: `Agent not found with name: ${props.agentName}`
                                error: "Either agentName or agentId must be provided"
                        // Convert conversation history to ChatMessage format
                        const historyArray = props.conversationHistory as Array<{ role: string; content: string }> | undefined;
                        const messages: ChatMessage[] = historyArray?.map((msg) => ({
                            role: msg.role as 'user' | 'assistant' | 'system',
                            content: msg.content
                        })) || [];
                            contextUser: sessionUser,
                            data: props.data as Record<string, unknown>
                        if (props.waitForCompletion) {
                            // Return the full result
                                payload: result.payload || result.agentRun?.FinalPayload,
                                message: result.agentRun?.Message,
                                responseForm: result.responseForm
                            // Return just the run ID for async checking
                                message: "Agent execution started. Use Get_Agent_Run_Status to check progress."
        // Process each agent tool configuration for specific agent tools
        for (const tool of agentTools) {
            const agentPattern = tool.agentName || "*";
            const agents = await discoverAgents(agentPattern, systemUser);
            // Add tools for each matching agent
                    addAgentExecuteTool(addToolWithFilter, agent, sessionContext);
        // Add status tool if any agent tool has status enabled
        const hasStatus = agentTools.some(tool => tool.status);
        if (hasStatus) {
                name: "Get_Agent_Run_Status",
                description: "Get the status of a running or completed agent execution",
                    runId: z.string().describe("The agent run ID")
                scopeInfo: (props) => ({ scopePath: 'agent:monitor', resource: props.runId as string || '*' }),
                    const agentRun = await md.GetEntityObject<AIAgentRunEntityExtended>('MJ: AI Agent Runs', sessionUser);
                    const loaded = await agentRun.Load(props.runId as string);
                        return JSON.stringify({ error: "Run not found" });
        // Add cancel tool if any agent tool has cancel enabled
        const hasCancel = agentTools.some(tool => tool.cancel);
        if (hasCancel) {
                name: "Cancel_Agent_Run",
                description: "Cancel a running agent execution (Note: cancellation support depends on agent implementation)",
                    runId: z.string().describe("The run ID of the agent execution to cancel")
                scopeInfo: (props) => ({ scopePath: 'agent:cancel', resource: props.runId as string || '*' }),
                    // Note: Actual cancellation would require the agent to check the cancellation token
                    // For now, we can update the status to indicate cancellation was requested
                    if (!loaded || agentRun.Status !== 'Running') {
                        return JSON.stringify({ success: false, message: "Run not found or not running" });
                    // Update status to indicate cancellation requested
                    return JSON.stringify({ success: saved });
 * Registers diagnostic tools for debugging and auditing agent executions.
 * Creates the following tools:
 * - `List_Recent_Agent_Runs` - Query recent agent runs with filtering
 * - `Get_Agent_Run_Summary` - Comprehensive summary with step metadata
 * - `Get_Agent_Run_Step_Detail` - Detailed step info with truncated I/O
 * - `Get_Agent_Run_Step_Full_Data` - Export complete step data to file
 * These tools help users debug agent behavior, audit executions, and
 * troubleshoot failures without needing direct database access.
function loadAgentRunDiagnosticTools(addToolWithFilter: AddToolFn, sessionContext: MCPSessionContext): void {
    // Tool 1: List Recent Agent Runs
        name: "List_Recent_Agent_Runs",
        description: "Fast query for recent AI agent runs with optional filtering by agent name, status, and date range",
            agentName: z.string().optional().describe("Filter by agent name (partial match)"),
            status: z.enum(['Success', 'Failed', 'Running', 'Cancelled', 'all']).default('all').describe("Filter by run status"),
            days: z.number().default(7).describe("Number of days to look back"),
            limit: z.number().default(10).describe("Maximum number of runs to return")
        scopeInfo: { scopePath: 'agent:monitor', resource: '*' },
            cutoffDate.setDate(cutoffDate.getDate() - (props.days as number));
            const dateFilter = `StartedAt >= '${cutoffDate.toISOString()}'`;
            let filter = dateFilter;
            if (props.agentName) {
                filter += ` AND Agent LIKE '%${props.agentName}%'`;
            if (props.status !== 'all') {
                filter += ` AND Status = '${props.status}'`;
                MaxRows: props.limit as number,
                Fields: ['ID', 'AgentID', 'Agent', 'Status', 'StartedAt', 'CompletedAt', 'TotalTokensUsed', 'TotalCost', 'ErrorMessage']
            }, sessionUser);
                return JSON.stringify({ error: result.ErrorMessage });
            return JSON.stringify(result.Results);
    // Tool 2: Get Agent Run Summary
        name: "Get_Agent_Run_Summary",
        description: "Comprehensive summary of an agent run with step-level metadata (excludes large I/O data)",
        scopeInfo: (props) => ({ scopePath: 'agent:monitor', resource: (props as Record<string, unknown>).runId as string || '*' }),
            runId: z.string().describe("The agent run ID to summarize")
                return JSON.stringify({ error: "Agent run not found" });
            const stepsResult = await rv.RunView<AIAgentRunStepEntityExtended>({
                ExtraFilter: `AgentRunID = '${props.runId}'`,
                Fields: ['ID', 'StepNumber', 'StepName', 'StepType', 'Status', 'StartedAt', 'CompletedAt', 'ErrorMessage'],
                return JSON.stringify({ error: stepsResult.ErrorMessage });
            const errorSteps = steps.filter(s => s.ErrorMessage);
            const summary = {
                agentId: agentRun.AgentID,
                startedAt: agentRun.StartedAt?.toISOString(),
                completedAt: agentRun.CompletedAt?.toISOString(),
                duration: agentRun.CompletedAt && agentRun.StartedAt
                    ? agentRun.CompletedAt.getTime() - agentRun.StartedAt.getTime()
                totalTokens: agentRun.TotalTokensUsed,
                totalCost: agentRun.TotalCost,
                steps: steps.map(s => ({
                    stepNumber: s.StepNumber,
                    stepId: s.ID,
                    stepName: s.StepName,
                    stepType: s.StepType,
                    status: s.Status,
                    duration: s.CompletedAt && s.StartedAt
                        ? new Date(s.CompletedAt).getTime() - new Date(s.StartedAt).getTime()
                    errorMessage: s.ErrorMessage || undefined
                firstError: errorSteps.length > 0 ? {
                    stepNumber: errorSteps[0].StepNumber,
                    stepName: errorSteps[0].StepName,
                    message: errorSteps[0].ErrorMessage
            return JSON.stringify(summary);
    // Tool 3: Get Agent Run Step Detail
        name: "Get_Agent_Run_Step_Detail",
        description: "Detailed information about a specific step including input/output data with smart truncation",
            runId: z.string().describe("The agent run ID"),
            stepNumber: z.number().describe("The step number to retrieve (1-based)"),
            maxChars: z.number().default(5000).describe("Maximum characters for I/O data (0 = no truncation)")
            const stepNumber = props.stepNumber as number;
                return JSON.stringify({ error: `Invalid step number. Run has ${steps.length} steps.` });
            const step = steps[stepNumber - 1];
            const inputData = truncateText(step.InputData, props.maxChars as number);
            const outputData = truncateText(step.OutputData, props.maxChars as number);
            const detail = {
                stepNumber: step.StepNumber,
                stepId: step.ID,
                stepName: step.StepName,
                stepType: step.StepType,
                status: step.Status,
                startedAt: step.StartedAt ? new Date(step.StartedAt).toISOString() : null,
                completedAt: step.CompletedAt ? new Date(step.CompletedAt).toISOString() : null,
                duration: step.CompletedAt && step.StartedAt
                    ? new Date(step.CompletedAt).getTime() - new Date(step.StartedAt).getTime()
                    data: inputData.value,
                    truncated: inputData.truncated,
                    originalLength: step.InputData?.length || 0
                    data: outputData.value,
                    truncated: outputData.truncated,
                    originalLength: step.OutputData?.length || 0
                errorMessage: step.ErrorMessage || undefined
            return JSON.stringify(detail);
    // Tool 4: Get Agent Run Step Full Data
        name: "Get_Agent_Run_Step_Full_Data",
        description: "Export complete untruncated step data to JSON file for detailed analysis",
            outputFile: z.string().optional().describe("File path to write JSON output (optional)")
            const stepData = step.GetAll();
            // Determine output file path
            const runIdShort = (props.runId as string).substring(0, 8);
            const defaultFile = `./agent-run-${runIdShort}-step-${stepNumber}.json`;
            const filePath = path.resolve(process.cwd(), (props.outputFile as string) || defaultFile);
            // Write to file
            const jsonContent = JSON.stringify(stepData, null, 2);
            await fs.writeFile(filePath, jsonContent, 'utf-8');
            const response: Record<string, unknown> = {
                message: `Step data exported to file`,
                fileSize: jsonContent.length,
                stepSummary: {
                    inputLength: step.InputData?.length || 0,
                    outputLength: step.OutputData?.length || 0
            // Include inline data if small enough
            if (jsonContent.length < 10000) {
                response.inlineData = stepData;
                response.note = "Data included inline (file also saved)";
            return JSON.stringify(response);
 * QUERY TOOLS
 * Loads query tools based on configuration.
 * Creates tools for discovering and executing stored MJ Queries:
 * - `Discover_Queries` - List available stored queries
 * - `Run_Query` - Execute a stored query by name or ID
 * - `Get_Database_Schema` - Get schema information for available entities
function loadQueryTools(addToolWithFilter: AddToolFn, sessionContext: MCPSessionContext): void {
    const queryTools = _config.mcpServerSettings?.queryTools;
    if (queryTools?.enabled) {
        // Add query discovery tool
            name: "Discover_Queries",
            description: "List available stored queries that can be executed. Returns query metadata including name, description, and category.",
                pattern: z.string().optional().describe("Name pattern to match queries (supports wildcards: *, *Query, Query*, *Query*)"),
                category: z.string().optional().describe("Category name or path to filter queries (e.g., 'Reports', '/MJ/AI/')")
            scopeInfo: { scopePath: 'query:read', resource: '*' },
                        EntityName: 'MJ: Queries',
                        ExtraFilter: `Status = 'Active'`,
                        Fields: ['ID', 'Name', 'Description', 'CategoryID', 'Status'],
                    let queries = result.Results || [];
                    // Filter by pattern if provided
                    const pattern = (props.pattern as string) || '*';
                    if (pattern !== '*') {
                        if (pattern.includes('*')) {
                                .replace(/[.+?^${}()|[\]\\]/g, '\\$&')
                                .replace(/\*/g, '.*');
                            queries = queries.filter((q: { Name: string }) => q.Name && regex.test(q.Name));
                            queries = queries.filter((q: { Name: string }) => q.Name === pattern);
                    return JSON.stringify(queries.map((q: { ID: string; Name: string; Description?: string; CategoryID?: string; Status: string }) => ({
                        id: q.ID,
                        name: q.Name,
                        description: q.Description || '',
                        categoryId: q.CategoryID,
                        status: q.Status
        // Add stored query execution tool
            name: "Run_Query",
            description: "Execute a stored query by name or ID. Stored queries are pre-defined SQL queries managed in MemberJunction.",
                queryName: z.string().optional().describe("Name of the stored query to execute"),
                queryId: z.string().optional().describe("ID of the stored query to execute"),
                categoryPath: z.string().optional().describe("Category path for disambiguation (e.g., '/MJ/Reports/')"),
                parameters: z.record(z.unknown()).optional().describe("Parameters to pass to parameterized queries"),
                maxRows: z.number().optional().describe("Maximum number of rows to return")
                scopePath: 'query:run',
                resource: (props.queryName as string) || (props.queryId as string) || '*'
                if (!props.queryName && !props.queryId) {
                        error: "Either queryName or queryId must be provided"
                        QueryID: props.queryId as string | undefined,
                        QueryName: props.queryName as string | undefined,
                        CategoryPath: props.categoryPath as string | undefined,
                        Parameters: props.parameters as Record<string, unknown> | undefined,
                        MaxRows: props.maxRows as number | undefined
                            error: result.ErrorMessage
                        rowCount: result.Results?.length || 0,
                        results: result.Results
        // Add schema discovery tool (for understanding available entities)
            name: "Get_Database_Schema",
            description: "Get information about available MemberJunction entities and their fields",
                schemaFilter: z.string().optional().describe("Filter by schema name (e.g., 'dbo', '__mj')"),
                entityFilter: z.string().optional().describe("Filter by entity name pattern")
                let entities = md.Entities;
                // Apply schema filter
                if (props.schemaFilter) {
                    const schemaLower = (props.schemaFilter as string).toLowerCase();
                    entities = entities.filter((e: EntityInfo) => e.SchemaName.toLowerCase() === schemaLower);
                // Apply entity filter
                if (props.entityFilter) {
                    const entityFilter = props.entityFilter as string;
                    if (entityFilter.includes('*')) {
                        const regexPattern = entityFilter
                        entities = entities.filter((e: EntityInfo) => regex.test(e.Name));
                        const filterLower = entityFilter.toLowerCase();
                        entities = entities.filter((e: EntityInfo) => e.Name.toLowerCase().includes(filterLower));
                // Apply allowed/blocked schemas from config
                const queryToolsConfig = _config.mcpServerSettings?.queryTools;
                if (queryToolsConfig?.allowedSchemas?.length) {
                    const allowed = queryToolsConfig.allowedSchemas.map((s: string) => s.toLowerCase());
                    entities = entities.filter((e: EntityInfo) => allowed.includes(e.SchemaName.toLowerCase()));
                if (queryToolsConfig?.blockedSchemas?.length) {
                    const blocked = queryToolsConfig.blockedSchemas.map((s: string) => s.toLowerCase());
                    entities = entities.filter((e: EntityInfo) => !blocked.includes(e.SchemaName.toLowerCase()));
                return JSON.stringify(entities.map((e: EntityInfo) => ({
                    schema: e.SchemaName,
                    table: e.BaseTable,
                    entityName: e.Name,
                    description: e.Description,
                    columns: e.Fields.map((f: EntityFieldInfo) => ({
                        name: f.Name,
                        type: f.Type,
                        length: f.Length,
                        nullable: f.AllowsNull,
                        isPrimaryKey: f.IsPrimaryKey,
                        description: f.Description
 * PROMPT TOOLS
 * Loads AI Prompt tools based on configuration.
 * Creates tools for discovering and executing AI prompts:
 * - `Discover_Prompts` - Lists available AI prompts
 * - `Run_Prompt` - Execute any prompt by name or ID
 * - `Execute_[PromptName]_Prompt` - Specific tools for each configured prompt
 * @param systemUser - System user for initialization
async function loadPromptTools(
    const promptTools = _config.mcpServerSettings?.promptTools;
    if (promptTools && promptTools.length > 0) {
        // Add discovery tool if any prompt tool has discover enabled
        const hasDiscovery = promptTools.some((tool: MCPServerPromptToolInfo) => tool.discover);
                name: "Discover_Prompts",
                description: "List available AI Prompts based on a name pattern and/or category (* for all)",
                    pattern: z.string().optional().describe("Name pattern to match prompts (supports wildcards)"),
                    category: z.string().optional().describe("Category name to filter prompts")
                scopeInfo: { scopePath: 'prompt:read', resource: '*' },
                    let prompts = aiEngine.Prompts;
                    // Filter by category
                    if (props.category && props.category !== '*') {
                        const categoryLower = (props.category as string).toLowerCase();
                        prompts = prompts.filter((p: AIPromptEntityExtended) => p.Category?.toLowerCase().includes(categoryLower));
                            prompts = prompts.filter((p: AIPromptEntityExtended) => p.Name && regex.test(p.Name));
                            prompts = prompts.filter((p: AIPromptEntityExtended) => p.Name === pattern);
                    return JSON.stringify(prompts.map((p: AIPromptEntityExtended) => ({
                        id: p.ID,
                        description: p.Description || '',
                        category: p.Category,
                        templateText: p.TemplateText?.substring(0, 200) + (p.TemplateText && p.TemplateText.length > 200 ? '...' : ''),
                        responseFormat: p.ResponseFormat
        // Add general prompt execution tool if any tool has execute enabled
        const hasExecute = promptTools.some((tool: MCPServerPromptToolInfo) => tool.execute);
                name: "Run_Prompt",
                description: "Execute any AI Prompt by name or ID with the specified data",
                    promptName: z.string().optional().describe("Name of the prompt to execute"),
                    promptId: z.string().optional().describe("ID of the prompt to execute"),
                    data: z.record(z.unknown()).optional().describe("Data to pass to the prompt template"),
                    modelId: z.string().optional().describe("Optional model ID to use for execution")
                    scopePath: 'prompt:execute',
                    resource: (props.promptName as string) || (props.promptId as string) || '*'
                        let prompt: AIPromptEntityExtended | undefined;
                        if (props.promptId) {
                            prompt = aiEngine.Prompts.find((p: AIPromptEntityExtended) => p.ID === props.promptId);
                                    error: `Prompt not found with ID: ${props.promptId}`
                        } else if (props.promptName) {
                            prompt = aiEngine.Prompts.find((p: AIPromptEntityExtended) => p.Name?.toLowerCase() === (props.promptName as string)?.toLowerCase());
                                    error: `Prompt not found with name: ${props.promptName}`
                                error: "Either promptName or promptId must be provided"
                        promptParams.data = (props.data as Record<string, unknown>) || {};
                        promptParams.contextUser = sessionUser;
                            rawResult: result.rawResult,
                            errorMessage: result.errorMessage
 * COMMUNICATION TOOLS
 * Loads Communication tools based on configuration.
 * Creates tools for sending messages via various communication channels:
 * - `Send_Email` - Send an email via configured provider
 * - `Get_Communication_Providers` - List available communication providers
function loadCommunicationTools(addToolWithFilter: AddToolFn, sessionContext: MCPSessionContext): void {
    const commTools = _config.mcpServerSettings?.communicationTools;
    if (commTools?.enabled) {
        // Add email sending tool
            name: "Send_Email",
            description: "Send an email message. Note: Requires email provider to be configured.",
                to: z.string().describe("Recipient email address"),
                subject: z.string().describe("Email subject"),
                body: z.string().describe("Email body (can be HTML)"),
                isHtml: z.boolean().optional().default(true).describe("Whether body is HTML (default: true)")
            scopeInfo: { scopePath: 'communication:send', resource: '*' },
            async execute(_props) {
                    // Note: This is a placeholder - actual implementation would use CommunicationEngine
                    // For now, return a message indicating that communication would be sent
                        error: "Email sending requires CommunicationEngine configuration. Please configure your email provider.",
                        note: "This tool is a placeholder. Full implementation requires proper CommunicationEngine integration."
        // Add provider discovery tool
            name: "Get_Communication_Providers",
            description: "List available communication providers configured in the system",
            scopeInfo: { scopePath: 'communication:read', resource: '*' },
                    EntityName: 'MJ: Communication Providers',
                    Fields: ['ID', 'Name', 'Description', 'Status', 'SupportsSending']
                return JSON.stringify(result.Results || []);
 * AGENT HELPERS
 * Discovers AI agents matching a given name pattern.
 * Pattern matching:
 * - `*` - Returns all agents
 * - `exact` - Exact name match (case-sensitive)
 * - `prefix*` - Agents starting with prefix (case-insensitive)
 * - `*suffix` - Agents ending with suffix (case-insensitive)
 * - `*contains*` - Agents containing the text (case-insensitive)
 * @param userContext - Optional user context for AIEngine configuration
 * @returns Array of matching agent entities
async function discoverAgents(pattern: string, userContext?: UserInfo): Promise<AIAgentEntityExtended[]> {
    await aiEngine.Config(false, userContext);
        return allAgents;
        return allAgents.filter(a => a.Name === pattern);
    return allAgents.filter(a => a.Name && regex.test(a.Name));
 * Creates and registers an execution tool for a specific AI agent.
 * The generated tool allows MCP clients to execute the agent with:
 * - Optional conversation history for context
 * - Optional template data
 * - Synchronous or asynchronous execution modes
 * @param agent - The agent entity to create an execution tool for
function addAgentExecuteTool(
        name: `Execute_${(agent.Name || 'Unknown').replace(/\s+/g, '_')}_Agent`,
        description: `Execute the ${agent.Name || 'Unknown'} agent. ${agent.Description || ''}`,
        scopeInfo: { scopePath: 'agent:execute', resource: agent.Name || '*' },
 * ENTITY TOOLS
 * Loads and registers entity tools based on configuration.
 * For each configured entity tool pattern, creates tools for the specified operations:
 * - Get: Retrieve a record by primary key
 * - Create: Create a new record
 * - Update: Update an existing record
 * - Delete: Delete a record
 * - RunView: Query records with filtering and sorting
 * Entity matching supports wildcards in both entityName and schemaName.
async function loadEntityTools(addToolWithFilter: AddToolFn): Promise<void> {
    const entityTools = _config.mcpServerSettings?.entityTools;
    if (entityTools && entityTools.length > 0) {
        // Iterate through the tools and add them to the server
        entityTools.forEach((tool) => {
            const matchingEntities = getMatchingEntitiesForTool(md.Entities, tool);
            matchingEntities.forEach((entity) => {
                if (tool.get) {
                    addEntityGetTool(addToolWithFilter, entity);
                if (tool.create) {
                    addEntityCreateTool(addToolWithFilter, entity);
                if (tool.update) {
                    addEntityUpdateTool(addToolWithFilter, entity);
                if (tool.delete) {
                    addEntityDeleteTool(addToolWithFilter, entity);
                if (tool.runView) {
                    addEntityRunViewTool(addToolWithFilter, entity);
 * Creates a RunView tool for querying entity records.
 * Allows filtering, sorting, and field selection.
 * @param entity - The entity to create the tool for
function addEntityRunViewTool(addToolWithFilter: AddToolFn, entity: EntityInfo): void {
    const paramObject = z.object({
        extraFilter: z.string().optional(),
        orderBy: z.string().optional(),
        fields: z.array(z.string()).optional(),
        name: `Run_${entity.ClassName}_View`,
        description: `Returns data from the ${entity.Name} entity, optionally filtered by extraFilter and ordered by orderBy`,
        parameters: paramObject,
        scopeInfo: { scopePath: 'view:run', resource: entity.Name },
        async execute(props, sessionContext) {
                ExtraFilter: props.extraFilter ? props.extraFilter as string : undefined,
                OrderBy: props.orderBy ? props.orderBy as string : undefined,
                Fields: props.fields ? props.fields as string[] : undefined,
            return JSON.stringify(result);
 * Creates a tool for creating new records in an entity.
 * Parameters are generated from the entity's non-readonly fields.
function addEntityCreateTool(addToolWithFilter: AddToolFn, entity: EntityInfo): void {
    const paramObject = getEntityParamObject(entity, true, false, false);
        name: `Create_${entity.ClassName}_Record`,
        description: `Creates a new record in the ${entity.Name} entity`,
        parameters: z.object(paramObject),
        scopeInfo: { scopePath: 'entity:create', resource: entity.Name },
            const record = await md.GetEntityObject(entity.Name, sessionUser);
            record.SetMany(props, true);
            if (!success) {
                return JSON.stringify({success, record: undefined, errorMessage: record.LatestResult.CompleteMessage });
                return JSON.stringify({success, record: await convertEntityObjectToJSON(record), errorMessage: undefined });
 * Creates a tool for updating existing records in an entity.
 * Requires primary key fields to identify the record, plus optional fields to update.
function addEntityUpdateTool(addToolWithFilter: AddToolFn, entity: EntityInfo): void {
    const paramObject = getEntityParamObject(entity, true, true, true);
        name: `Update_${entity.ClassName}_Record`,
        description: `Updates the specified record in the ${entity.Name} entity`,
        scopeInfo: { scopePath: 'entity:update', resource: entity.Name },
            const loaded = await record.InnerLoad(new CompositeKey(
                // use the primary keys to load the record
                entity.PrimaryKeys.map((pk) => {
                        Value: props[pk.Name]
                // remove the primary keys from the props so we don't try to update them
                const newProps = { ...props };
                entity.PrimaryKeys.forEach((pk) => {
                    delete newProps[pk.Name];
                record.SetMany(newProps, true);
                return JSON.stringify({success, record: await convertEntityObjectToJSON(record), errorMessage: !success ? record.LatestResult.CompleteMessage : undefined });
                return JSON.stringify({success: false, record: undefined, errorMessage: "Record not found"});
 * Creates a tool for deleting records from an entity.
 * Only requires the primary key field(s) to identify the record.
function addEntityDeleteTool(addToolWithFilter: AddToolFn, entity: EntityInfo): void {
    const pkeyParams = getEntityPrimaryKeyParamsObject(entity);
        name: `Delete_${entity.ClassName}_Record`,
        description: `Deletes the specified record from the ${entity.Name} entity`,
        parameters: z.object(pkeyParams),
        scopeInfo: { scopePath: 'entity:delete', resource: entity.Name },
                const savedRecordJSON = await convertEntityObjectToJSON(record);
                return JSON.stringify({success, record: savedRecordJSON, errorMessage: !success ? record.LatestResult.CompleteMessage : undefined });
 * ENTITY HELPER FUNCTIONS
 * Converts a BaseEntity object to a JSON representation suitable for API responses.
 * @param record - The entity record to convert
 * @returns JSON string representation of the entity data
async function convertEntityObjectToJSON(record: BaseEntity): Promise<string> {
 * Builds a Zod parameter object for entity CRUD operations.
 * @param entity - The entity to build parameters for
 * @param excludeReadOnlyFields - If true, excludes read-only fields from the schema
 * @param includePrimaryKeys - If true, includes primary key fields
 * @param nonPKeysOptional - If true, makes non-primary-key fields optional
 * @returns Object with Zod schemas for each included field
function getEntityParamObject(
    entity: EntityInfo,
    excludeReadOnlyFields: boolean,
    includePrimaryKeys: boolean,
    nonPKeysOptional: boolean
): Record<string, z.ZodTypeAny> {
    const paramObject: Record<string, z.ZodTypeAny> = {};
    entity.Fields.filter(f => {
        if (f.IsPrimaryKey && includePrimaryKeys) {
        else if (f.ReadOnly && excludeReadOnlyFields) {
    }).forEach((f) => {
        addSingleParamToObject(paramObject, f, f.IsPrimaryKey ? false : nonPKeysOptional);
    return paramObject;
 * Adds a single field as a Zod parameter to the parameter object.
 * Handles type mapping from MemberJunction types to Zod schemas,
 * including support for value lists (enums).
 * @param theObject - The parameter object to add the field to
 * @param field - The entity field info
 * @param optional - If true, makes the parameter optional
function addSingleParamToObject(
    theObject: Record<string, z.ZodTypeAny>,
    field: EntityFieldInfo,
    optional: boolean
    let newParam: z.ZodTypeAny;
    switch (field.TSType) {
        case 'Date':
            newParam = z.date();
            newParam = z.boolean();
            newParam = z.number();
            // For strings, check if we have a value list and create an enum
            if (field.ValueListTypeEnum === 'None' || field.EntityFieldValues.length === 0) {
                newParam = z.string();
                const enumList = field.EntityFieldValues.map((v) => v.Value);
                if (field.ValueListTypeEnum === 'List') {
                    newParam = z.enum(enumList as [string, ...string[]]);
                    // List + user input: allow enum values or any string
                    newParam = z.union([z.enum(enumList as [string, ...string[]]), z.string()]);
    if (optional) {
        theObject[field.Name] = newParam.optional();
        theObject[field.Name] = newParam;
 * Builds a Zod parameter object containing only the primary key fields for an entity.
 * Used for Get and Delete operations that only need to identify the record.
 * @param entity - The entity to build primary key parameters for
 * @returns Object with Zod schemas for each primary key field
function getEntityPrimaryKeyParamsObject(entity: EntityInfo): Record<string, z.ZodTypeAny> {
        addSingleParamToObject(paramObject, pk, false);
 * Creates a tool for retrieving a single record from an entity by primary key.
function addEntityGetTool(addToolWithFilter: AddToolFn, entity: EntityInfo): void {
        name: `Get_${entity.ClassName}_Record`,
        description: `Retrieves the specified record from the ${entity.Name} entity`,
        scopeInfo: { scopePath: 'entity:read', resource: entity.Name },
            await record.InnerLoad(new CompositeKey(
                entity.PrimaryKeys.map((pk) => ({
            return await convertEntityObjectToJSON(record);
 * Configuration interface for entity tool matching.
interface EntityToolConfig {
    get?: boolean;
    create?: boolean;
    update?: boolean;
    delete?: boolean;
    runView?: boolean;
    schemaName?: string;
 * Filters entities based on tool configuration patterns.
 * Supports wildcard matching for both entityName and schemaName:
 * - `*` - Matches all
 * - `prefix*` - Matches names starting with prefix
 * - `*suffix` - Matches names ending with suffix
 * - `*contains*` - Matches names containing the text
 * @param allEntities - Array of all available entities
 * @param tool - Tool configuration with entity/schema patterns
 * @returns Array of entities matching the patterns
function getMatchingEntitiesForTool(allEntities: EntityInfo[], tool: EntityToolConfig): EntityInfo[] {
    const matchingEntities = allEntities.filter((entity) => {
        const entityName = entity.Name;
        const schemaName = entity.SchemaName;
        const toolEntityName = tool.entityName?.trim().toLowerCase() || "*";
        const toolSchemaName = tool.schemaName?.trim().toLowerCase() || "*";
        // we support wildcards such as * which is all entities/schemas, *Partial which would be Partial is the ending of the string, or Partial* where Partial is the start of the string
        // so we need to check for the conditions as follows: exact match, wildcard at the start, wildcard at the end, and wildcard only means always match and assign to two variables, schemaMatch
        // first to scope the schema and then entityMatch if the schemaMatch is true
        if (toolSchemaName === "*") {
        else if (toolSchemaName.startsWith("*") && toolSchemaName.endsWith("*")) {
            schemaMatch = schemaName.toLowerCase().includes(toolSchemaName.slice(1, -1));
        else if (toolSchemaName.endsWith("*")) {
            schemaMatch = schemaName.toLowerCase().startsWith(toolSchemaName.slice(0, -1));
        else if (toolSchemaName.startsWith("*")) {
            schemaMatch = schemaName.toLowerCase().endsWith(toolSchemaName.slice(1));
            schemaMatch = schemaName.toLowerCase() === toolSchemaName;
            // if the schema matches, we can check the entity name, otherwise we don't bother since we don't care about the entity name
            if (toolEntityName === "*") {
            else if (toolEntityName.startsWith("*") && toolEntityName.endsWith("*")) {
                entityMatch = entityName.toLowerCase().includes(toolEntityName.slice(1, -1));
            else if (toolEntityName.endsWith("*")) {
                entityMatch = entityName.toLowerCase().startsWith(toolEntityName.slice(0, -1));
            else if (toolEntityName.startsWith("*")) {
                entityMatch = entityName.toLowerCase().endsWith(toolEntityName.slice(1));
                entityMatch = entityName.toLowerCase() === toolEntityName;
    return matchingEntities;
 * CLI UTILITIES
 * Lists all available tools without starting the server.
 * This function is used by the `--list-tools` CLI option to display what tools
 * would be available based on the current configuration. It connects to the
 * database to discover dynamic tools (entities, agents) but does not start
 * the MCP server.
 * Output is grouped by tool prefix and sorted alphabetically.
 * @param filterOptions - Optional filter patterns to show a subset of tools
 * // List all tools
 * await listAvailableTools();
 * // List only Get_* tools
 * await listAvailableTools({ includePatterns: ['Get_*'] });
export async function listAvailableTools(filterOptions: ToolFilterOptions = {}): Promise<void> {
        // Store filter options
        // Initialize database connection to discover dynamic tools
        // Register all tools (they won't actually be added to server, just tracked)
        // We need to use a dummy filter that includes everything for listing
        const listingFilterOptions = { ...filterOptions };
        activeFilterOptions = {}; // Temporarily clear filters to get all tool names
        // Add built-in tools
        registeredToolNames.push("Get_Entity_List");
        registeredToolNames.push("Get_Single_Entity");
        // Add agent run diagnostic tools
        registeredToolNames.push("List_Recent_Agent_Runs");
        registeredToolNames.push("Get_Agent_Run_Summary");
        registeredToolNames.push("Get_Agent_Run_Step_Detail");
        registeredToolNames.push("Get_Agent_Run_Step_Full_Data");
        // Use system user for tool discovery
            throw new Error('System user not found in UserCache');
        // Load tools to populate registeredToolNames
        await loadEntityToolsForListing(systemUser);
        await loadAgentToolsForListing(systemUser);
        // Close database connection
        await pool.close();
        // Apply filters to the list if specified
        let toolsToShow = registeredToolNames;
        if (listingFilterOptions.includePatterns || listingFilterOptions.excludePatterns) {
            activeFilterOptions = listingFilterOptions;
            toolsToShow = registeredToolNames.filter(name => shouldIncludeTool(name, listingFilterOptions));
        // Sort tools alphabetically
        toolsToShow.sort();
        console.log("\n=== Available MCP Tools ===\n");
            console.log(`Showing ${toolsToShow.length} of ${registeredToolNames.length} tools (filtered)\n`);
            console.log(`Total tools: ${toolsToShow.length}\n`);
        // Group tools by prefix for better readability
        const toolGroups: Record<string, string[]> = {};
        for (const tool of toolsToShow) {
            const prefix = tool.split('_')[0];
            if (!toolGroups[prefix]) {
                toolGroups[prefix] = [];
            toolGroups[prefix].push(tool);
        // Print grouped tools
        for (const [prefix, tools] of Object.entries(toolGroups).sort()) {
            console.log(`--- ${prefix} ---`);
            for (const tool of tools) {
                console.log(`  ${tool}`);
            console.log();
        console.log("Use --include and --exclude to filter tools when starting the server.");
        console.log("Example: npx @memberjunction/ai-mcp-server --include \"Get_Users_*,Run_Agent\"");
        console.error("Failed to list tools:", error);
 * Populates the registeredToolNames array with entity tool names for the --list-tools CLI option.
 * Does not actually register the tools with the server, just collects their names.
 * @param _systemUser - System user for context (unused in this function but kept for API consistency)
async function loadEntityToolsForListing(_systemUser: UserInfo): Promise<void> {
                    registeredToolNames.push(`Get_${entity.ClassName}_Record`);
                    registeredToolNames.push(`Create_${entity.ClassName}_Record`);
                    registeredToolNames.push(`Update_${entity.ClassName}_Record`);
                    registeredToolNames.push(`Delete_${entity.ClassName}_Record`);
                    registeredToolNames.push(`Run_${entity.ClassName}_View`);
 * Populates the registeredToolNames array with agent tool names for the --list-tools CLI option.
 * @param systemUser - System user for context when discovering agents
async function loadAgentToolsForListing(systemUser: UserInfo): Promise<void> {
        const hasDiscovery = agentTools.some(tool => tool.discover);
            registeredToolNames.push("Discover_Agents");
            registeredToolNames.push("Run_Agent");
        // Add specific agent execution tools
                    registeredToolNames.push(`Execute_${(agent.Name || 'Unknown').replace(/\s+/g, '_')}_Agent`);
            registeredToolNames.push("Get_Agent_Run_Status");
            registeredToolNames.push("Cancel_Agent_Run");
import express, { Request, Response, NextFunction, Router } from 'express';
import cors from 'cors';
import { createHash } from 'crypto';
  LogStatus,
import { MJComponentEntity, MJComponentRegistryEntity } from '@memberjunction/core-entities';
import { configInfo, componentRegistrySettings, dbDatabase, dbHost, dbPort, dbUsername, dbReadOnlyUsername, dbReadOnlyPassword } from './config.js';
import createMSSQLConfig from './orm.js';
import { DataSourceInfo, ComponentRegistryServerOptions, ComponentFeedbackParams, ComponentFeedbackResponse, FeedbackHandler } from './types.js';
 * Base class for the Component Registry API Server.
 * This class provides a complete implementation of the Component Registry API v1 specification.
 * To customize the server behavior, extend this class and override the appropriate methods.
 * Common customization points include:
 * - Authentication: Override `checkAPIKey()` to implement custom authentication
 * - Component filtering: Override `getComponentFilter()` to customize which components are served
 * - Response formatting: Override the route handler methods to customize response formats
 * class MyCustomRegistryServer extends ComponentRegistryAPIServer {
 *   protected async checkAPIKey(req: Request): Promise<boolean> {
 *     const apiKey = req.headers['x-api-key'];
 *     return await myCustomAuthProvider.validateKey(apiKey);
export class ComponentRegistryAPIServer {
  protected app: express.Application | null = null;
  protected router: express.Router | null = null;
  protected registry: MJComponentRegistryEntity | null = null;
  protected metadata: Metadata;
  protected pool: sql.ConnectionPool | null = null;
  protected readOnlyPool: sql.ConnectionPool | null = null;
  protected dataSources: DataSourceInfo[] = [];
  protected options: ComponentRegistryServerOptions;
  constructor(options: ComponentRegistryServerOptions = {}) {
    // Set default options
    this.options = {
      basePath: '/api/v1',
      skipDatabaseSetup: false,
    // Create app or router based on mode
    if (this.options.mode === 'standalone') {
      this.app = express();
      this.router = null;
      this.app = null;
      this.router = express.Router();
   * Get the Express Router for mounting on an existing app.
   * Only available in 'router' mode.
   * @returns The Express Router with all registry routes configured
   * @throws Error if called in standalone mode
  public getRouter(): express.Router {
    if (this.options.mode !== 'router') {
      throw new Error('getRouter() is only available in router mode');
    if (!this.router) {
      throw new Error('Router not initialized. Call initialize() first.');
    return this.router;
   * Initialize the server, including database connection, middleware, and routes.
   * This method should be called before starting the server.
   * @returns Promise that resolves when initialization is complete
   * @throws Error if database connection fails or registry cannot be loaded
  public async initialize(): Promise<void> {
    // Setup database connection only if not skipped
    if (!this.options.skipDatabaseSetup) {
      await this.setupDatabase();
    // Load registry metadata if ID provided
    if (componentRegistrySettings?.registryId) {
      await this.loadRegistry();
    // Setup middleware and routes
    this.setupMiddleware();
    this.setupRoutes();
   * Start the Express server on the configured port.
   * Must be called after `initialize()`.
   * @returns Promise that resolves when the server is listening
  public async start(): Promise<void> {
    if (this.options.mode !== 'standalone') {
      throw new Error('start() is only available in standalone mode. Use getRouter() in router mode.');
    if (!this.app) {
      throw new Error('Express app not initialized');
    const port = componentRegistrySettings?.port || 3200;
      this.app!.listen(port, () => {
        LogStatus(`Component Registry API Server running on port ${port}`);
        LogStatus(`API endpoint: http://localhost:${port}${this.options.basePath}`);
   * Set up the database connection using MemberJunction's SQL Server provider.
   * Follows the same pattern as MJServer for consistency.
   * @virtual
  protected async setupDatabase(): Promise<void> {
    // Create the main connection pool using the same config pattern as MJServer
    this.pool = new sql.ConnectionPool(createMSSQLConfig());
    await this.pool.connect();
    // Get cache refresh interval from config (default to 0 if not set)
    const cacheRefreshInterval = configInfo.databaseSettings?.metadataCacheRefreshInterval || 0;
    // Setup MemberJunction SQL Server client with cache refresh interval
    const config = new SQLServerProviderConfigData(this.pool, configInfo.mjCoreSchema, cacheRefreshInterval);
    // Initialize metadata and log entity count like MJServer does
    LogStatus(`Database connection established. ${md?.Entities ? md.Entities.length : 0} entities loaded.`);
    // Create data sources array
    this.dataSources = [new DataSourceInfo({
      dataSource: this.pool, 
      type: 'Read-Write', 
      host: dbHost, 
      userName: dbUsername
    })];
    // Establish a second read-only connection if credentials are provided
    if (dbReadOnlyUsername && dbReadOnlyPassword) {
      const readOnlyConfig = {
        ...createMSSQLConfig(),
        user: dbReadOnlyUsername,
        password: dbReadOnlyPassword,
      this.readOnlyPool = new sql.ConnectionPool(readOnlyConfig);
      await this.readOnlyPool.connect();
      // Add read-only pool to data sources
      this.dataSources.push(new DataSourceInfo({
        dataSource: this.readOnlyPool, 
        type: 'Read-Only', 
        userName: dbReadOnlyUsername
      LogStatus('Read-only connection pool has been initialized.');
   * Load the registry metadata from the database.
   * This is called automatically if a registryId is provided in the configuration.
  protected async loadRegistry(): Promise<void> {
    if (!componentRegistrySettings?.registryId) {
    this.registry = await this.metadata.GetEntityObject<MJComponentRegistryEntity>('MJ: Component Registries');
    const loaded = await this.registry.Load(componentRegistrySettings.registryId);
      throw new Error(`Failed to load registry with ID: ${componentRegistrySettings.registryId}`);
    LogStatus(`Loaded registry: ${this.registry.Name}`);
   * Set up Express middleware.
   * Override this method to add custom middleware or modify the middleware stack.
    // Get the target for middleware (app or router)
    const target = this.options.mode === 'standalone' ? this.app : this.router;
      throw new Error('No app or router available for middleware setup');
    // In standalone mode, setup CORS and JSON parsing
    // In router mode, assume parent app handles these
      // CORS
      target.use(cors({
        origin: componentRegistrySettings?.corsOrigins || ['*']
      // JSON parsing
      target.use(express.json());
    // Auth middleware applies in both modes (if enabled)
    if (componentRegistrySettings?.requireAuth) {
      // In router mode, paths are relative to where router is mounted
      // In standalone mode, use full paths
      if (this.options.mode === 'router') {
        // Apply auth to all /components routes relative to router mount point
        target.use('/components', this.authMiddleware.bind(this));
        // Apply auth to full path in standalone mode
        target.use(`${this.options.basePath}/components`, this.authMiddleware.bind(this));
   * Authentication middleware that calls the checkAPIKey method.
   * This middleware is automatically applied to /api/v1/components routes when requireAuth is true.
  protected async authMiddleware(req: Request, res: Response, next: NextFunction): Promise<void> {
      const isValid = await this.checkAPIKey(req);
      if (!isValid) {
        res.status(401).json({ error: 'Unauthorized' });
      LogError(`Authentication error: ${error instanceof Error ? error.message : String(error)}`);
      res.status(500).json({ error: 'Authentication error' });
   * Check if the API key in the request is valid.
   * By default, this method always returns true (no authentication).
   * Override this method to implement custom authentication logic.
   * Common patterns include:
   * - Checking Bearer tokens in Authorization header
   * - Validating API keys in custom headers
   * - Verifying JWT tokens
   * - Checking against a database of valid keys
   * @param req - The Express request object
   * @returns Promise<boolean> - True if the request is authenticated, false otherwise
   * protected async checkAPIKey(req: Request): Promise<boolean> {
   *   const apiKey = req.headers['x-api-key'] as string;
   *   if (!apiKey) return false;
   *   // Check against database, cache, or external service
   *   return await this.validateKeyInDatabase(apiKey);
    // Default implementation: no authentication required
    // Override this method in a subclass to implement custom authentication
   * Set up the API routes.
   * Override this method to add custom routes or modify existing ones.
  protected setupRoutes(): void {
    // Get the target for routes (app or router)
      throw new Error('No app or router available for route setup');
      // Router mode: paths are relative to where router is mounted
      target.get('/registry', this.getRegistryInfo.bind(this));
      target.get('/health', this.getHealth.bind(this));
      target.get('/components', this.listComponents.bind(this));
      target.get('/components/search', this.searchComponents.bind(this));
      target.get('/components/:namespace/:name', this.getComponent.bind(this));
      target.post('/feedback', this.submitFeedback.bind(this));
      // Standalone mode: use full paths with basePath
      const basePath = this.options.basePath;
      target.get(`${basePath}/registry`, this.getRegistryInfo.bind(this));
      target.get(`${basePath}/health`, this.getHealth.bind(this));
      target.get(`${basePath}/components`, this.listComponents.bind(this));
      target.get(`${basePath}/components/search`, this.searchComponents.bind(this));
      target.get(`${basePath}/components/:namespace/:name`, this.getComponent.bind(this));
      target.post(`${basePath}/feedback`, this.submitFeedback.bind(this));
   * Get the base filter for component queries.
   * By default, this returns components where SourceRegistryID IS NULL (local components only).
   * Override this method to customize which components are served by the registry.
   * @returns The SQL filter string to apply to all component queries
   * protected getComponentFilter(): string {
   *   // Include both local and specific external registry components
   *   return "(SourceRegistryID IS NULL OR SourceRegistryID = 'abc-123')";
    return 'SourceRegistryID IS NULL';
   * Handler for GET /api/v1/registry
   * Returns basic information about the registry.
  protected async getRegistryInfo(req: Request, res: Response): Promise<void> {
      name: this.registry?.Name || 'Local Component Registry',
      description: this.registry?.Description || 'MemberJunction Component Registry',
      requiresAuth: componentRegistrySettings?.requireAuth || false
   * Handler for GET /api/v1/health
   * Returns the health status of the registry server.
  protected async getHealth(req: Request, res: Response): Promise<void> {
        ExtraFilter: this.getComponentFilter(),
        status: 'healthy',
        componentCount: result.TotalRowCount
      res.status(503).json({
        status: 'unhealthy',
   * Handler for GET /api/v1/components
   * Lists all published components in the registry, showing only the latest version of each.
  protected async listComponents(req: Request, res: Response): Promise<void> {
      const baseFilter = this.getComponentFilter();
      const filter = `${baseFilter} AND Status = 'Published'`;
      const result = await rv.RunView<MJComponentEntity>({
        OrderBy: 'Namespace, Name, VersionSequence DESC' 
        res.status(500).json({ error: result.ErrorMessage });
      // Group by namespace/name and take latest version
      const latestComponents = this.getLatestVersions(result.Results || []);
      const components = latestComponents.map(c => ({
        namespace: c.Namespace,
        name: c.Name,
        version: c.Version,
        title: c.Title,
        description: c.Description,
        type: c.Type,
        status: c.Status
        total: components.length
      LogError(`Failed to list components: ${error instanceof Error ? error.message : String(error)}`);
      res.status(500).json({ error: 'Failed to list components' });
   * Handler for GET /api/v1/components/search
   * Search for components by query string and optional type filter.
  protected async searchComponents(req: Request, res: Response): Promise<void> {
      const { q, type } = req.query;
      let filter = `${this.getComponentFilter()} AND Status = 'Published'`;
      if (q && typeof q === 'string') {
        // Escape single quotes in the search query
        const escapedQuery = q.replace(/'/g, "''");
        filter += ` AND (Name LIKE '%${escapedQuery}%' OR Title LIKE '%${escapedQuery}%' OR Description LIKE '%${escapedQuery}%')`;
      if (type && typeof type === 'string') {
        const escapedType = type.replace(/'/g, "''");
        filter += ` AND Type = '${escapedType}'`;
      const results = latestComponents.map(c => ({
        total: results.length,
        query: q || ''
      LogError(`Search failed: ${error instanceof Error ? error.message : String(error)}`);
      res.status(500).json({ error: 'Search failed' });
   * Generate SHA-256 hash of component specification
   * @param specification - The component specification JSON string
   * @returns SHA-256 hash as hex string
  protected generateSpecificationHash(specification: string): string {
    return createHash('sha256').update(specification).digest('hex');
   * Handler for GET /api/v1/components/:namespace/:name
   * Get a specific component by namespace and name.
   * Optionally specify a version with ?version=x.x.x query parameter.
   * Optionally specify a hash with ?hash=abc123 to enable caching (returns 304 if unchanged).
  protected async getComponent(req: Request, res: Response): Promise<void> {
      const { namespace, name } = req.params;
      const { version, hash } = req.query;
      // Ensure namespace and name are strings (route params are always strings)
      const namespaceStr = Array.isArray(namespace) ? namespace[0] : namespace;
      const nameStr = Array.isArray(name) ? name[0] : name;
      // Escape single quotes in parameters
      const escapedNamespace = namespaceStr.replace(/'/g, "''");
      const escapedName = nameStr.replace(/'/g, "''");
      let filter = `Namespace = '${escapedNamespace}' AND Name = '${escapedName}' AND ${this.getComponentFilter()}`;
      if (version && typeof version === 'string') {
        const escapedVersion = version.replace(/'/g, "''");
        filter += ` AND Version = '${escapedVersion}'`;
        OrderBy: 'VersionSequence DESC',
      if (!result.Success || !result.Results?.length) {
        res.status(404).json({ error: 'Component not found' });
      const component = result.Results[0];
      // Generate hash of the current specification
      const currentHash = this.generateSpecificationHash(component.Specification);
      // If client provided a hash and it matches, return 304 Not Modified
      if (hash && typeof hash === 'string' && hash === currentHash) {
        res.status(304).json({
          message: 'Not modified',
          hash: currentHash,
          id: component.ID,
          namespace: component.Namespace,
          name: component.Name,
          version: component.Version
      // Return full specification with hash
        version: component.Version,
        specification: JSON.parse(component.Specification),
        hash: currentHash
      LogError(`Failed to fetch component: ${error instanceof Error ? error.message : String(error)}`);
      res.status(500).json({ error: 'Failed to fetch component' });
   * Helper method to get the latest version of each component from a list.
   * Components are grouped by namespace/name and the highest version is selected.
   * @param components - Array of components potentially containing multiple versions
   * @returns Array of components with only the latest version of each
  protected getLatestVersions(components: MJComponentEntity[]): MJComponentEntity[] {
    const latestComponents = new Map<string, MJComponentEntity>();
    for (const component of components) {
      const key = `${component.Namespace}/${component.Name}`;
      if (!latestComponents.has(key)) {
        latestComponents.set(key, component);
    return Array.from(latestComponents.values());
   * Default feedback handler implementation.
   * Simply logs feedback and returns success.
   * Override by calling setFeedbackHandler() with a custom implementation.
  protected feedbackHandler: FeedbackHandler = {
    async submitFeedback(params: ComponentFeedbackParams): Promise<ComponentFeedbackResponse> {
      LogStatus('Component feedback received (default handler):', undefined, {
        component: `${params.componentNamespace}/${params.componentName}`,
        version: params.componentVersion,
        rating: params.rating,
        feedbackType: params.feedbackType
        feedbackID: undefined
   * Set a custom feedback handler.
   * This allows external code (e.g., Skip) to override feedback handling logic.
   * @param handler - Custom feedback handler implementation
   * const server = new ComponentRegistryAPIServer();
   * server.setFeedbackHandler({
   *   async submitFeedback(params, context) {
   *     // Custom logic here
   *     return { success: true, feedbackID: '...' };
  public setFeedbackHandler(handler: FeedbackHandler): void {
    this.feedbackHandler = handler;
   * Handler for POST /api/v1/feedback
   * Submit feedback for a component.
  protected async submitFeedback(req: Request, res: Response): Promise<void> {
      const params: ComponentFeedbackParams = req.body;
      // Basic validation
      if (!params.componentName || !params.componentNamespace) {
          error: 'componentName and componentNamespace are required'
      if (params.rating === undefined || params.rating < 0 || params.rating > 5) {
          error: 'rating must be between 0 and 5'
      // Call the feedback handler (default or custom)
      const result = await this.feedbackHandler.submitFeedback(params, req.body.context);
      LogError(`Failed to submit feedback: ${error instanceof Error ? error.message : String(error)}`);
        error: 'Failed to submit feedback'
 * Start the Component Registry Server using the default implementation.
 * This function checks if the registry is enabled in configuration before starting.
 * @returns Promise that resolves when the server is running
 * @throws Error if initialization or startup fails
export async function startComponentRegistryServer(): Promise<void> {
  if (!componentRegistrySettings?.enableRegistry) {
    LogStatus('Component Registry Server is disabled in configuration');
  const server = new ComponentRegistryAPIServer();
