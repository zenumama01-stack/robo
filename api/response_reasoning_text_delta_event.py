__all__ = ["ResponseReasoningTextDeltaEvent"]
class ResponseReasoningTextDeltaEvent(BaseModel):
    """Emitted when a delta is added to a reasoning text."""
    """The index of the reasoning content part this delta is associated with."""
    """The text delta that was added to the reasoning content."""
    """The ID of the item this reasoning text delta is associated with."""
    """The index of the output item this reasoning text delta is associated with."""
    type: Literal["response.reasoning_text.delta"]
    """The type of the event. Always `response.reasoning_text.delta`."""
