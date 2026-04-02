__all__ = ["ResponseCustomToolCallItem"]
class ResponseCustomToolCallItem(ResponseCustomToolCall):
    id: str  # type: ignore
    """The unique ID of the custom tool call item."""
