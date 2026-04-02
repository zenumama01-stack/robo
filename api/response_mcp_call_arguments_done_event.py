__all__ = ["ResponseMcpCallArgumentsDoneEvent"]
class ResponseMcpCallArgumentsDoneEvent(BaseModel):
    """Emitted when the arguments for an MCP tool call are finalized."""
    """A JSON string containing the finalized arguments for the MCP tool call."""
    """The type of the event. Always 'response.mcp_call_arguments.done'."""
