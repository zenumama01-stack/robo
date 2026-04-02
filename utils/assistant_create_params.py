from ..._types import SequenceNotStr
from ..shared.chat_model import ChatModel
from .assistant_tool_param import AssistantToolParam
from ..shared_params.metadata import Metadata
from ..shared.reasoning_effort import ReasoningEffort
from .assistant_response_format_option_param import AssistantResponseFormatOptionParam
    "AssistantCreateParams",
    "ToolResources",
    "ToolResourcesCodeInterpreter",
    "ToolResourcesFileSearch",
    "ToolResourcesFileSearchVectorStore",
    "ToolResourcesFileSearchVectorStoreChunkingStrategy",
    "ToolResourcesFileSearchVectorStoreChunkingStrategyAuto",
    "ToolResourcesFileSearchVectorStoreChunkingStrategyStatic",
    "ToolResourcesFileSearchVectorStoreChunkingStrategyStaticStatic",
class AssistantCreateParams(TypedDict, total=False):
    model: Required[Union[str, ChatModel]]
    description: Optional[str]
    instructions: Optional[str]
    reasoning_effort: Optional[ReasoningEffort]
    Constrains effort on reasoning for
    response_format: Optional[AssistantResponseFormatOptionParam]
    tool_resources: Optional[ToolResources]
    tools: Iterable[AssistantToolParam]
class ToolResourcesCodeInterpreter(TypedDict, total=False):
    available to the `code_interpreter` tool. There can be a maximum of 20 files
class ToolResourcesFileSearchVectorStoreChunkingStrategyAuto(TypedDict, total=False):
class ToolResourcesFileSearchVectorStoreChunkingStrategyStaticStatic(TypedDict, total=False):
class ToolResourcesFileSearchVectorStoreChunkingStrategyStatic(TypedDict, total=False):
    static: Required[ToolResourcesFileSearchVectorStoreChunkingStrategyStaticStatic]
ToolResourcesFileSearchVectorStoreChunkingStrategy: TypeAlias = Union[
    ToolResourcesFileSearchVectorStoreChunkingStrategyAuto, ToolResourcesFileSearchVectorStoreChunkingStrategyStatic
class ToolResourcesFileSearchVectorStore(TypedDict, total=False):
    chunking_strategy: ToolResourcesFileSearchVectorStoreChunkingStrategy
    If not set, will use the `auto` strategy.
    A list of [file](https://platform.openai.com/docs/api-reference/files) IDs to
    add to the vector store. For vector stores created before Nov 2025, there can be
    a maximum of 10,000 files in a vector store. For vector stores created starting
    in Nov 2025, the limit is 100,000,000 files.
class ToolResourcesFileSearch(TypedDict, total=False):
    vector_store_ids: SequenceNotStr[str]
    The
    vector_stores: Iterable[ToolResourcesFileSearchVectorStore]
    A helper to create a
    with file_ids and attach it to this assistant. There can be a maximum of 1
    vector store attached to the assistant.
class ToolResources(TypedDict, total=False):
    code_interpreter: ToolResourcesCodeInterpreter
    file_search: ToolResourcesFileSearch
    add to the vector store. There can be a maximum of 10000 files in a vector
    store.
