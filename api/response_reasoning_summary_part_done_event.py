__all__ = ["ResponseReasoningSummaryPartDoneEvent", "Part"]
    """The completed summary part."""
class ResponseReasoningSummaryPartDoneEvent(BaseModel):
    """Emitted when a reasoning summary part is completed."""
    type: Literal["response.reasoning_summary_part.done"]
    """The type of the event. Always `response.reasoning_summary_part.done`."""
