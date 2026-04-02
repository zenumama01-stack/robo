__all__ = ["ResponseAudioDeltaEvent"]
class ResponseAudioDeltaEvent(BaseModel):
    """Base64-encoded audio data delta."""
    output_index: int
    """The index of the output item in the response."""
    """The ID of the response."""
    type: Literal["response.audio.delta"]
    """The event type, must be `response.audio.delta`."""
    """Returned when the model-generated audio is updated."""
    type: Literal["response.output_audio.delta"]
    """The event type, must be `response.output_audio.delta`."""
    """Emitted when there is a partial audio response."""
    """A chunk of Base64 encoded response audio bytes."""
    sequence_number: int
    """A sequence number for this chunk of the stream response."""
    """The type of the event. Always `response.audio.delta`."""
