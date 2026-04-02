__all__ = ["TranscriptionSession", "ClientSecret", "InputAudioTranscription", "TurnDetection"]
    model: Optional[Literal["gpt-4o-transcribe", "gpt-4o-mini-transcribe", "whisper-1"]] = None
    """The model to use for transcription.
    Can be `gpt-4o-transcribe`, `gpt-4o-mini-transcribe`, or `whisper-1`.
class TranscriptionSession(BaseModel):
    """Ephemeral key returned by the API.
    Only present when the session is created on the server via REST API.
    """Configuration of the transcription model."""
