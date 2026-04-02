__all__ = ["ChatCompletionNamedToolChoiceParam", "Function"]
class ChatCompletionNamedToolChoiceParam(TypedDict, total=False):
    Use to force the model to call a specific function.
    """For function calling, the type is always `function`."""
