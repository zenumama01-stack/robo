__all__ = ["ResponseMcpCallArgumentsDeltaEvent"]
class ResponseMcpCallArgumentsDeltaEvent(BaseModel):
    Emitted when there is a delta (partial update) to the arguments of an MCP tool call.
    A JSON string containing the partial update to the arguments for the MCP tool
    """The unique identifier of the MCP tool call item being processed."""
    """The type of the event. Always 'response.mcp_call_arguments.delta'."""
