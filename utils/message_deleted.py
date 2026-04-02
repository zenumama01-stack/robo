__all__ = ["MessageDeleted"]
class MessageDeleted(BaseModel):
    object: Literal["thread.message.deleted"]
