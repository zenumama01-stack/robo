from .response_input import ResponseInput
from .response_conversation_param import ResponseConversationParam
__all__ = ["ResponsesClientEvent", "ContextManagement", "Conversation", "StreamOptions", "ToolChoice"]
class ContextManagement(BaseModel):
    compact_threshold: Optional[int] = None
Conversation: TypeAlias = Union[str, ResponseConversationParam, None]
class StreamOptions(BaseModel):
    include_obfuscation: Optional[bool] = None
class ResponsesClientEvent(BaseModel):
    """The type of the client event. Always `response.create`."""
    context_management: Optional[List[ContextManagement]] = None
    include: Optional[List[ResponseIncludable]] = None
    input: Union[str, ResponseInput, None] = None
    model: Optional[ResponsesModel] = None
    parallel_tool_calls: Optional[bool] = None
    store: Optional[bool] = None
    stream: Optional[bool] = None
    stream_options: Optional[StreamOptions] = None
