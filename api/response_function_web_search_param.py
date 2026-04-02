    "ResponseFunctionWebSearchParam",
class ActionSearchSource(TypedDict, total=False):
    type: Required[Literal["url"]]
class ActionSearch(TypedDict, total=False):
    query: Required[str]
    type: Required[Literal["search"]]
    queries: SequenceNotStr[str]
    sources: Iterable[ActionSearchSource]
class ActionOpenPage(TypedDict, total=False):
    type: Required[Literal["open_page"]]
    url: Optional[str]
class ActionFind(TypedDict, total=False):
    pattern: Required[str]
    type: Required[Literal["find_in_page"]]
Action: TypeAlias = Union[ActionSearch, ActionOpenPage, ActionFind]
class ResponseFunctionWebSearchParam(TypedDict, total=False):
    action: Required[Action]
    status: Required[Literal["in_progress", "searching", "completed", "failed"]]
    type: Required[Literal["web_search_call"]]
