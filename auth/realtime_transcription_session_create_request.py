from .realtime_transcription_session_audio import RealtimeTranscriptionSessionAudio
__all__ = ["RealtimeTranscriptionSessionCreateRequest"]
class RealtimeTranscriptionSessionCreateRequest(BaseModel):
    type: Literal["transcription"]
    """The type of session to create.
    Always `transcription` for transcription sessions.
    audio: Optional[RealtimeTranscriptionSessionAudio] = None
