from .realtime_session_create_response import RealtimeSessionCreateResponse
from .realtime_transcription_session_create_response import RealtimeTranscriptionSessionCreateResponse
__all__ = ["ClientSecretCreateResponse", "Session"]
Session: TypeAlias = Annotated[
    Union[RealtimeSessionCreateResponse, RealtimeTranscriptionSessionCreateResponse], PropertyInfo(discriminator="type")
class ClientSecretCreateResponse(BaseModel):
    """Response from creating a session and client secret for the Realtime API."""
    """Expiration timestamp for the client secret, in seconds since epoch."""
    """The session configuration for either a realtime or transcription session."""
    """The generated client secret value."""
