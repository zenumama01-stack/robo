__all__ = ["ConversationCreatedEvent", "Conversation"]
class Conversation(BaseModel):
    """The unique ID of the conversation."""
    object: Optional[Literal["realtime.conversation"]] = None
    """The object type, must be `realtime.conversation`."""
class ConversationCreatedEvent(BaseModel):
    conversation: Conversation
    """The conversation resource."""
    event_id: str
    """The unique ID of the server event."""
    type: Literal["conversation.created"]
    """The event type, must be `conversation.created`."""
    """Returned when a conversation is created. Emitted right after session creation."""
