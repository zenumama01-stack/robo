from .realtime_audio_config_input import RealtimeAudioConfigInput
from .realtime_audio_config_output import RealtimeAudioConfigOutput
__all__ = ["RealtimeAudioConfig"]
class RealtimeAudioConfig(BaseModel):
    input: Optional[RealtimeAudioConfigInput] = None
    output: Optional[RealtimeAudioConfigOutput] = None
