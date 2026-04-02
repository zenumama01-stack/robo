    "TranscriptionSessionUpdate",
    "SessionClientSecretExpiresAt",
class SessionClientSecretExpiresAt(BaseModel):
    anchor: Optional[Literal["created_at"]] = None
    expires_at: Optional[SessionClientSecretExpiresAt] = None
    include: Optional[List[str]] = None
class TranscriptionSessionUpdate(BaseModel):
    """Realtime transcription session object configuration."""
    type: Literal["transcription_session.update"]
    """The event type, must be `transcription_session.update`."""
