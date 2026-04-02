__all__ = ["ResponseMcpCallArgumentsDone"]
class ResponseMcpCallArgumentsDone(BaseModel):
    """Returned when MCP tool call arguments are finalized during response generation."""
    """The final JSON-encoded arguments string."""
    type: Literal["response.mcp_call_arguments.done"]
    """The event type, must be `response.mcp_call_arguments.done`."""
