__all__ = ["ConversationItemDeleteEvent"]
class ConversationItemDeleteEvent(BaseModel):
    item_id: str
    """The ID of the item to delete."""
    type: Literal["conversation.item.delete"]
    """The event type, must be `conversation.item.delete`."""
