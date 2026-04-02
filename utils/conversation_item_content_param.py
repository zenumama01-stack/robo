__all__ = ["ConversationItemContentParam"]
class ConversationItemContentParam(TypedDict, total=False):
    audio: str
    transcript: str
    type: Literal["input_text", "input_audio", "item_reference", "text", "audio"]
