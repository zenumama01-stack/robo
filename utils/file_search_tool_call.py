    "FileSearchToolCall",
    "FileSearch",
    "FileSearchRankingOptions",
    "FileSearchResult",
    "FileSearchResultContent",
    """The ranking options for the file search."""
class FileSearchResultContent(BaseModel):
    """The text content of the file."""
    type: Optional[Literal["text"]] = None
    """The type of the content."""
class FileSearchResult(BaseModel):
    """A result instance of the file search."""
    """The ID of the file that result was found in."""
    file_name: str
    """The name of the file that result was found in."""
    """The score of the result.
    content: Optional[List[FileSearchResultContent]] = None
    """The content of the result that was found.
    The content is only included if requested via the include query parameter.
    """For now, this is always going to be an empty object."""
    results: Optional[List[FileSearchResult]] = None
    """The results of the file search."""
class FileSearchToolCall(BaseModel):
    """The ID of the tool call object."""
    This is always going to be `file_search` for this type of tool call.
