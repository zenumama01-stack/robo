__all__ = ["ConversationItemRetrieveEventParam"]
class ConversationItemRetrieveEventParam(TypedDict, total=False):
    type: Required[Literal["conversation.item.retrieve"]]
