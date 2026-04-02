from ...shared_params.metadata import Metadata
from .conversation_item_with_reference_param import ConversationItemWithReferenceParam
__all__ = ["ResponseCreateEventParam", "Response", "ResponseTool"]
class ResponseTool(TypedDict, total=False):
    parameters: object
class Response(TypedDict, total=False):
    conversation: Union[str, Literal["auto", "none"]]
    input: Iterable[ConversationItemWithReferenceParam]
    max_response_output_tokens: Union[int, Literal["inf"]]
    modalities: List[Literal["text", "audio"]]
    output_audio_format: Literal["pcm16", "g711_ulaw", "g711_alaw"]
    tool_choice: str
    tools: Iterable[ResponseTool]
class ResponseCreateEventParam(TypedDict, total=False):
    type: Required[Literal["response.create"]]
    response: Response
from .realtime_response_create_params_param import RealtimeResponseCreateParamsParam
__all__ = ["ResponseCreateEventParam"]
    response: RealtimeResponseCreateParamsParam
