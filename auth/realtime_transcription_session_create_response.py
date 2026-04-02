from .realtime_transcription_session_turn_detection import RealtimeTranscriptionSessionTurnDetection
__all__ = ["RealtimeTranscriptionSessionCreateResponse", "Audio", "AudioInput", "AudioInputNoiseReduction"]
    """Configuration for input audio noise reduction."""
    turn_detection: Optional[RealtimeTranscriptionSessionTurnDetection] = None
    """Configuration for input audio for the session."""
class RealtimeTranscriptionSessionCreateResponse(BaseModel):
    """A Realtime transcription session configuration object."""
    """The object type. Always `realtime.transcription_session`."""
    """The type of session. Always `transcription` for transcription sessions."""
    """Expiration timestamp for the session, in seconds since epoch."""
    - `item.input_audio_transcription.logprobs`: Include logprobs for input audio
