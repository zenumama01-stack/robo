    RealtimeTranscriptionSessionAudioInputTurnDetection,
__all__ = ["RealtimeTranscriptionSessionAudioInput", "NoiseReduction"]
class RealtimeTranscriptionSessionAudioInput(BaseModel):
    turn_detection: Optional[RealtimeTranscriptionSessionAudioInputTurnDetection] = None
