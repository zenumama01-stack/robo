__all__ = ["RealtimeTracingConfig", "TracingConfiguration"]
class TracingConfiguration(BaseModel):
RealtimeTracingConfig: TypeAlias = Union[Literal["auto"], TracingConfiguration, None]
