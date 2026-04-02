__all__ = ["ResponseMcpCallFailed"]
class ResponseMcpCallFailed(BaseModel):
    """Returned when an MCP tool call has failed."""
    type: Literal["response.mcp_call.failed"]
    """The event type, must be `response.mcp_call.failed`."""
