__all__ = ["ThreadDeleted"]
class ThreadDeleted(BaseModel):
    object: Literal["thread.deleted"]
