__all__ = ["TranscriptionTextDeltaEvent", "Logprob"]
    """The token that was used to generate the log probability."""
    bytes: Optional[List[int]] = None
    """The bytes that were used to generate the log probability."""
class TranscriptionTextDeltaEvent(BaseModel):
    """Emitted when there is an additional text delta.
    This is also the first event emitted when the transcription starts. Only emitted when you [create a transcription](https://platform.openai.com/docs/api-reference/audio/create-transcription) with the `Stream` parameter set to `true`.
    """The text delta that was additionally transcribed."""
    type: Literal["transcript.text.delta"]
    """The type of the event. Always `transcript.text.delta`."""
    """The log probabilities of the delta.
    Only included if you
    [create a transcription](https://platform.openai.com/docs/api-reference/audio/create-transcription)
    with the `include[]` parameter set to `logprobs`.
    segment_id: Optional[str] = None
    """Identifier of the diarized segment that this delta belongs to.
    Only present when using `gpt-4o-transcribe-diarize`.
