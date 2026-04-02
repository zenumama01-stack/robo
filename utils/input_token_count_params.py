from .tool_param import ToolParam
from .tool_choice_options import ToolChoiceOptions
from .tool_choice_mcp_param import ToolChoiceMcpParam
from .tool_choice_shell_param import ToolChoiceShellParam
from .tool_choice_types_param import ToolChoiceTypesParam
from ..shared_params.reasoning import Reasoning
from .tool_choice_custom_param import ToolChoiceCustomParam
from .response_input_item_param import ResponseInputItemParam
from .tool_choice_allowed_param import ToolChoiceAllowedParam
from .tool_choice_function_param import ToolChoiceFunctionParam
from .tool_choice_apply_patch_param import ToolChoiceApplyPatchParam
from .response_conversation_param_param import ResponseConversationParamParam
from .response_format_text_config_param import ResponseFormatTextConfigParam
__all__ = ["InputTokenCountParams", "Conversation", "Text", "ToolChoice"]
class InputTokenCountParams(TypedDict, total=False):
    conversation: Optional[Conversation]
    """The conversation that this response belongs to.
    Items from this conversation are prepended to `input_items` for this response
    request. Input items and output items from this response are automatically added
    to this conversation after this response completes.
    input: Union[str, Iterable[ResponseInputItemParam], None]
    """Text, image, or file inputs to the model, used to generate a response"""
    A system (or developer) message inserted into the model's context. When used
    parallel_tool_calls: Optional[bool]
    """Whether to allow the model to run tool calls in parallel."""
    previous_response_id: Optional[str]
    """The unique ID of the previous response to the model.
    Use this to create multi-turn conversations. Learn more about
    reasoning: Optional[Reasoning]
    **gpt-5 and o-series models only** Configuration options for
    text: Optional[Text]
    tool_choice: Optional[ToolChoice]
    """Controls which tool the model should use, if any."""
    tools: Optional[Iterable[ToolParam]]
    truncation: Literal["auto", "disabled"]
    """The truncation strategy to use for the model response.
      items from the beginning of the conversation. - `disabled` (default): If the
      input size will exceed the context window size for a model, the request will
      fail with a 400 error.
Conversation: TypeAlias = Union[str, ResponseConversationParamParam]
class Text(TypedDict, total=False):
ToolChoice: TypeAlias = Union[
    ToolChoiceAllowedParam,
    ToolChoiceTypesParam,
    ToolChoiceMcpParam,
    ToolChoiceCustomParam,
    ToolChoiceApplyPatchParam,
    ToolChoiceShellParam,
Conversation: TypeAlias = Union[str, ResponseConversationParam]
