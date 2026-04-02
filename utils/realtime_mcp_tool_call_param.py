from .realtime_mcphttp_error_param import RealtimeMcphttpErrorParam
from .realtime_mcp_protocol_error_param import RealtimeMcpProtocolErrorParam
from .realtime_mcp_tool_execution_error_param import RealtimeMcpToolExecutionErrorParam
__all__ = ["RealtimeMcpToolCallParam", "Error"]
Error: TypeAlias = Union[RealtimeMcpProtocolErrorParam, RealtimeMcpToolExecutionErrorParam, RealtimeMcphttpErrorParam]
class RealtimeMcpToolCallParam(TypedDict, total=False):
    type: Required[Literal["mcp_call"]]
    approval_request_id: Optional[str]
    error: Optional[Error]
    output: Optional[str]
