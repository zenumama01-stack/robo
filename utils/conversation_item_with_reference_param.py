__all__ = ["ConversationItemWithReferenceParam", "Content"]
class Content(TypedDict, total=False):
    type: Literal["input_text", "input_audio", "item_reference", "text"]
class ConversationItemWithReferenceParam(TypedDict, total=False):
    content: Iterable[Content]
    type: Literal["message", "function_call", "function_call_output", "item_reference"]
