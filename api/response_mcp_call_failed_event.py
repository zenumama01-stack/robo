__all__ = ["ResponseMcpCallFailedEvent"]
class ResponseMcpCallFailedEvent(BaseModel):
    """Emitted when an MCP  tool call has failed."""
    """The ID of the MCP tool call item that failed."""
    """The index of the output item that failed."""
    """The type of the event. Always 'response.mcp_call.failed'."""
