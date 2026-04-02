__all__ = ["RealtimeMcphttpErrorParam"]
class RealtimeMcphttpErrorParam(TypedDict, total=False):
    type: Required[Literal["http_error"]]
