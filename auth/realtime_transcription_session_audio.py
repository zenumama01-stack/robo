from .realtime_transcription_session_audio_input import RealtimeTranscriptionSessionAudioInput
__all__ = ["RealtimeTranscriptionSessionAudio"]
class RealtimeTranscriptionSessionAudio(BaseModel):
    input: Optional[RealtimeTranscriptionSessionAudioInput] = None
