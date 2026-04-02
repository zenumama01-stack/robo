    "SessionUpdateEvent",
    "SessionClientSecret",
    "SessionClientSecretExpiresAfter",
    "SessionInputAudioNoiseReduction",
    "SessionInputAudioTranscription",
    "SessionTool",
    "SessionTracing",
    "SessionTracingTracingConfiguration",
    "SessionTurnDetection",
class SessionClientSecretExpiresAfter(BaseModel):
    anchor: Literal["created_at"]
    seconds: Optional[int] = None
class SessionClientSecret(BaseModel):
    expires_after: Optional[SessionClientSecretExpiresAfter] = None
class SessionInputAudioNoiseReduction(BaseModel):
class SessionInputAudioTranscription(BaseModel):
class SessionTool(BaseModel):
class SessionTracingTracingConfiguration(BaseModel):
SessionTracing: TypeAlias = Union[Literal["auto"], SessionTracingTracingConfiguration]
class SessionTurnDetection(BaseModel):
    client_secret: Optional[SessionClientSecret] = None
    input_audio_noise_reduction: Optional[SessionInputAudioNoiseReduction] = None
    input_audio_transcription: Optional[SessionInputAudioTranscription] = None
    tools: Optional[List[SessionTool]] = None
    tracing: Optional[SessionTracing] = None
    turn_detection: Optional[SessionTurnDetection] = None
class SessionUpdateEvent(BaseModel):
    type: Literal["session.update"]
    """The event type, must be `session.update`."""
__all__ = ["SessionUpdateEvent", "Session"]
    """Update the Realtime session.
    """Optional client-generated ID used to identify this event.
    This is an arbitrary string that a client may assign. It will be passed back if
    there is an error with the event, but the corresponding `session.updated` event
    will not include it.
