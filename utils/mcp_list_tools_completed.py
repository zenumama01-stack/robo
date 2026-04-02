__all__ = ["McpListToolsCompleted"]
class McpListToolsCompleted(BaseModel):
    """Returned when listing MCP tools has completed for an item."""
    """The ID of the MCP list tools item."""
    type: Literal["mcp_list_tools.completed"]
    """The event type, must be `mcp_list_tools.completed`."""
