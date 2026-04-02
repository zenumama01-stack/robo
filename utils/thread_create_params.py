    "ThreadCreateParams",
    "Message",
    "MessageAttachment",
    "MessageAttachmentTool",
    "MessageAttachmentToolFileSearch",
class ThreadCreateParams(TypedDict, total=False):
    messages: Iterable[Message]
class MessageAttachmentToolFileSearch(TypedDict, total=False):
MessageAttachmentTool: TypeAlias = Union[CodeInterpreterToolParam, MessageAttachmentToolFileSearch]
class MessageAttachment(TypedDict, total=False):
    tools: Iterable[MessageAttachmentTool]
class Message(TypedDict, total=False):
    attachments: Optional[Iterable[MessageAttachment]]
