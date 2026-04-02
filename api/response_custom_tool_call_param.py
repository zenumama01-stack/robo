__all__ = ["ResponseCustomToolCallParam"]
class ResponseCustomToolCallParam(TypedDict, total=False):
    type: Required[Literal["custom_tool_call"]]
    namespace: str
