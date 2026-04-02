__all__ = ["ToolSearchToolParam"]
class ToolSearchToolParam(TypedDict, total=False):
    type: Required[Literal["tool_search"]]
