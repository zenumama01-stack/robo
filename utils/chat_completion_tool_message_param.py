__all__ = ["ChatCompletionToolMessageParam"]
class ChatCompletionToolMessageParam(TypedDict, total=False):
    """The contents of the tool message."""
    role: Required[Literal["tool"]]
    """The role of the messages author, in this case `tool`."""
    tool_call_id: Required[str]
    """Tool call that this message is responding to."""
