from .realtime_audio_config_input_param import RealtimeAudioConfigInputParam
from .realtime_audio_config_output_param import RealtimeAudioConfigOutputParam
__all__ = ["RealtimeAudioConfigParam"]
class RealtimeAudioConfigParam(TypedDict, total=False):
    input: RealtimeAudioConfigInputParam
    output: RealtimeAudioConfigOutputParam
