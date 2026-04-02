    "TranscriptionSessionUpdateParam",
class SessionClientSecretExpiresAt(TypedDict, total=False):
    expires_at: SessionClientSecretExpiresAt
class TranscriptionSessionUpdateParam(TypedDict, total=False):
    type: Required[Literal["transcription_session.update"]]
