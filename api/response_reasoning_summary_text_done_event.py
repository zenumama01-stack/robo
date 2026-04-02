__all__ = ["ResponseReasoningSummaryTextDoneEvent"]
class ResponseReasoningSummaryTextDoneEvent(BaseModel):
    """Emitted when a reasoning summary text is completed."""
    """The ID of the item this summary text is associated with."""
    """The index of the output item this summary text is associated with."""
    """The full text of the completed reasoning summary."""
    type: Literal["response.reasoning_summary_text.done"]
    """The type of the event. Always `response.reasoning_summary_text.done`."""
