__all__ = ["ChatCompletionNamedToolChoiceCustomParam", "Custom"]
class ChatCompletionNamedToolChoiceCustomParam(TypedDict, total=False):
    Use to force the model to call a specific custom tool.
    """For custom tool calling, the type is always `custom`."""
