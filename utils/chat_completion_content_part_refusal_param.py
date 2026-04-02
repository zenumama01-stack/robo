__all__ = ["ChatCompletionContentPartRefusalParam"]
class ChatCompletionContentPartRefusalParam(TypedDict, total=False):
    refusal: Required[str]
    type: Required[Literal["refusal"]]
