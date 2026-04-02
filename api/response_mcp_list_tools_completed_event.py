__all__ = ["ResponseMcpListToolsCompletedEvent"]
class ResponseMcpListToolsCompletedEvent(BaseModel):
    """Emitted when the list of available MCP tools has been successfully retrieved."""
    """The ID of the MCP tool call item that produced this output."""
    """The index of the output item that was processed."""
    type: Literal["response.mcp_list_tools.completed"]
    """The type of the event. Always 'response.mcp_list_tools.completed'."""
