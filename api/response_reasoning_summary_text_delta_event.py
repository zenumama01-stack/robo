__all__ = ["ResponseReasoningSummaryTextDeltaEvent"]
class ResponseReasoningSummaryTextDeltaEvent(BaseModel):
    """Emitted when a delta is added to a reasoning summary text."""
    """The text delta that was added to the summary."""
    """The ID of the item this summary text delta is associated with."""
    """The index of the output item this summary text delta is associated with."""
    type: Literal["response.reasoning_summary_text.delta"]
    """The type of the event. Always `response.reasoning_summary_text.delta`."""
