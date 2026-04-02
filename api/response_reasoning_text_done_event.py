__all__ = ["ResponseReasoningTextDoneEvent"]
class ResponseReasoningTextDoneEvent(BaseModel):
    """Emitted when a reasoning text is completed."""
    """The index of the reasoning content part."""
    """The ID of the item this reasoning text is associated with."""
    """The index of the output item this reasoning text is associated with."""
    """The full text of the completed reasoning content."""
    type: Literal["response.reasoning_text.done"]
    """The type of the event. Always `response.reasoning_text.done`."""
