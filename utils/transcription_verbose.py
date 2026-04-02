from .transcription_word import TranscriptionWord
from .transcription_segment import TranscriptionSegment
__all__ = ["TranscriptionVerbose", "Usage"]
class TranscriptionVerbose(BaseModel):
    Represents a verbose json transcription response returned by model, based on the provided input.
    """The duration of the input audio."""
    """The language of the input audio."""
    segments: Optional[List[TranscriptionSegment]] = None
    """Segments of the transcribed text and their corresponding details."""
    words: Optional[List[TranscriptionWord]] = None
    """Extracted words and their corresponding timestamps."""
