__all__ = ["RealtimeMcpToolExecutionError"]
class RealtimeMcpToolExecutionError(BaseModel):
    type: Literal["tool_execution_error"]
