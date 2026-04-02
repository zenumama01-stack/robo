__all__ = ["ResponseMcpCallCompletedEvent"]
class ResponseMcpCallCompletedEvent(BaseModel):
    """Emitted when an MCP  tool call has completed successfully."""
    """The ID of the MCP tool call item that completed."""
    """The index of the output item that completed."""
    """The type of the event. Always 'response.mcp_call.completed'."""
