__all__ = ["ChatCompletionDeleted"]
class ChatCompletionDeleted(BaseModel):
    """The ID of the chat completion that was deleted."""
    """Whether the chat completion was deleted."""
    object: Literal["chat.completion.deleted"]
    """The type of object being deleted."""
