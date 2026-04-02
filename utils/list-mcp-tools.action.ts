 * @fileoverview Action to list available tools from an MCP server
 * This action retrieves the list of tools available from an MCP server
 * connection, useful for discovery and documentation purposes.
 * @module @memberjunction/actions/mcp/list-mcp-tools
 * Action to list available tools from an MCP server.
 * This action connects to an MCP server and retrieves the list of
 * available tools with their names, descriptions, and input schemas.
 *   ActionName: 'List MCP Tools',
 *     { Name: 'ConnectionID', Value: 'connection-uuid' }
 * - `ConnectionID` (required): UUID of the MCP Server Connection to query
 * - Success: Tool list in Message as JSON
 * - Output params include: `Tools`, `ToolCount`
@RegisterClass(BaseAction, "List MCP Tools")
export class ListMCPToolsAction extends BaseAction {
            // Ensure we're connected
                    skipAutoSync: true
            // List tools
            const result = await manager.listTools(connectionId, {
                contextUser: params.ContextUser
            this.addOutputParam(params, 'Tools', result.tools);
            this.addOutputParam(params, 'ToolCount', result.tools.length);
                    ResultCode: 'LIST_FAILED',
                    Message: result.error ?? 'Failed to list tools',
            // Format output message with tool summary
            const toolSummary = result.tools.map(t => ({
                name: t.name,
                description: t.description ?? '(No description)'
            const message = JSON.stringify({
                count: result.tools.length,
                tools: toolSummary
            LogError(`[ListMCPToolsAction] Error: ${error}`);
