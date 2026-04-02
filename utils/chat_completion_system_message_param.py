__all__ = ["ChatCompletionSystemMessageParam"]
class ChatCompletionSystemMessageParam(TypedDict, total=False):
    messages sent by the user. With o1 models and newer, use `developer` messages
    for this purpose instead.
    """The contents of the system message."""
    role: Required[Literal["system"]]
    """The role of the messages author, in this case `system`."""
