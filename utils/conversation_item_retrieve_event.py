__all__ = ["ConversationItemRetrieveEvent"]
class ConversationItemRetrieveEvent(BaseModel):
    """The ID of the item to retrieve."""
    type: Literal["conversation.item.retrieve"]
    """The event type, must be `conversation.item.retrieve`."""
