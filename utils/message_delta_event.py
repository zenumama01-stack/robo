from .message_delta import MessageDelta
__all__ = ["MessageDeltaEvent"]
class MessageDeltaEvent(BaseModel):
    """The identifier of the message, which can be referenced in API endpoints."""
    delta: MessageDelta
    object: Literal["thread.message.delta"]
    """The object type, which is always `thread.message.delta`."""
