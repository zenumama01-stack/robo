from .assistant_tool_choice_option_param import AssistantToolChoiceOptionParam
from .threads.message_content_part_param import MessageContentPartParam
    "ThreadCreateAndRunParamsBase",
    "Thread",
    "ThreadMessage",
    "ThreadMessageAttachment",
    "ThreadMessageAttachmentTool",
    "ThreadMessageAttachmentToolFileSearch",
    "ThreadToolResources",
    "ThreadToolResourcesCodeInterpreter",
    "ThreadToolResourcesFileSearch",
    "ThreadToolResourcesFileSearchVectorStore",
    "ThreadToolResourcesFileSearchVectorStoreChunkingStrategy",
    "ThreadToolResourcesFileSearchVectorStoreChunkingStrategyAuto",
    "ThreadToolResourcesFileSearchVectorStoreChunkingStrategyStatic",
    "ThreadToolResourcesFileSearchVectorStoreChunkingStrategyStaticStatic",
    "TruncationStrategy",
    "ThreadCreateAndRunParamsNonStreaming",
    "ThreadCreateAndRunParamsStreaming",
class ThreadCreateAndRunParamsBase(TypedDict, total=False):
    assistant_id: Required[str]
    """Override the default system message of the assistant.
    This is useful for modifying the behavior on a per-run basis.
    max_completion_tokens: Optional[int]
    The maximum number of completion tokens that may be used over the course of the
    max_prompt_tokens: Optional[int]
    """The maximum number of prompt tokens that may be used over the course of the run.
    model: Union[str, ChatModel, None]
    The ID of the [Model](https://platform.openai.com/docs/api-reference/models) to
    parallel_tool_calls: bool
    Whether to enable
    thread: Thread
    """Options to create a new thread.
    If no thread is provided when running a request, an empty thread will be
    created.
    tool_choice: Optional[AssistantToolChoiceOptionParam]
    Controls which (if any) tool is called by the model. `none` means the model will
    tools: Optional[Iterable[AssistantToolParam]]
    """Override the tools the assistant can use for this run.
    truncation_strategy: Optional[TruncationStrategy]
    """Controls for how a thread will be truncated prior to the run.
    Use this to control the initial context window of the run.
class ThreadMessageAttachmentToolFileSearch(TypedDict, total=False):
ThreadMessageAttachmentTool: TypeAlias = Union[CodeInterpreterToolParam, ThreadMessageAttachmentToolFileSearch]
class ThreadMessageAttachment(TypedDict, total=False):
    """The ID of the file to attach to the message."""
    tools: Iterable[ThreadMessageAttachmentTool]
    """The tools to add this file to."""
class ThreadMessage(TypedDict, total=False):
    content: Required[Union[str, Iterable[MessageContentPartParam]]]
    """The text contents of the message."""
    role: Required[Literal["user", "assistant"]]
    """The role of the entity that is creating the message. Allowed values include:
    attachments: Optional[Iterable[ThreadMessageAttachment]]
    """A list of files attached to the message, and the tools they should be added to."""
class ThreadToolResourcesCodeInterpreter(TypedDict, total=False):
class ThreadToolResourcesFileSearchVectorStoreChunkingStrategyAuto(TypedDict, total=False):
class ThreadToolResourcesFileSearchVectorStoreChunkingStrategyStaticStatic(TypedDict, total=False):
class ThreadToolResourcesFileSearchVectorStoreChunkingStrategyStatic(TypedDict, total=False):
    static: Required[ThreadToolResourcesFileSearchVectorStoreChunkingStrategyStaticStatic]
ThreadToolResourcesFileSearchVectorStoreChunkingStrategy: TypeAlias = Union[
    ThreadToolResourcesFileSearchVectorStoreChunkingStrategyAuto,
    ThreadToolResourcesFileSearchVectorStoreChunkingStrategyStatic,
class ThreadToolResourcesFileSearchVectorStore(TypedDict, total=False):
    chunking_strategy: ThreadToolResourcesFileSearchVectorStoreChunkingStrategy
class ThreadToolResourcesFileSearch(TypedDict, total=False):
    vector_stores: Iterable[ThreadToolResourcesFileSearchVectorStore]
    with file_ids and attach it to this thread. There can be a maximum of 1 vector
    store attached to the thread.
class ThreadToolResources(TypedDict, total=False):
    code_interpreter: ThreadToolResourcesCodeInterpreter
    file_search: ThreadToolResourcesFileSearch
class Thread(TypedDict, total=False):
    If no thread is provided when running a
    request, an empty thread will be created.
    messages: Iterable[ThreadMessage]
    A list of [messages](https://platform.openai.com/docs/api-reference/messages) to
    tool_resources: Optional[ThreadToolResources]
class TruncationStrategy(TypedDict, total=False):
    type: Required[Literal["auto", "last_messages"]]
    """The truncation strategy to use for the thread.
    The default is `auto`. If set to `last_messages`, the thread will be truncated
    to the n most recent messages in the thread. When set to `auto`, messages in the
    middle of the thread will be dropped to fit the context length of the model,
    `max_prompt_tokens`.
    last_messages: Optional[int]
    The number of most recent messages from the thread when constructing the context
    for the run.
class ThreadCreateAndRunParamsNonStreaming(ThreadCreateAndRunParamsBase, total=False):
    If `true`, returns a stream of events that happen during the Run as server-sent
class ThreadCreateAndRunParamsStreaming(ThreadCreateAndRunParamsBase):
ThreadCreateAndRunParams = Union[ThreadCreateAndRunParamsNonStreaming, ThreadCreateAndRunParamsStreaming]
