from .custom_tool_param import CustomToolParam
__all__ = ["NamespaceToolParam", "Tool", "ToolFunction"]
class ToolFunction(TypedDict, total=False):
    parameters: Optional[object]
    strict: Optional[bool]
Tool: TypeAlias = Union[ToolFunction, CustomToolParam]
class NamespaceToolParam(TypedDict, total=False):
    type: Required[Literal["namespace"]]
