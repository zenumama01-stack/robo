__all__ = ["ChatCompletionMessageFunctionToolCallParam", "Function"]
class Function(TypedDict, total=False):
class ChatCompletionMessageFunctionToolCallParam(TypedDict, total=False):
    function: Required[Function]
