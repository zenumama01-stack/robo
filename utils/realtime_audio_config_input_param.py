from .audio_transcription_param import AudioTranscriptionParam
from .realtime_audio_formats_param import RealtimeAudioFormatsParam
from .realtime_audio_input_turn_detection_param import RealtimeAudioInputTurnDetectionParam
__all__ = ["RealtimeAudioConfigInputParam", "NoiseReduction"]
class NoiseReduction(TypedDict, total=False):
    type: NoiseReductionType
class RealtimeAudioConfigInputParam(TypedDict, total=False):
    format: RealtimeAudioFormatsParam
    noise_reduction: NoiseReduction
    transcription: AudioTranscriptionParam
    turn_detection: Optional[RealtimeAudioInputTurnDetectionParam]
