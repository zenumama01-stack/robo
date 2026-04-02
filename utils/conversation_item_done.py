__all__ = ["ConversationItemDone"]
class ConversationItemDone(BaseModel):
    """Returned when a conversation item is finalized.
    The event will include the full content of the Item except for audio data, which can be retrieved separately with a `conversation.item.retrieve` event if needed.
    type: Literal["conversation.item.done"]
    """The event type, must be `conversation.item.done`."""
