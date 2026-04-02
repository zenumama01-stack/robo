__all__ = ["ResponseMcpListToolsFailedEvent"]
class ResponseMcpListToolsFailedEvent(BaseModel):
    """Emitted when the attempt to list available MCP tools has failed."""
    type: Literal["response.mcp_list_tools.failed"]
    """The type of the event. Always 'response.mcp_list_tools.failed'."""
