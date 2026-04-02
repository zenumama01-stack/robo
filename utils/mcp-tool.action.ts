 * @fileoverview Generic driver class for MCP Tool Actions
 * This class handles execution of auto-generated Actions that are linked to
 * MCP Server Tools via the GeneratedActionID field. When an MCP tool is synced
 * to an Action (under System/AI/MCP/{ServerName}), this driver class is used
 * to execute it.
 * The class:
 * 1. Looks up the MCPServerTool using params.Action.ID (matching GeneratedActionID)
 * 2. Finds an active connection for the tool's server
 * 3. Maps Action parameters to tool arguments
 * 4. Calls the tool via MCPClientManager
 * @module @memberjunction/actions/mcp/mcp-tool
import { LogError, LogStatus, RunView } from "@memberjunction/core";
import { MJMCPServerToolEntity, MJMCPServerConnectionEntity } from "@memberjunction/core-entities";
 * Generic driver class for all auto-generated MCP Tool Actions.
 * When an MCP Server Tool is synced to an Action (via syncActionsForServer),
 * the Action's DriverClass is set to 'MCPToolAction'. This class handles
 * execution of all such actions by:
 * 1. Looking up the MCPServerTool entity that has GeneratedActionID = Action.ID
 * 2. Finding an active connection for the tool's server
 * 3. Mapping ActionParams to tool arguments (using param Name as key)
 * 4. Calling the MCP tool and returning the result
 * // An Action with DriverClass='MCPToolAction' will be handled by this class
 * // The tool is identified by looking up MCPServerTool.GeneratedActionID = Action.ID
 *   ActionName: 'search_documentation',  // Name matches ToolName from MCP Server
 *     { Name: 'query', Value: 'MemberJunction API' },
 *     { Name: 'maxResults', Value: 10 }
 * **Optional Parameter:**
 * - `ConnectionID` (optional): Specific connection to use. If not provided,
 *   an active connection for the server will be automatically selected.
@RegisterClass(BaseAction, "MCPToolAction")
export class MCPToolAction extends BaseAction {
            const actionId = params.Action.ID;
            const actionName = params.Action.Name;
            LogStatus(`[MCPToolAction] Executing action '${actionName}' (ID: ${actionId})`);
            // Step 1: Look up the MCPServerTool by GeneratedActionID
            const tool = await this.lookupMCPServerTool(actionId, params);
            if (!tool) {
                    ResultCode: 'TOOL_NOT_FOUND',
                    Message: `Could not find MCP Server Tool linked to action '${actionName}' (ID: ${actionId}). ` +
                             `Ensure the tool has been synced with GeneratedActionID set.`
            LogStatus(`[MCPToolAction] Found tool '${tool.ToolName}' on server '${tool.MCPServer}'`);
            // Step 2: Get the connection (either from params or find an active one)
            const connectionId = await this.resolveConnectionId(tool, params);
                    ResultCode: 'NO_CONNECTION',
                    Message: `No active connection available for MCP Server '${tool.MCPServer}'. ` +
                             `Please create an active connection or provide a ConnectionID parameter.`
            // Step 3: Build arguments from ActionParams
            const toolArguments = this.buildToolArguments(params);
            LogStatus(`[MCPToolAction] Calling tool '${tool.ToolName}' via connection '${connectionId}'`);
            // Step 4: Get the MCP Client Manager and ensure connected
            // Step 5: Call the tool
                tool.ToolName,
                    arguments: toolArguments,
            // Step 6: Add output parameters
            this.addOutputParam(params, 'ToolName', tool.ToolName);
            this.addOutputParam(params, 'ServerName', tool.MCPServer);
                    Message: result.error ?? `Tool '${tool.ToolName}' execution failed`,
            LogError(`[MCPToolAction] Error executing action '${params.Action?.Name}': ${error}`);
     * Looks up the MCPServerTool entity that has GeneratedActionID matching the action ID.
    private async lookupMCPServerTool(
    ): Promise<MJMCPServerToolEntity | null> {
        const result = await rv.RunView<MJMCPServerToolEntity>({
            EntityName: 'MJ: MCP Server Tools',
            ExtraFilter: `GeneratedActionID='${actionId}'`,
     * Resolves the connection ID to use for the tool execution.
     * First checks for an explicit ConnectionID parameter, then finds an active connection.
    private async resolveConnectionId(
        // Check for explicit ConnectionID parameter
        const explicitConnectionId = this.getStringParam(params, 'ConnectionID');
        if (explicitConnectionId) {
            return explicitConnectionId;
        // Find an active connection for the tool's server
        const result = await rv.RunView<MJMCPServerConnectionEntity>({
            ExtraFilter: `MCPServerID='${tool.MCPServerID}' AND Status='Active'`,
            OrderBy: '__mj_CreatedAt ASC',  // Use oldest connection as default
     * Builds the tool arguments object from ActionParams.
     * Excludes system parameters like ConnectionID and Timeout.
    private buildToolArguments(params: RunActionParams): Record<string, unknown> {
        const systemParams = new Set(['connectionid', 'timeout']);
        const args: Record<string, unknown> = {};
        for (const param of params.Params) {
            // Skip output params and system params
            if (param.Type === 'Output' || systemParams.has(param.Name.toLowerCase())) {
            // Add to arguments using original param name
            if (param.Value !== undefined && param.Value !== null) {
                args[param.Name] = param.Value;
