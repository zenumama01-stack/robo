 * @fileoverview MCP Resource Component
 * Resource wrapper for the MCP Dashboard that allows it to be used
 * as a nav item in applications (like the AI Application).
 * @module MCP Resource
 * MCP Resource Component
 * Wrapper that hosts the MCP Dashboard for use in application nav items.
 * Registered as 'MCPResource' for use with ResourceType: "Custom" nav items.
@RegisterClass(BaseResourceComponent, 'MCPResource')
    selector: 'mj-mcp-resource',
        <mj-mcp-dashboard></mj-mcp-dashboard>
export class MCPResourceComponent extends BaseResourceComponent implements OnInit {
        // Signal that the resource has finished loading
        // This is required for the shell's loading screen to dismiss
        return 'MCP';
        return 'fa-solid fa-plug-circle-bolt';
