from .audio_transcription import AudioTranscription
from .noise_reduction_type import NoiseReductionType
from .realtime_audio_formats import RealtimeAudioFormats
from .realtime_audio_input_turn_detection import RealtimeAudioInputTurnDetection
__all__ = ["RealtimeAudioConfigInput", "NoiseReduction"]
class NoiseReduction(BaseModel):
    This can be set to `null` to turn off.
    Noise reduction filters audio added to the input audio buffer before it is sent to VAD and the model.
    Filtering the audio can improve VAD and turn detection accuracy (reducing false positives) and model performance by improving perception of the input audio.
    type: Optional[NoiseReductionType] = None
class RealtimeAudioConfigInput(BaseModel):
    format: Optional[RealtimeAudioFormats] = None
    """The format of the input audio."""
    noise_reduction: Optional[NoiseReduction] = None
    transcription: Optional[AudioTranscription] = None
    turn_detection: Optional[RealtimeAudioInputTurnDetection] = None
    trigger model response.
    Server VAD means that the model will detect the start and end of speech based on
    audio volume and respond at the end of user speech.
