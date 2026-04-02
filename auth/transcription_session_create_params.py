    "TranscriptionSessionCreateParams",
    "ClientSecretExpiresAt",
class TranscriptionSessionCreateParams(TypedDict, total=False):
    include: List[str]
    """The set of items to include in the transcription. Current available items are:
    """Configuration for input audio transcription.
    The client can optionally set the language and prompt for transcription, these
    offer additional guidance to the transcription service.
class ClientSecretExpiresAt(TypedDict, total=False):
    expires_at: ClientSecretExpiresAt
    model: Literal["gpt-4o-transcribe", "gpt-4o-mini-transcribe", "whisper-1"]
    """Whether or not to automatically generate a response when a VAD stop event
    Not available for transcription sessions.
    occurs. Not available for transcription sessions.
