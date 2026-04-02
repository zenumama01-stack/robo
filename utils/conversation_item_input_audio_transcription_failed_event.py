__all__ = ["ConversationItemInputAudioTranscriptionFailedEvent", "Error"]
class Error(BaseModel):
    """Error code, if any."""
    """A human-readable error message."""
    """Parameter related to the error, if any."""
    type: Optional[str] = None
    """The type of error."""
class ConversationItemInputAudioTranscriptionFailedEvent(BaseModel):
    error: Error
    """Details of the transcription error."""
    """The ID of the user message item."""
    type: Literal["conversation.item.input_audio_transcription.failed"]
    """The event type, must be `conversation.item.input_audio_transcription.failed`."""
    Returned when input audio transcription is configured, and a transcription
    request for a user message failed. These events are separate from other
    `error` events so that the client can identify the related Item.
