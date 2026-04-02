from .chatkit_response_output_text import ChatKitResponseOutputText
__all__ = ["ChatKitThreadAssistantMessageItem"]
class ChatKitThreadAssistantMessageItem(BaseModel):
    """Assistant-authored message within a thread."""
    """Identifier of the thread item."""
    content: List[ChatKitResponseOutputText]
    """Ordered assistant response segments."""
    """Unix timestamp (in seconds) for when the item was created."""
    object: Literal["chatkit.thread_item"]
    """Type discriminator that is always `chatkit.thread_item`."""
    thread_id: str
    """Identifier of the parent thread."""
    type: Literal["chatkit.assistant_message"]
    """Type discriminator that is always `chatkit.assistant_message`."""
