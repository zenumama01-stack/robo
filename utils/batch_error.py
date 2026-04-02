__all__ = ["BatchError"]
class BatchError(BaseModel):
    """An error code identifying the error type."""
    line: Optional[int] = None
    """The line number of the input file where the error occurred, if applicable."""
    message: Optional[str] = None
    """A human-readable message providing more details about the error."""
    """The name of the parameter that caused the error, if applicable."""
