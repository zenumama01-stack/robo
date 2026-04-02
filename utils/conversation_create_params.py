from ..responses.response_input_item_param import ResponseInputItemParam
__all__ = ["ConversationCreateParams"]
class ConversationCreateParams(TypedDict, total=False):
    items: Optional[Iterable[ResponseInputItemParam]]
    """Initial items to include in the conversation context.
    You may add up to 20 items at a time.
