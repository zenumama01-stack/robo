from .realtime_transcription_session_audio_param import RealtimeTranscriptionSessionAudioParam
__all__ = ["RealtimeTranscriptionSessionCreateRequestParam"]
class RealtimeTranscriptionSessionCreateRequestParam(TypedDict, total=False):
    type: Required[Literal["transcription"]]
    audio: RealtimeTranscriptionSessionAudioParam
