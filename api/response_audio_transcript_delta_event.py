__all__ = ["ResponseAudioTranscriptDeltaEvent"]
class ResponseAudioTranscriptDeltaEvent(BaseModel):
    """The transcript delta."""
    type: Literal["response.audio_transcript.delta"]
    """The event type, must be `response.audio_transcript.delta`."""
    """Returned when the model-generated transcription of audio output is updated."""
    type: Literal["response.output_audio_transcript.delta"]
    """The event type, must be `response.output_audio_transcript.delta`."""
    """Emitted when there is a partial transcript of audio."""
    """The partial transcript of the audio response."""
    """The sequence number of this event."""
    type: Literal["response.audio.transcript.delta"]
    """The type of the event. Always `response.audio.transcript.delta`."""
