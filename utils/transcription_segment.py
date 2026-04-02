__all__ = ["TranscriptionSegment"]
class TranscriptionSegment(BaseModel):
    """Unique identifier of the segment."""
    avg_logprob: float
    """Average logprob of the segment.
    If the value is lower than -1, consider the logprobs failed.
    compression_ratio: float
    """Compression ratio of the segment.
    If the value is greater than 2.4, consider the compression failed.
    """End time of the segment in seconds."""
    no_speech_prob: float
    """Probability of no speech in the segment.
    If the value is higher than 1.0 and the `avg_logprob` is below -1, consider this
    segment silent.
    seek: int
    """Seek offset of the segment."""
    """Start time of the segment in seconds."""
    """Temperature parameter used for generating the segment."""
    """Text content of the segment."""
    tokens: List[int]
    """Array of token IDs for the text content."""
