__all__ = ["ResponseInputImageParam"]
class ResponseInputImageParam(TypedDict, total=False):
    detail: Required[Literal["low", "high", "auto", "original"]]
    detail: Required[Literal["low", "high", "auto"]]
