__all__ = ["ResponseMcpListToolsInProgressEvent"]
class ResponseMcpListToolsInProgressEvent(BaseModel):
    Emitted when the system is in the process of retrieving the list of available MCP tools.
    """The ID of the MCP tool call item that is being processed."""
    """The index of the output item that is being processed."""
    type: Literal["response.mcp_list_tools.in_progress"]
    """The type of the event. Always 'response.mcp_list_tools.in_progress'."""
