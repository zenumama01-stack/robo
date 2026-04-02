from .chat_completion_content_part_text import ChatCompletionContentPartText
from .chat_completion_content_part_image import ChatCompletionContentPartImage
__all__ = ["ChatCompletionStoreMessage", "ChatCompletionStoreMessageContentPart"]
ChatCompletionStoreMessageContentPart: TypeAlias = Union[ChatCompletionContentPartText, ChatCompletionContentPartImage]
class ChatCompletionStoreMessage(ChatCompletionMessage):
    """The identifier of the chat message."""
    content_parts: Optional[List[ChatCompletionStoreMessageContentPart]] = None
    If a content parts array was provided, this is an array of `text` and
    `image_url` parts. Otherwise, null.
