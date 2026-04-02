__all__ = ["McpListToolsInProgress"]
class McpListToolsInProgress(BaseModel):
    """Returned when listing MCP tools is in progress for an item."""
    type: Literal["mcp_list_tools.in_progress"]
    """The event type, must be `mcp_list_tools.in_progress`."""
