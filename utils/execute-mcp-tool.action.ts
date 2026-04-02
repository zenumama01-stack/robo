 * @fileoverview Action to execute a tool on an MCP server
 * This action provides a workflow/agent interface for invoking tools
 * on external MCP servers via MCPClientManager.
 * @module @memberjunction/actions/mcp/execute-mcp-tool
import { MCPClientManager } from "@memberjunction/ai-mcp-client";
 * Action to execute a tool on an MCP server.
 * This action connects to an MCP server (if not already connected),
 * calls the specified tool with provided arguments, and returns the result.
 *   ActionName: 'Execute MCP Tool',
 *     { Name: 'ConnectionID', Value: 'connection-uuid' },
 *     { Name: 'ToolName', Value: 'search_documents' },
 *     { Name: 'Arguments', Value: { query: 'MJ documentation', maxResults: 10 } }
 * **Parameters:**
 * - `ConnectionID` (required): UUID of the MCP Server Connection to use
 * - `ToolName` (required): Name of the tool to execute
 * - `Arguments` (optional): Object with tool input parameters
 * - `Timeout` (optional): Request timeout in milliseconds (default: 60000)
 * **Result:**
 * - Success: Tool output in Message as JSON
 * - Output params include: `ToolOutput`, `DurationMs`, `IsToolError`
@RegisterClass(BaseAction, "Execute MCP Tool")
export class ExecuteMCPToolAction extends BaseAction {
            const connectionId = this.getStringParam(params, 'ConnectionID');
            const toolName = this.getStringParam(params, 'ToolName');
            const arguments_ = this.getObjectParam(params, 'Arguments') ?? {};
            const timeout = this.getNumericParam(params, 'Timeout', 60000);
            if (!connectionId) {
                    Message: "Parameter 'ConnectionID' is required"
            if (!toolName) {
                    Message: "Parameter 'ToolName' is required"
            // Get the MCP Client Manager
            const manager = MCPClientManager.Instance;
            // Ensure we're connected (will connect if not already)
            if (!manager.isConnected(connectionId)) {
                await manager.connect(connectionId, {
                    contextUser: params.ContextUser,
                    skipAutoSync: true // Don't auto-sync when just calling tools
            const result = await manager.callTool(
                    arguments: arguments_ as Record<string, unknown>,
                { contextUser: params.ContextUser }
            this.addOutputParam(params, 'ToolOutput', result.content);
            this.addOutputParam(params, 'StructuredOutput', result.structuredContent);
            this.addOutputParam(params, 'DurationMs', result.durationMs);
            this.addOutputParam(params, 'IsToolError', result.isToolError);
                    ResultCode: result.isToolError ? 'TOOL_ERROR' : 'EXECUTION_FAILED',
                    Message: result.error ?? 'Tool execution failed',
            // Format output message
            const output = result.structuredContent ?? result.content;
            const message = typeof output === 'string' ? output : JSON.stringify(output, null, 2);
            LogError(`[ExecuteMCPToolAction] Error: ${error}`);
    // Helper methods for parameter extraction
        return value.length > 0 ? value : undefined;
    private getObjectParam(params: RunActionParams, name: string): Record<string, unknown> | undefined {
        // Handle both JSON string and object input
                return JSON.parse(param.Value);
        if (typeof param.Value === 'object') {
            return param.Value as Record<string, unknown>;
