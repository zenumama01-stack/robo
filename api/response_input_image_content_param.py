__all__ = ["ResponseInputImageContentParam"]
class ResponseInputImageContentParam(TypedDict, total=False):
    detail: Optional[Literal["low", "high", "auto", "original"]]
    image_url: Optional[str]
    detail: Optional[Literal["low", "high", "auto"]]
