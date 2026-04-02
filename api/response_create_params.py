from .response_input_param import ResponseInputParam
from .response_prompt_param import ResponsePromptParam
from .response_text_config_param import ResponseTextConfigParam
from ..shared_params.responses_model import ResponsesModel
    "ResponseCreateParamsBase",
    "ContextManagement",
    "Conversation",
    "StreamOptions",
    "ResponseCreateParamsNonStreaming",
    "ResponseCreateParamsStreaming",
class ResponseCreateParamsBase(TypedDict, total=False):
    background: Optional[bool]
    context_management: Optional[Iterable[ContextManagement]]
    """Context management configuration for this request."""
    include: Optional[List[ResponseIncludable]]
    input: Union[str, ResponseInputParam]
    """Text, image, or file inputs to the model, used to generate a response.
    max_output_tokens: Optional[int]
    max_tool_calls: Optional[int]
    """Whether to store the generated model response for later retrieval via API."""
    stream_options: Optional[StreamOptions]
    """Options for streaming responses. Only set this when you set `stream: true`."""
    text: ResponseTextConfigParam
    truncation: Optional[Literal["auto", "disabled"]]
class ContextManagement(TypedDict, total=False):
    type: Required[str]
    """The context management entry type. Currently only 'compaction' is supported."""
    compact_threshold: Optional[int]
    """Token threshold at which compaction should be triggered for this entry."""
class StreamOptions(TypedDict, total=False):
class ResponseCreateParamsNonStreaming(ResponseCreateParamsBase, total=False):
class ResponseCreateParamsStreaming(ResponseCreateParamsBase):
ResponseCreateParams = Union[ResponseCreateParamsNonStreaming, ResponseCreateParamsStreaming]
