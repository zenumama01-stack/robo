__all__ = ["ResponseContentPartDoneEvent", "Part"]
class ResponseContentPartDoneEvent(BaseModel):
    """The content part that is done."""
    type: Literal["response.content_part.done"]
    """The event type, must be `response.content_part.done`."""
    Returned when a content part is done streaming in an assistant message item.
    Also emitted when a Response is interrupted, incomplete, or cancelled.
__all__ = ["ResponseContentPartDoneEvent", "Part", "PartReasoningText"]
    """Emitted when a content part is done."""
    """The index of the content part that is done."""
    """The type of the event. Always `response.content_part.done`."""
