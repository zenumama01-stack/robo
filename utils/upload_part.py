__all__ = ["UploadPart"]
class UploadPart(BaseModel):
    """The upload Part represents a chunk of bytes we can add to an Upload object."""
    """The upload Part unique identifier, which can be referenced in API endpoints."""
    """The Unix timestamp (in seconds) for when the Part was created."""
    object: Literal["upload.part"]
    """The object type, which is always `upload.part`."""
    upload_id: str
    """The ID of the Upload object that this Part was added to."""
