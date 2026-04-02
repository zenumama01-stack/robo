__all__ = ["ResponseCustomToolCall"]
class ResponseCustomToolCall(BaseModel):
    """An identifier used to map this custom tool call to a tool call output."""
    """The name of the custom tool being called."""
    type: Literal["custom_tool_call"]
    """The type of the custom tool call. Always `custom_tool_call`."""
    """The unique ID of the custom tool call in the OpenAI platform."""
    namespace: Optional[str] = None
    """The namespace of the custom tool being called."""
