__all__ = ["ConversationItemTruncateEvent"]
class ConversationItemTruncateEvent(BaseModel):
    audio_end_ms: int
    """Inclusive duration up to which audio is truncated, in milliseconds.
    If the audio_end_ms is greater than the actual audio duration, the server will
    respond with an error.
    """The index of the content part to truncate. Set this to 0."""
    """The ID of the assistant message item to truncate.
    Only assistant message items can be truncated.
    type: Literal["conversation.item.truncate"]
    """The event type, must be `conversation.item.truncate`."""
    """The index of the content part to truncate. Set this to `0`."""
