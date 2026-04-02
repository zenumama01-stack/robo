__all__ = ["ResponseFileSearchToolCallParam", "Result"]
class Result(TypedDict, total=False):
    attributes: Optional[Dict[str, Union[str, float, bool]]]
class ResponseFileSearchToolCallParam(TypedDict, total=False):
    queries: Required[SequenceNotStr[str]]
    status: Required[Literal["in_progress", "searching", "completed", "incomplete", "failed"]]
    type: Required[Literal["file_search_call"]]
    results: Optional[Iterable[Result]]
