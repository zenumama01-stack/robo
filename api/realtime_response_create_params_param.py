from .realtime_function_tool_param import RealtimeFunctionToolParam
from ..responses.tool_choice_mcp_param import ToolChoiceMcpParam
from ..responses.tool_choice_function_param import ToolChoiceFunctionParam
from .realtime_response_create_mcp_tool_param import RealtimeResponseCreateMcpToolParam
from .realtime_response_create_audio_output_param import RealtimeResponseCreateAudioOutputParam
__all__ = ["RealtimeResponseCreateParamsParam", "ToolChoice", "Tool"]
ToolChoice: TypeAlias = Union[ToolChoiceOptions, ToolChoiceFunctionParam, ToolChoiceMcpParam]
Tool: TypeAlias = Union[RealtimeFunctionToolParam, RealtimeResponseCreateMcpToolParam]
class RealtimeResponseCreateParamsParam(TypedDict, total=False):
    audio: RealtimeResponseCreateAudioOutputParam
    input: Iterable[ConversationItemParam]
    tool_choice: ToolChoice
