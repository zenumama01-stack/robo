__all__ = ["ConversationItemDeletedEvent"]
class ConversationItemDeletedEvent(BaseModel):
    """The ID of the item that was deleted."""
    type: Literal["conversation.item.deleted"]
    """The event type, must be `conversation.item.deleted`."""
    Returned when an item in the conversation is deleted by the client with a
    `conversation.item.delete` event. This event is used to synchronize the
    server's understanding of the conversation history with the client's view.
