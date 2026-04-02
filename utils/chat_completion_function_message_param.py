__all__ = ["ChatCompletionFunctionMessageParam"]
class ChatCompletionFunctionMessageParam(TypedDict, total=False):
    content: Required[Optional[str]]
    """The contents of the function message."""
    role: Required[Literal["function"]]
    """The role of the messages author, in this case `function`."""
