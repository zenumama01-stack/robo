__all__ = ["ChatCompletionDeveloperMessageParam"]
class ChatCompletionDeveloperMessageParam(TypedDict, total=False):
    Developer-provided instructions that the model should follow, regardless of
    messages sent by the user. With o1 models and newer, `developer` messages
    replace the previous `system` messages.
    content: Required[Union[str, Iterable[ChatCompletionContentPartTextParam]]]
    """The contents of the developer message."""
    role: Required[Literal["developer"]]
    """The role of the messages author, in this case `developer`."""
