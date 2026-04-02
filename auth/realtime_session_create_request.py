from .realtime_truncation import RealtimeTruncation
from .realtime_audio_config import RealtimeAudioConfig
from .realtime_tools_config import RealtimeToolsConfig
from .realtime_tracing_config import RealtimeTracingConfig
from .realtime_tool_choice_config import RealtimeToolChoiceConfig
__all__ = ["RealtimeSessionCreateRequest"]
class RealtimeSessionCreateRequest(BaseModel):
    type: Literal["realtime"]
    audio: Optional[RealtimeAudioConfig] = None
    include: Optional[List[Literal["item.input_audio_transcription.logprobs"]]] = None
    tool_choice: Optional[RealtimeToolChoiceConfig] = None
    tools: Optional[RealtimeToolsConfig] = None
    tracing: Optional[RealtimeTracingConfig] = None
    truncation: Optional[RealtimeTruncation] = None
