__all__ = ["ResponseAudioTranscriptDoneEvent"]
class ResponseAudioTranscriptDoneEvent(BaseModel):
    """The final transcript of the audio."""
    type: Literal["response.audio_transcript.done"]
    """The event type, must be `response.audio_transcript.done`."""
    Returned when the model-generated transcription of audio output is done
    streaming. Also emitted when a Response is interrupted, incomplete, or
    cancelled.
    type: Literal["response.output_audio_transcript.done"]
    """The event type, must be `response.output_audio_transcript.done`."""
    """Emitted when the full audio transcript is completed."""
    type: Literal["response.audio.transcript.done"]
    """The type of the event. Always `response.audio.transcript.done`."""
