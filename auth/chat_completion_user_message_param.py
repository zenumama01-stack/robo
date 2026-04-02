from .chat_completion_content_part_param import ChatCompletionContentPartParam
__all__ = ["ChatCompletionUserMessageParam"]
class ChatCompletionUserMessageParam(TypedDict, total=False):
    Messages sent by an end user, containing prompts or additional context
    content: Required[Union[str, Iterable[ChatCompletionContentPartParam]]]
    """The contents of the user message."""
    role: Required[Literal["user"]]
    """The role of the messages author, in this case `user`."""
