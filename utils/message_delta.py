from .message_content_delta import MessageContentDelta
__all__ = ["MessageDelta"]
class MessageDelta(BaseModel):
    """The delta containing the fields that have changed on the Message."""
    content: Optional[List[MessageContentDelta]] = None
    role: Optional[Literal["user", "assistant"]] = None
