__all__ = ["Thread", "ToolResources", "ToolResourcesCodeInterpreter", "ToolResourcesFileSearch"]
    attached to this thread. There can be a maximum of 1 vector store attached to
    the thread.
    A set of resources that are made available to the assistant's tools in this thread. The resources are specific to the type of tool. For example, the `code_interpreter` tool requires a list of file IDs, while the `file_search` tool requires a list of vector store IDs.
class Thread(BaseModel):
    Represents a thread that contains [messages](https://platform.openai.com/docs/api-reference/messages).
    """The Unix timestamp (in seconds) for when the thread was created."""
    object: Literal["thread"]
    """The object type, which is always `thread`."""
    A set of resources that are made available to the assistant's tools in this
