__all__ = ["RealtimeConversationItemAssistantMessageParam", "Content"]
    type: Literal["output_text", "output_audio"]
class RealtimeConversationItemAssistantMessageParam(TypedDict, total=False):
    content: Required[Iterable[Content]]
    type: Required[Literal["message"]]
