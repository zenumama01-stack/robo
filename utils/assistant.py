from .assistant_tool import AssistantTool
from ..shared.metadata import Metadata
from .assistant_response_format_option import AssistantResponseFormatOption
__all__ = ["Assistant", "ToolResources", "ToolResourcesCodeInterpreter", "ToolResourcesFileSearch"]
class ToolResourcesCodeInterpreter(BaseModel):
    file_ids: Optional[List[str]] = None
    A list of [file](https://platform.openai.com/docs/api-reference/files) IDs made
    available to the `code_interpreter`` tool. There can be a maximum of 20 files
    associated with the tool.
class ToolResourcesFileSearch(BaseModel):
    vector_store_ids: Optional[List[str]] = None
    The ID of the
    [vector store](https://platform.openai.com/docs/api-reference/vector-stores/object)
    attached to this assistant. There can be a maximum of 1 vector store attached to
    the assistant.
class ToolResources(BaseModel):
    """A set of resources that are used by the assistant's tools.
    The resources are specific to the type of tool. For example, the `code_interpreter` tool requires a list of file IDs, while the `file_search` tool requires a list of vector store IDs.
    code_interpreter: Optional[ToolResourcesCodeInterpreter] = None
    file_search: Optional[ToolResourcesFileSearch] = None
class Assistant(BaseModel):
    """Represents an `assistant` that can call the model and use tools."""
    """The Unix timestamp (in seconds) for when the assistant was created."""
    description: Optional[str] = None
    """The description of the assistant. The maximum length is 512 characters."""
    instructions: Optional[str] = None
    """The system instructions that the assistant uses.
    The maximum length is 256,000 characters.
    """The name of the assistant. The maximum length is 256 characters."""
    object: Literal["assistant"]
    """The object type, which is always `assistant`."""
    tools: List[AssistantTool]
    """A list of tool enabled on the assistant.
    There can be a maximum of 128 tools per assistant. Tools can be of types
    `code_interpreter`, `file_search`, or `function`.
    response_format: Optional[AssistantResponseFormatOption] = None
    """Specifies the format that the model must output.
    Compatible with [GPT-4o](https://platform.openai.com/docs/models#gpt-4o),
    tool_resources: Optional[ToolResources] = None
    The resources are specific to the type of tool. For example, the
