__all__ = ["ResponseToolSearchOutputItem"]
class ResponseToolSearchOutputItem(BaseModel):
    """The unique ID of the tool search output item."""
    """The status of the tool search output item that was recorded."""
    """The loaded tool definitions returned by tool search."""
    type: Literal["tool_search_output"]
    """The type of the item. Always `tool_search_output`."""
