    "RealtimeToolsConfigUnion",
class McpAllowedToolsMcpToolFilter(BaseModel):
McpAllowedTools: TypeAlias = Union[List[str], McpAllowedToolsMcpToolFilter, None]
class McpRequireApprovalMcpToolApprovalFilterAlways(BaseModel):
class McpRequireApprovalMcpToolApprovalFilterNever(BaseModel):
class McpRequireApprovalMcpToolApprovalFilter(BaseModel):
    always: Optional[McpRequireApprovalMcpToolApprovalFilterAlways] = None
    never: Optional[McpRequireApprovalMcpToolApprovalFilterNever] = None
McpRequireApproval: TypeAlias = Union[McpRequireApprovalMcpToolApprovalFilter, Literal["always", "never"], None]
class Mcp(BaseModel):
    allowed_tools: Optional[McpAllowedTools] = None
    require_approval: Optional[McpRequireApproval] = None
RealtimeToolsConfigUnion: TypeAlias = Annotated[Union[RealtimeFunctionTool, Mcp], PropertyInfo(discriminator="type")]
