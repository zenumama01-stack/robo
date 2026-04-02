__all__ = ["ResponseCustomToolCallInputDeltaEvent"]
class ResponseCustomToolCallInputDeltaEvent(BaseModel):
    """Event representing a delta (partial update) to the input of a custom tool call."""
    """The incremental input data (delta) for the custom tool call."""
    """Unique identifier for the API item associated with this event."""
    """The index of the output this delta applies to."""
    type: Literal["response.custom_tool_call_input.delta"]
    """The event type identifier."""
