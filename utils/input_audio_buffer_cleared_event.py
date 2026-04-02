__all__ = ["InputAudioBufferClearedEvent"]
class InputAudioBufferClearedEvent(BaseModel):
    type: Literal["input_audio_buffer.cleared"]
    """The event type, must be `input_audio_buffer.cleared`."""
    Returned when the input audio buffer is cleared by the client with a
    `input_audio_buffer.clear` event.
