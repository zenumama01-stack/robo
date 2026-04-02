__all__ = ["ResponseReasoningSummaryPartAddedEvent", "Part"]
    """The summary part that was added."""
    """The text of the summary part."""
    """The type of the summary part. Always `summary_text`."""
class ResponseReasoningSummaryPartAddedEvent(BaseModel):
    """Emitted when a new reasoning summary part is added."""
    """The ID of the item this summary part is associated with."""
    """The index of the output item this summary part is associated with."""
    summary_index: int
    """The index of the summary part within the reasoning summary."""
    type: Literal["response.reasoning_summary_part.added"]
    """The type of the event. Always `response.reasoning_summary_part.added`."""
