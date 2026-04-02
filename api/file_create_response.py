__all__ = ["FileCreateResponse"]
class FileCreateResponse(BaseModel):
    """Unique identifier for the file."""
    """Size of the file in bytes."""
    container_id: str
    """The container this file belongs to."""
    """Unix timestamp (in seconds) when the file was created."""
    object: Literal["container.file"]
    """The type of this object (`container.file`)."""
    path: str
    """Path of the file in the container."""
    source: str
    """Source of the file (e.g., `user`, `assistant`)."""
