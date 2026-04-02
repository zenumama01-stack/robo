from .realtime_tools_config_union import RealtimeToolsConfigUnion
__all__ = ["RealtimeToolsConfig"]
RealtimeToolsConfig: TypeAlias = List[RealtimeToolsConfigUnion]
