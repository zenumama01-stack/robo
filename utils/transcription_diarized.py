from .transcription_diarized_segment import TranscriptionDiarizedSegment
__all__ = ["TranscriptionDiarized", "Usage", "UsageTokens", "UsageTokensInputTokenDetails", "UsageDuration"]
class TranscriptionDiarized(BaseModel):
    Represents a diarized transcription response returned by the model, including the combined transcript and speaker-segment annotations.
    duration: float
    segments: List[TranscriptionDiarizedSegment]
    """Segments of the transcript annotated with timestamps and speaker labels."""
    task: Literal["transcribe"]
    """The type of task that was run. Always `transcribe`."""
    """The concatenated transcript text for the entire audio input."""
    """Token or duration usage statistics for the request."""
