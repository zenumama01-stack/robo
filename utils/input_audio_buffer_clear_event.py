__all__ = ["InputAudioBufferClearEvent"]
class InputAudioBufferClearEvent(BaseModel):
    type: Literal["input_audio_buffer.clear"]
    """The event type, must be `input_audio_buffer.clear`."""
