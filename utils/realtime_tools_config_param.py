    "RealtimeToolsConfigParam",
    "RealtimeToolsConfigUnionParam",
    "Mcp",
    "McpAllowedTools",
    "McpAllowedToolsMcpToolFilter",
    "McpRequireApproval",
    "McpRequireApprovalMcpToolApprovalFilter",
    "McpRequireApprovalMcpToolApprovalFilterAlways",
    "McpRequireApprovalMcpToolApprovalFilterNever",
class McpAllowedToolsMcpToolFilter(TypedDict, total=False):
McpAllowedTools: TypeAlias = Union[SequenceNotStr[str], McpAllowedToolsMcpToolFilter]
class McpRequireApprovalMcpToolApprovalFilterAlways(TypedDict, total=False):
class McpRequireApprovalMcpToolApprovalFilterNever(TypedDict, total=False):
class McpRequireApprovalMcpToolApprovalFilter(TypedDict, total=False):
    always: McpRequireApprovalMcpToolApprovalFilterAlways
    never: McpRequireApprovalMcpToolApprovalFilterNever
McpRequireApproval: TypeAlias = Union[McpRequireApprovalMcpToolApprovalFilter, Literal["always", "never"]]
class Mcp(TypedDict, total=False):
    allowed_tools: Optional[McpAllowedTools]
    require_approval: Optional[McpRequireApproval]
RealtimeToolsConfigUnionParam: TypeAlias = Union[RealtimeFunctionToolParam, Mcp]
RealtimeToolsConfigParam: TypeAlias = List[RealtimeToolsConfigUnionParam]
