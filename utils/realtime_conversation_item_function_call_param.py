__all__ = ["RealtimeConversationItemFunctionCallParam"]
class RealtimeConversationItemFunctionCallParam(TypedDict, total=False):
    type: Required[Literal["function_call"]]
