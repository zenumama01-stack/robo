__all__ = ["ResponseToolSearchOutputItemParamParam"]
class ResponseToolSearchOutputItemParamParam(TypedDict, total=False):
    tools: Required[Iterable[ToolParam]]
    type: Required[Literal["tool_search_output"]]
