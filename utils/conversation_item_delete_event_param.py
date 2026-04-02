__all__ = ["ConversationItemDeleteEventParam"]
class ConversationItemDeleteEventParam(TypedDict, total=False):
    item_id: Required[str]
    type: Required[Literal["conversation.item.delete"]]
