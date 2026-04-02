from .realtime_function_tool import RealtimeFunctionTool
from ..responses.response_prompt import ResponsePrompt
from ..responses.tool_choice_mcp import ToolChoiceMcp
from ..responses.tool_choice_options import ToolChoiceOptions
from ..responses.tool_choice_function import ToolChoiceFunction
from .realtime_response_create_mcp_tool import RealtimeResponseCreateMcpTool
from .realtime_response_create_audio_output import RealtimeResponseCreateAudioOutput
__all__ = ["RealtimeResponseCreateParams", "ToolChoice", "Tool"]
ToolChoice: TypeAlias = Union[ToolChoiceOptions, ToolChoiceFunction, ToolChoiceMcp]
Tool: TypeAlias = Union[RealtimeFunctionTool, RealtimeResponseCreateMcpTool]
class RealtimeResponseCreateParams(BaseModel):
    audio: Optional[RealtimeResponseCreateAudioOutput] = None
    input: Optional[List[ConversationItem]] = None
    Response. Note that this can include references to items that previously
    appeared in the session using their id.
    behavior. Note that the server sets default instructions which will be used if
    this field is not set and are visible in the `session.created` event at the
    start of the session.
    prompt: Optional[ResponsePrompt] = None
    tool_choice: Optional[ToolChoice] = None
