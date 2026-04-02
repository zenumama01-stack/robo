__all__ = ["RealtimeConversationItemFunctionCallOutputParam"]
class RealtimeConversationItemFunctionCallOutputParam(TypedDict, total=False):
    call_id: Required[str]
    output: Required[str]
    type: Required[Literal["function_call_output"]]
