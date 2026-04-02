__all__ = ["InputAudioBufferCommitEvent"]
class InputAudioBufferCommitEvent(BaseModel):
    type: Literal["input_audio_buffer.commit"]
    """The event type, must be `input_audio_buffer.commit`."""
