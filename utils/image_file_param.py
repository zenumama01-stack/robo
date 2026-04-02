__all__ = ["ImageFileParam"]
class ImageFileParam(TypedDict, total=False):
    file_id: Required[str]
    detail: Literal["auto", "low", "high"]
