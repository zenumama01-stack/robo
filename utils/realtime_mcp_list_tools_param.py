__all__ = ["RealtimeMcpListToolsParam", "Tool"]
    input_schema: Required[object]
    annotations: Optional[object]
class RealtimeMcpListToolsParam(TypedDict, total=False):
    tools: Required[Iterable[Tool]]
    type: Required[Literal["mcp_list_tools"]]
