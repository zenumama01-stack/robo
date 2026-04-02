__all__ = ["InputAudioBufferAppendEvent"]
class InputAudioBufferAppendEvent(BaseModel):
    """Base64-encoded audio bytes.
    This must be in the format specified by the `input_audio_format` field in the
    session configuration.
    type: Literal["input_audio_buffer.append"]
    """The event type, must be `input_audio_buffer.append`."""
