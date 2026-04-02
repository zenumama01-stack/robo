from .chat_session_workflow_param import ChatSessionWorkflowParam
from .chat_session_rate_limits_param import ChatSessionRateLimitsParam
from .chat_session_expires_after_param import ChatSessionExpiresAfterParam
from .chat_session_chatkit_configuration_param import ChatSessionChatKitConfigurationParam
__all__ = ["SessionCreateParams"]
class SessionCreateParams(TypedDict, total=False):
    user: Required[str]
    A free-form string that identifies your end user; ensures this Session can
    workflow: Required[ChatSessionWorkflowParam]
    """Workflow that powers the session."""
    chatkit_configuration: ChatSessionChatKitConfigurationParam
    """Optional overrides for ChatKit runtime configuration features"""
    expires_after: ChatSessionExpiresAfterParam
    """Optional override for session expiration timing in seconds from creation.
    rate_limits: ChatSessionRateLimitsParam
    """Optional override for per-minute request limits. When omitted, defaults to 10."""
    "SessionCreateParams",
    "ClientSecret",
    "ClientSecretExpiresAfter",
    client_secret: ClientSecret
    """Configuration options for the generated client secret."""
    input_audio_format: Literal["pcm16", "g711_ulaw", "g711_alaw"]
    input_audio_noise_reduction: InputAudioNoiseReduction
    input_audio_transcription: InputAudioTranscription
    tools: Iterable[Tool]
    turn_detection: TurnDetection
class ClientSecretExpiresAfter(TypedDict, total=False):
    """The anchor point for the ephemeral token expiration.
    Only `created_at` is currently supported.
    seconds: int
    """The number of seconds from the anchor point to the expiration.
    Select a value between `10` and `7200`.
class ClientSecret(TypedDict, total=False):
    expires_after: ClientSecretExpiresAfter
    """Configuration for the ephemeral token expiration."""
class InputAudioNoiseReduction(TypedDict, total=False):
    type: Literal["near_field", "far_field"]
class InputAudioTranscription(TypedDict, total=False):
class Tool(TypedDict, total=False):
class TracingTracingConfiguration(TypedDict, total=False):
    group_id: str
    metadata: object
    workflow_name: str
class TurnDetection(TypedDict, total=False):
    create_response: bool
    eagerness: Literal["low", "medium", "high", "auto"]
    interrupt_response: bool
    type: Literal["server_vad", "semantic_vad"]
