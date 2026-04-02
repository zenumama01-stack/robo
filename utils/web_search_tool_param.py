__all__ = ["WebSearchToolParam", "Filters", "UserLocation"]
class Filters(TypedDict, total=False):
    allowed_domains: Optional[SequenceNotStr[str]]
class WebSearchToolParam(TypedDict, total=False):
    type: Required[Literal["web_search", "web_search_2025_08_26"]]
