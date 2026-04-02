__all__ = ["RealtimeTracingConfigParam", "TracingConfiguration"]
class TracingConfiguration(TypedDict, total=False):
RealtimeTracingConfigParam: TypeAlias = Union[Literal["auto"], TracingConfiguration]
