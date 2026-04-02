from .tool_call import ToolCall
__all__ = ["ToolCallsStepDetails"]
class ToolCallsStepDetails(BaseModel):
    tool_calls: List[ToolCall]
