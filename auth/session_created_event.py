from .session import Session
__all__ = ["SessionCreatedEvent"]
class SessionCreatedEvent(BaseModel):
    session: Session
    """Realtime session object configuration."""
    type: Literal["session.created"]
    """The event type, must be `session.created`."""
from .realtime_session_create_request import RealtimeSessionCreateRequest
from .realtime_transcription_session_create_request import RealtimeTranscriptionSessionCreateRequest
__all__ = ["SessionCreatedEvent", "Session"]
Session: TypeAlias = Union[RealtimeSessionCreateRequest, RealtimeTranscriptionSessionCreateRequest]
    """Returned when a Session is created.
    Emitted automatically when a new
    connection is established as the first server event. This event will contain
    the default Session configuration.
    """The session configuration."""
