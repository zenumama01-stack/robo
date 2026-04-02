from .realtime_session_client_secret import RealtimeSessionClientSecret
    "RealtimeSessionCreateResponse",
    "AudioInput",
    "AudioInputNoiseReduction",
    "AudioInputTurnDetection",
    "AudioInputTurnDetectionServerVad",
    "AudioInputTurnDetectionSemanticVad",
    "AudioOutput",
    "ToolChoice",
    "ToolMcpTool",
    "ToolMcpToolAllowedTools",
    "ToolMcpToolAllowedToolsMcpToolFilter",
    "ToolMcpToolRequireApproval",
    "ToolMcpToolRequireApprovalMcpToolApprovalFilter",
    "ToolMcpToolRequireApprovalMcpToolApprovalFilterAlways",
    "ToolMcpToolRequireApprovalMcpToolApprovalFilterNever",
class AudioInputNoiseReduction(BaseModel):
class AudioInputTurnDetectionServerVad(BaseModel):
class AudioInputTurnDetectionSemanticVad(BaseModel):
AudioInputTurnDetection: TypeAlias = Annotated[
    Union[AudioInputTurnDetectionServerVad, AudioInputTurnDetectionSemanticVad, None],
class AudioInput(BaseModel):
    noise_reduction: Optional[AudioInputNoiseReduction] = None
    turn_detection: Optional[AudioInputTurnDetection] = None
    input: Optional[AudioInput] = None
class ToolMcpToolAllowedToolsMcpToolFilter(BaseModel):
ToolMcpToolAllowedTools: TypeAlias = Union[List[str], ToolMcpToolAllowedToolsMcpToolFilter, None]
class ToolMcpToolRequireApprovalMcpToolApprovalFilterAlways(BaseModel):
class ToolMcpToolRequireApprovalMcpToolApprovalFilterNever(BaseModel):
class ToolMcpToolRequireApprovalMcpToolApprovalFilter(BaseModel):
    always: Optional[ToolMcpToolRequireApprovalMcpToolApprovalFilterAlways] = None
    never: Optional[ToolMcpToolRequireApprovalMcpToolApprovalFilterNever] = None
ToolMcpToolRequireApproval: TypeAlias = Union[
    ToolMcpToolRequireApprovalMcpToolApprovalFilter, Literal["always", "never"], None
class ToolMcpTool(BaseModel):
    allowed_tools: Optional[ToolMcpToolAllowedTools] = None
    require_approval: Optional[ToolMcpToolRequireApproval] = None
Tool: TypeAlias = Union[RealtimeFunctionTool, ToolMcpTool]
    """Granular configuration for tracing."""
    Traces Dashboard.
    The arbitrary metadata to attach to this trace to enable filtering in the Traces
    Dashboard.
    This is used to name the trace in the Traces Dashboard.
Tracing: TypeAlias = Union[Literal["auto"], TracingTracingConfiguration, None]
class RealtimeSessionCreateResponse(BaseModel):
    """A new Realtime session configuration, with an ephemeral key.
    Default TTL
    for keys is one minute.
    client_secret: RealtimeSessionClientSecret
