    "RealtimeResponseCreateMcpToolParam",
class AllowedToolsMcpToolFilter(TypedDict, total=False):
    read_only: bool
    tool_names: SequenceNotStr[str]
AllowedTools: TypeAlias = Union[SequenceNotStr[str], AllowedToolsMcpToolFilter]
class RequireApprovalMcpToolApprovalFilterAlways(TypedDict, total=False):
class RequireApprovalMcpToolApprovalFilterNever(TypedDict, total=False):
class RequireApprovalMcpToolApprovalFilter(TypedDict, total=False):
    always: RequireApprovalMcpToolApprovalFilterAlways
    never: RequireApprovalMcpToolApprovalFilterNever
RequireApproval: TypeAlias = Union[RequireApprovalMcpToolApprovalFilter, Literal["always", "never"]]
class RealtimeResponseCreateMcpToolParam(TypedDict, total=False):
    type: Required[Literal["mcp"]]
    allowed_tools: Optional[AllowedTools]
    authorization: str
    connector_id: Literal[
    defer_loading: bool
    headers: Optional[Dict[str, str]]
    require_approval: Optional[RequireApproval]
    server_description: str
    server_url: str
