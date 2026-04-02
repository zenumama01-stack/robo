__all__ = ["McpListToolsFailed"]
class McpListToolsFailed(BaseModel):
    """Returned when listing MCP tools has failed for an item."""
    type: Literal["mcp_list_tools.failed"]
    """The event type, must be `mcp_list_tools.failed`."""
