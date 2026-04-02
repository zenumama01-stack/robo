__all__ = ["ConversationItemList"]
class ConversationItemList(BaseModel):
    """A list of Conversation items."""
    data: List[ConversationItem]
    """A list of conversation items."""
    first_id: str
    last_id: str
