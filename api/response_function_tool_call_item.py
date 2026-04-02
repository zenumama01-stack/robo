__all__ = ["ResponseFunctionToolCallItem"]
class ResponseFunctionToolCallItem(ResponseFunctionToolCall):
    status: Literal["in_progress", "completed", "incomplete"]  # type: ignore
