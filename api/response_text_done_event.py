__all__ = ["ResponseTextDoneEvent"]
class ResponseTextDoneEvent(BaseModel):
    """The final text content."""
    type: Literal["response.text.done"]
    """The event type, must be `response.text.done`."""
    """Returned when the text value of an "output_text" content part is done streaming.
    Also
    emitted when a Response is interrupted, incomplete, or cancelled.
    type: Literal["response.output_text.done"]
    """The event type, must be `response.output_text.done`."""
__all__ = ["ResponseTextDoneEvent", "Logprob", "LogprobTopLogprob"]
    """Emitted when text content is finalized."""
    """The index of the content part that the text content is finalized."""
    """The ID of the output item that the text content is finalized."""
    """The index of the output item that the text content is finalized."""
    """The text content that is finalized."""
    """The type of the event. Always `response.output_text.done`."""
