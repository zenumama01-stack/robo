__all__ = ["RealtimeMcpProtocolError"]
class RealtimeMcpProtocolError(BaseModel):
    code: int
    type: Literal["protocol_error"]
