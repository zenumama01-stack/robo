__all__ = ["ResponseCancelEventParam"]
class ResponseCancelEventParam(TypedDict, total=False):
    type: Required[Literal["response.cancel"]]
