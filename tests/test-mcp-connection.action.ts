 * @fileoverview Action to test an MCP server connection
 * This action verifies connectivity to an MCP server and retrieves
 * server information including name, version, and capabilities.
 * @module @memberjunction/actions/mcp/test-mcp-connection
 * Action to test an MCP server connection.
 * This action attempts to connect to an MCP server and validates
 * the connection is working. It returns server information and latency.
 *   ActionName: 'Test MCP Connection',
 * - `ConnectionID` (required): UUID of the MCP Server Connection to test
 * - Success: Connection details in Message
 * - Output params include: `ServerName`, `ServerVersion`, `LatencyMs`, `Capabilities`
@RegisterClass(BaseAction, "Test MCP Connection")
export class TestMCPConnectionAction extends BaseAction {
            // Test connection
            const result = await manager.testConnection(connectionId, {
            this.addOutputParam(params, 'ServerName', result.serverName);
            this.addOutputParam(params, 'ServerVersion', result.serverVersion);
            this.addOutputParam(params, 'LatencyMs', result.latencyMs);
            this.addOutputParam(params, 'Capabilities', result.capabilities);
                    ResultCode: 'CONNECTION_FAILED',
                    Message: result.error ?? 'Connection test failed',
            const serverInfo = result.serverName
                ? `${result.serverName}${result.serverVersion ? ` v${result.serverVersion}` : ''}`
                : 'Unknown server';
            const message = `Connection successful to ${serverInfo}. Latency: ${result.latencyMs}ms`;
            LogError(`[TestMCPConnectionAction] Error: ${error}`);
