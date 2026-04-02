__all__ = ["VideoCreateError"]
class VideoCreateError(BaseModel):
    """An error that occurred while generating the response."""
    code: str
    """A machine-readable error code that was returned."""
    """A human-readable description of the error that was returned."""
