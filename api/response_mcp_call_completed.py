__all__ = ["ResponseMcpCallCompleted"]
class ResponseMcpCallCompleted(BaseModel):
    """Returned when an MCP tool call has completed successfully."""
    type: Literal["response.mcp_call.completed"]
    """The event type, must be `response.mcp_call.completed`."""
