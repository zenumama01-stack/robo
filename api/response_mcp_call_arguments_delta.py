__all__ = ["ResponseMcpCallArgumentsDelta"]
class ResponseMcpCallArgumentsDelta(BaseModel):
    """Returned when MCP tool call arguments are updated during response generation."""
    """The JSON-encoded arguments delta."""
    """The ID of the MCP tool call item."""
    type: Literal["response.mcp_call_arguments.delta"]
    """The event type, must be `response.mcp_call_arguments.delta`."""
    obfuscation: Optional[str] = None
    """If present, indicates the delta text was obfuscated."""
