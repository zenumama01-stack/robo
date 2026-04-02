__all__ = ["ConversationItemTruncateEventParam"]
class ConversationItemTruncateEventParam(TypedDict, total=False):
    audio_end_ms: Required[int]
    content_index: Required[int]
    type: Required[Literal["conversation.item.truncate"]]
