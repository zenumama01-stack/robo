__all__ = ["RealtimeAudioInputTurnDetectionParam", "ServerVad", "SemanticVad"]
class ServerVad(TypedDict, total=False):
    idle_timeout_ms: Optional[int]
class SemanticVad(TypedDict, total=False):
    type: Required[Literal["semantic_vad"]]
RealtimeAudioInputTurnDetectionParam: TypeAlias = Union[ServerVad, SemanticVad]
