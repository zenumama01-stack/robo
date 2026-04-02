 * @fileoverview Action to synchronize tools from an MCP server to the database
 * This action discovers tools from an MCP server and updates the local
 * tool definitions in the database for discoverability and management.
 * @module @memberjunction/actions/mcp/sync-mcp-tools
 * Action to synchronize MCP server tools to the database.
 * This action connects to an MCP server, lists all available tools,
 * and updates the local database cache. New tools are added, existing
 * tools are updated, and tools no longer available are marked deprecated.
 *   ActionName: 'Sync MCP Tools',
 * - `ConnectionID` (required): UUID of the MCP Server Connection to sync
 * - Success: Sync summary in Message
 * - Output params include: `Added`, `Updated`, `Deprecated`, `Total`
@RegisterClass(BaseAction, "Sync MCP Tools")
export class SyncMCPToolsAction extends BaseAction {
                    skipAutoSync: true // We'll sync manually
            // Sync tools
            const result = await manager.syncTools(connectionId, {
            this.addOutputParam(params, 'Added', result.added);
            this.addOutputParam(params, 'Updated', result.updated);
            this.addOutputParam(params, 'Deprecated', result.deprecated);
            this.addOutputParam(params, 'Total', result.total);
                    ResultCode: 'SYNC_FAILED',
                    Message: result.error ?? 'Tool synchronization failed',
            const message = `Tool sync completed: ${result.added} added, ${result.updated} updated, ${result.deprecated} deprecated. Total: ${result.total} tools.`;
            LogError(`[SyncMCPToolsAction] Error: ${error}`);
