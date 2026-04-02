from .chat_completion_allowed_tools_param import ChatCompletionAllowedToolsParam
__all__ = ["ChatCompletionAllowedToolChoiceParam"]
class ChatCompletionAllowedToolChoiceParam(TypedDict, total=False):
    """Constrains the tools available to the model to a pre-defined set."""
    allowed_tools: Required[ChatCompletionAllowedToolsParam]
    type: Required[Literal["allowed_tools"]]
    """Allowed tool configuration type. Always `allowed_tools`."""
