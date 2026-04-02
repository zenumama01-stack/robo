from .realtime_error import RealtimeError
__all__ = ["RealtimeErrorEvent"]
class RealtimeErrorEvent(BaseModel):
    Returned when an error occurs, which could be a client problem or a server
    problem. Most errors are recoverable and the session will stay open, we
    recommend to implementors to monitor and log error messages by default.
    error: RealtimeError
