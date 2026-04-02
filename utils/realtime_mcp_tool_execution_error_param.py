__all__ = ["RealtimeMcpToolExecutionErrorParam"]
class RealtimeMcpToolExecutionErrorParam(TypedDict, total=False):
    type: Required[Literal["tool_execution_error"]]
