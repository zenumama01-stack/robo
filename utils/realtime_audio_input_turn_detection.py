__all__ = ["RealtimeAudioInputTurnDetection", "ServerVad", "SemanticVad"]
class ServerVad(BaseModel):
    Server-side voice activity detection (VAD) which flips on when user speech is detected and off after a period of silence.
    type: Literal["server_vad"]
    """Type of turn detection, `server_vad` to turn on simple Server VAD."""
    If `interrupt_response` is set to `false` this may fail to create a response if
    the model is already responding.
    If both `create_response` and `interrupt_response` are set to `false`, the model
    will never respond automatically but VAD events will still be emitted.
    idle_timeout_ms: Optional[int] = None
    """Optional timeout after which a model response will be triggered automatically.
    This is useful for situations in which a long pause from the user is unexpected,
    such as a phone call. The model will effectively prompt the user to continue the
    conversation based on the current context.
    The timeout value will be applied after the last model response's audio has
    finished playing, i.e. it's set to the `response.done` time plus audio playback
    duration.
    An `input_audio_buffer.timeout_triggered` event (plus events associated with the
    Response) will be emitted when the timeout is reached. Idle timeout is currently
    only supported for `server_vad` mode.
    Whether or not to automatically interrupt (cancel) any ongoing response with
    output to the default conversation (i.e. `conversation` of `auto`) when a VAD
    start event occurs. If `true` then the response will be cancelled, otherwise it
    will continue until complete.
class SemanticVad(BaseModel):
    Server-side semantic turn detection which uses a model to determine when the user has finished speaking.
    type: Literal["semantic_vad"]
    """Type of turn detection, `semantic_vad` to turn on Semantic VAD."""
    is equivalent to `medium`. `low`, `medium`, and `high` have max timeouts of 8s,
    4s, and 2s respectively.
RealtimeAudioInputTurnDetection: TypeAlias = Annotated[
    Union[ServerVad, SemanticVad, None], PropertyInfo(discriminator="type")
