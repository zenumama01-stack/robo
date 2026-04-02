__all__ = ["ResponseReasoningItemParam", "Summary", "Content"]
class Summary(TypedDict, total=False):
    type: Required[Literal["summary_text"]]
    type: Required[Literal["reasoning_text"]]
class ResponseReasoningItemParam(TypedDict, total=False):
    summary: Required[Iterable[Summary]]
    type: Required[Literal["reasoning"]]
    encrypted_content: Optional[str]
