__all__ = ["TranscriptionTextDoneEvent", "Logprob", "Usage", "UsageInputTokenDetails"]
class UsageInputTokenDetails(BaseModel):
    input_token_details: Optional[UsageInputTokenDetails] = None
class TranscriptionTextDoneEvent(BaseModel):
    """Emitted when the transcription is complete.
    Contains the complete transcription text. Only emitted when you [create a transcription](https://platform.openai.com/docs/api-reference/audio/create-transcription) with the `Stream` parameter set to `true`.
    """The text that was transcribed."""
    type: Literal["transcript.text.done"]
    """The type of the event. Always `transcript.text.done`."""
    """The log probabilities of the individual tokens in the transcription.
