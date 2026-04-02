__all__ = ["ThreadDeleteResponse"]
class ThreadDeleteResponse(BaseModel):
    """Confirmation payload returned after deleting a thread."""
    """Identifier of the deleted thread."""
    """Indicates that the thread has been deleted."""
    object: Literal["chatkit.thread.deleted"]
    """Type discriminator that is always `chatkit.thread.deleted`."""
