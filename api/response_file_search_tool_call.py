__all__ = ["ResponseFileSearchToolCall", "Result"]
    """The unique ID of the file."""
    filename: Optional[str] = None
    score: Optional[float] = None
    """The relevance score of the file - a value between 0 and 1."""
    """The text that was retrieved from the file."""
class ResponseFileSearchToolCall(BaseModel):
    """The results of a file search tool call.
    [file search guide](https://platform.openai.com/docs/guides/tools-file-search) for more information.
    """The unique ID of the file search tool call."""
    queries: List[str]
    """The queries used to search for files."""
    status: Literal["in_progress", "searching", "completed", "incomplete", "failed"]
    """The status of the file search tool call.
    One of `in_progress`, `searching`, `incomplete` or `failed`,
    type: Literal["file_search_call"]
    """The type of the file search tool call. Always `file_search_call`."""
    results: Optional[List[Result]] = None
    """The results of the file search tool call."""
