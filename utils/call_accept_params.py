from .realtime_truncation_param import RealtimeTruncationParam
from .realtime_audio_config_param import RealtimeAudioConfigParam
from .realtime_tools_config_param import RealtimeToolsConfigParam
from .realtime_tracing_config_param import RealtimeTracingConfigParam
from ..responses.response_prompt_param import ResponsePromptParam
from .realtime_tool_choice_config_param import RealtimeToolChoiceConfigParam
__all__ = ["CallAcceptParams"]
class CallAcceptParams(TypedDict, total=False):
    type: Required[Literal["realtime"]]
    """The type of session to create. Always `realtime` for the Realtime API."""
    audio: RealtimeAudioConfigParam
    """Configuration for input and output audio."""
    include: List[Literal["item.input_audio_transcription.logprobs"]]
    """Additional fields to include in server outputs.
    max_output_tokens: Union[int, Literal["inf"]]
    output_modalities: List[Literal["text", "audio"]]
    It defaults to `["audio"]`, indicating that the model will respond with audio
    plus a transcript. `["text"]` can be used to make the model respond with text
    only. It is not possible to request both `text` and `audio` at the same time.
    prompt: Optional[ResponsePromptParam]
    Reference to a prompt template and its variables.
    tool_choice: RealtimeToolChoiceConfigParam
    Provide one of the string modes or force a specific function/MCP tool.
    tools: RealtimeToolsConfigParam
    """Tools available to the model."""
    tracing: Optional[RealtimeTracingConfigParam]
    Realtime API can write session traces to the
    truncation: RealtimeTruncationParam
    When the number of tokens in a conversation exceeds the model's input token
