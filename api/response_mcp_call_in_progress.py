__all__ = ["ResponseMcpCallInProgress"]
class ResponseMcpCallInProgress(BaseModel):
    """Returned when an MCP tool call has started and is in progress."""
    type: Literal["response.mcp_call.in_progress"]
    """The event type, must be `response.mcp_call.in_progress`."""
