    RealtimeTranscriptionSessionAudioInputTurnDetectionParam,
__all__ = ["RealtimeTranscriptionSessionAudioInputParam", "NoiseReduction"]
class RealtimeTranscriptionSessionAudioInputParam(TypedDict, total=False):
    turn_detection: Optional[RealtimeTranscriptionSessionAudioInputTurnDetectionParam]
