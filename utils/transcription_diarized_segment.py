__all__ = ["TranscriptionDiarizedSegment"]
class TranscriptionDiarizedSegment(BaseModel):
    """A segment of diarized transcript text with speaker metadata."""
    """Unique identifier for the segment."""
    end: float
    """End timestamp of the segment in seconds."""
    speaker: str
    """Speaker label for this segment.
    When known speakers are provided, the label matches `known_speaker_names[]`.
    Otherwise speakers are labeled sequentially using capital letters (`A`, `B`,
    ...).
    start: float
    """Start timestamp of the segment in seconds."""
    """Transcript text for this segment."""
    type: Literal["transcript.text.segment"]
    """The type of the segment. Always `transcript.text.segment`."""
