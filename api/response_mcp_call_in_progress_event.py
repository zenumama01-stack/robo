__all__ = ["ResponseMcpCallInProgressEvent"]
class ResponseMcpCallInProgressEvent(BaseModel):
    """Emitted when an MCP  tool call is in progress."""
    """The type of the event. Always 'response.mcp_call.in_progress'."""
