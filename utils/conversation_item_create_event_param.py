from .conversation_item_param import ConversationItemParam
__all__ = ["ConversationItemCreateEventParam"]
class ConversationItemCreateEventParam(TypedDict, total=False):
    item: Required[ConversationItemParam]
    type: Required[Literal["conversation.item.create"]]
    previous_item_id: str
