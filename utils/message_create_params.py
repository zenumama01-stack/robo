from .message_content_part_param import MessageContentPartParam
from ..code_interpreter_tool_param import CodeInterpreterToolParam
__all__ = ["MessageCreateParams", "Attachment", "AttachmentTool", "AttachmentToolFileSearch"]
class MessageCreateParams(TypedDict, total=False):
    attachments: Optional[Iterable[Attachment]]
class AttachmentToolFileSearch(TypedDict, total=False):
AttachmentTool: TypeAlias = Union[CodeInterpreterToolParam, AttachmentToolFileSearch]
class Attachment(TypedDict, total=False):
    tools: Iterable[AttachmentTool]
