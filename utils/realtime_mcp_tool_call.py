from .realtime_mcphttp_error import RealtimeMcphttpError
from .realtime_mcp_protocol_error import RealtimeMcpProtocolError
from .realtime_mcp_tool_execution_error import RealtimeMcpToolExecutionError
__all__ = ["RealtimeMcpToolCall", "Error"]
Error: TypeAlias = Annotated[
    Union[RealtimeMcpProtocolError, RealtimeMcpToolExecutionError, RealtimeMcphttpError, None],
class RealtimeMcpToolCall(BaseModel):
    """A Realtime item representing an invocation of a tool on an MCP server."""
    """The ID of an associated approval request, if any."""
