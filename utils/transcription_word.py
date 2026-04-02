__all__ = ["TranscriptionWord"]
class TranscriptionWord(BaseModel):
    """End time of the word in seconds."""
    """Start time of the word in seconds."""
    word: str
    """The text content of the word."""
