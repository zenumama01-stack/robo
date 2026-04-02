__all__ = ["ResponseCustomToolCallInputDoneEvent"]
class ResponseCustomToolCallInputDoneEvent(BaseModel):
    """Event indicating that input for a custom tool call is complete."""
    """The complete input data for the custom tool call."""
    """The index of the output this event applies to."""
    type: Literal["response.custom_tool_call_input.done"]
