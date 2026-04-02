__all__ = ["ResponseInputTextContentParam"]
class ResponseInputTextContentParam(TypedDict, total=False):
    type: Required[Literal["input_text"]]
