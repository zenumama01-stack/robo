    "SessionUpdateEventParam",
class SessionClientSecretExpiresAfter(TypedDict, total=False):
class SessionClientSecret(TypedDict, total=False):
    expires_after: SessionClientSecretExpiresAfter
class SessionInputAudioNoiseReduction(TypedDict, total=False):
class SessionInputAudioTranscription(TypedDict, total=False):
class SessionTool(TypedDict, total=False):
class SessionTracingTracingConfiguration(TypedDict, total=False):
class SessionTurnDetection(TypedDict, total=False):
class Session(TypedDict, total=False):
    client_secret: SessionClientSecret
    input_audio_noise_reduction: SessionInputAudioNoiseReduction
    input_audio_transcription: SessionInputAudioTranscription
    tools: Iterable[SessionTool]
    tracing: SessionTracing
    turn_detection: SessionTurnDetection
class SessionUpdateEventParam(TypedDict, total=False):
    session: Required[Session]
    type: Required[Literal["session.update"]]
__all__ = ["SessionUpdateEventParam", "Session"]
