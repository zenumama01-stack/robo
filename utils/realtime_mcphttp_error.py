__all__ = ["RealtimeMcphttpError"]
class RealtimeMcphttpError(BaseModel):
    type: Literal["http_error"]
