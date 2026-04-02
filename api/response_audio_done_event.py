__all__ = ["ResponseAudioDoneEvent"]
class ResponseAudioDoneEvent(BaseModel):
    type: Literal["response.audio.done"]
    """The event type, must be `response.audio.done`."""
    """Returned when the model-generated audio is done.
    Also emitted when a Response
    is interrupted, incomplete, or cancelled.
    type: Literal["response.output_audio.done"]
    """The event type, must be `response.output_audio.done`."""
    """Emitted when the audio response is complete."""
    """The sequence number of the delta."""
    """The type of the event. Always `response.audio.done`."""
