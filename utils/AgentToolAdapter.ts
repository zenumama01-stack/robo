 * @fileoverview Agent Tool Adapter for MCP Client
 * Provides utilities for AI agents to discover and use MCP tools.
 * This adapter converts MCP tool definitions to formats suitable for
 * various AI providers (OpenAI, Anthropic, etc.) and provides
 * convenient methods for tool execution.
 * @module AgentToolAdapter
import { MCPClientManager } from './MCPClientManager.js';
import { MCPToolDefinition, MCPToolInfo, MCPToolCallResult, MCPContentBlock } from './types.js';
 * OpenAI-compatible function definition
export interface OpenAIFunctionDefinition {
    type: 'function';
    function: {
        parameters: Record<string, unknown>;
 * Anthropic-compatible tool definition
export interface AnthropicToolDefinition {
    input_schema: Record<string, unknown>;
 * Generic tool definition that works across providers
export interface AgentToolDefinition {
    /** Unique identifier for the tool (includes connection context) */
    /** Tool name as defined by the MCP server */
    /** Human-readable title */
    /** Detailed description of what the tool does */
    /** JSON Schema for input parameters */
    inputSchema: Record<string, unknown>;
    /** Connection ID this tool belongs to */
    connectionId: string;
    /** Tool annotations/hints */
    annotations?: {
        readOnly?: boolean;
        destructive?: boolean;
        idempotent?: boolean;
 * Result from executing a tool through the adapter
export interface AgentToolResult {
    /** Whether the execution was successful */
    /** The tool that was executed */
    toolName: string;
    /** Connection used for execution */
    /** Result content from the tool */
    content: MCPContentBlock[];
    /** Execution time in milliseconds */
    /** Error message if execution failed */
    /** Whether the result indicates an error from the tool itself */
    isError?: boolean;
 * Options for tool discovery
export interface ToolDiscoveryOptions {
    /** Filter to specific connection IDs */
    connectionIds?: string[];
    /** Filter tools by name pattern (regex) */
    namePattern?: string;
    /** Include only read-only tools */
    readOnlyOnly?: boolean;
    /** Exclude destructive tools */
    excludeDestructive?: boolean;
    /** Maximum number of tools to return */
 * Agent Tool Adapter
 * Provides a high-level interface for AI agents to discover and use MCP tools.
 * Handles connection management, tool format conversion, and execution.
 * const adapter = new AgentToolAdapter(contextUser);
 * // Get all available tools in OpenAI format
 * const openAITools = await adapter.getToolsForOpenAI();
 * // Execute a tool by name
 * const result = await adapter.executeTool('github_create_issue', {
 *     title: 'Bug report',
 *     body: 'Description of the bug'
export class AgentToolAdapter {
    private mcpManager: MCPClientManager;
    private toolCache: Map<string, AgentToolDefinition> = new Map();
    private lastCacheRefresh = 0;
    private cacheValidityMs = 60000; // 1 minute cache
        this.mcpManager = MCPClientManager.Instance;
     * Discovers all available tools across all active connections
     * @param options - Discovery options for filtering tools
     * @returns Array of tool definitions
    async discoverTools(options?: ToolDiscoveryOptions): Promise<AgentToolDefinition[]> {
        await this.refreshToolCache();
        let tools = Array.from(this.toolCache.values());
        if (options?.connectionIds?.length) {
            tools = tools.filter(t => options.connectionIds!.includes(t.connectionId));
        if (options?.namePattern) {
            const regex = new RegExp(options.namePattern, 'i');
            tools = tools.filter(t => regex.test(t.name) || regex.test(t.title));
        if (options?.readOnlyOnly) {
            tools = tools.filter(t => t.annotations?.readOnly === true);
        if (options?.excludeDestructive) {
            tools = tools.filter(t => t.annotations?.destructive !== true);
        // Sort by name alphabetically
        tools.sort((a, b) => a.name.localeCompare(b.name));
        if (options?.limit) {
            tools = tools.slice(0, options.limit);
        return tools;
     * Gets tools formatted for OpenAI's function calling API
     * @param options - Discovery options
     * @returns Array of OpenAI function definitions
    async getToolsForOpenAI(options?: ToolDiscoveryOptions): Promise<OpenAIFunctionDefinition[]> {
        const tools = await this.discoverTools(options);
        return tools.map(tool => ({
            type: 'function' as const,
                name: tool.id, // Use full ID to include connection context
                description: this.formatDescription(tool),
                parameters: tool.inputSchema
     * Gets tools formatted for Anthropic's tool use API
     * @returns Array of Anthropic tool definitions
    async getToolsForAnthropic(options?: ToolDiscoveryOptions): Promise<AnthropicToolDefinition[]> {
            name: tool.id,
            input_schema: tool.inputSchema
     * Executes a tool by its ID or name
     * @param toolIdOrName - Tool ID (with connection prefix) or just the tool name
     * @param args - Arguments to pass to the tool
     * @param connectionId - Optional connection ID (required if using tool name only)
     * @returns Tool execution result
    async executeTool(
        toolIdOrName: string,
        args: Record<string, unknown>,
        connectionId?: string
    ): Promise<AgentToolResult> {
            // Resolve tool and connection
            const resolved = await this.resolveToolAndConnection(toolIdOrName, connectionId);
            if (!resolved) {
                    toolName: toolIdOrName,
                    connectionId: connectionId || 'unknown',
                    content: [],
                    error: `Tool not found: ${toolIdOrName}`
            // Execute the tool
            const result = await this.mcpManager.callTool(
                resolved.connectionId,
                resolved.toolName,
                { arguments: args },
                { contextUser: this.contextUser }
                success: !result.isToolError,
                toolName: resolved.toolName,
                connectionId: resolved.connectionId,
                content: result.content,
                isError: result.isToolError,
                error: result.isToolError ? this.extractErrorMessage(result) : undefined
                error: error instanceof Error ? error.message : String(error)
     * Gets a tool by its ID or name
     * @param toolIdOrName - Tool ID or name
     * @returns Tool definition or undefined
    async getTool(toolIdOrName: string): Promise<AgentToolDefinition | undefined> {
        // Try direct ID lookup first
        if (this.toolCache.has(toolIdOrName)) {
            return this.toolCache.get(toolIdOrName);
        // Try name-based lookup
        for (const tool of this.toolCache.values()) {
            if (tool.name === toolIdOrName) {
                return tool;
     * Checks if a specific tool is available
     * @returns True if the tool is available
    async hasToolAvailable(toolIdOrName: string): Promise<boolean> {
        const tool = await this.getTool(toolIdOrName);
        return tool !== undefined;
     * Forces a refresh of the tool cache
    async refreshCache(): Promise<void> {
        this.lastCacheRefresh = 0;
    // ========================================
    // Private Methods
    private async refreshToolCache(): Promise<void> {
        const now = Date.now();
        if (now - this.lastCacheRefresh < this.cacheValidityMs) {
            return; // Cache is still valid
        this.toolCache.clear();
        // Get all active connection IDs
        const connectionIds = this.mcpManager.getActiveConnections();
        // Load tools from each connection
        for (const connectionId of connectionIds) {
                const result = await this.mcpManager.listTools(connectionId, {
                    contextUser: this.contextUser
                    for (const tool of result.tools) {
                        const agentTool = this.convertToAgentTool(tool, connectionId);
                        this.toolCache.set(agentTool.id, agentTool);
                // Log but continue with other connections
                console.warn(`Failed to load tools from connection ${connectionId}:`, error);
        this.lastCacheRefresh = now;
    private convertToAgentTool(
        tool: MCPToolInfo,
        connectionId: string
    ): AgentToolDefinition {
        // Create a unique ID that includes connection context
        const id = `${connectionId}__${tool.name}`;
            name: tool.name,
            title: tool.annotations?.title || tool.name,
            description: tool.description || '',
            inputSchema: tool.inputSchema || { type: 'object', properties: {} },
            connectionId,
            annotations: tool.annotations ? {
                readOnly: tool.annotations.readOnlyHint,
                destructive: tool.annotations.destructiveHint,
                idempotent: tool.annotations.idempotentHint
    private formatDescription(tool: AgentToolDefinition): string {
        let desc = tool.description;
        // Add warnings for destructive tools
        if (tool.annotations?.destructive) {
            desc = `⚠️ DESTRUCTIVE: ${desc}`;
    private async resolveToolAndConnection(
    ): Promise<{ toolName: string; connectionId: string } | null> {
        // If it looks like a full ID (contains __), parse it
        if (toolIdOrName.includes('__')) {
            const [connId, ...nameParts] = toolIdOrName.split('__');
            const toolName = nameParts.join('__');
            return { toolName, connectionId: connId };
        // If connection ID is provided, use it directly
        if (connectionId) {
            return { toolName: toolIdOrName, connectionId };
        // Find the first matching tool by name
                return { toolName: tool.name, connectionId: tool.connectionId };
    private extractErrorMessage(result: MCPToolCallResult): string {
        for (const block of result.content) {
            if (block.type === 'text' && typeof block.text === 'string') {
                return block.text;
        return 'Unknown error';
 * Creates an AgentToolAdapter instance
 * @param contextUser - User context for permissions
 * @returns New AgentToolAdapter instance
export function createAgentToolAdapter(contextUser: UserInfo): AgentToolAdapter {
    return new AgentToolAdapter(contextUser);
