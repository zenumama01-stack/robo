__all__ = ["RefusalDeltaBlock"]
class RefusalDeltaBlock(BaseModel):
    """The refusal content that is part of a message."""
    """The index of the refusal part in the message."""
    refusal: Optional[str] = None
