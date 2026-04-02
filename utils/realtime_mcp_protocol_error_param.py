__all__ = ["RealtimeMcpProtocolErrorParam"]
class RealtimeMcpProtocolErrorParam(TypedDict, total=False):
    code: Required[int]
    message: Required[str]
    type: Required[Literal["protocol_error"]]
