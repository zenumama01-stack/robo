__all__ = ["VideoDeleteResponse"]
class VideoDeleteResponse(BaseModel):
    """Confirmation payload returned after deleting a video."""
    """Identifier of the deleted video."""
    """Indicates that the video resource was deleted."""
    object: Literal["video.deleted"]
    """The object type that signals the deletion response."""
