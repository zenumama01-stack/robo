__all__ = ["ConversationItemInputAudioTranscriptionSegment"]
class ConversationItemInputAudioTranscriptionSegment(BaseModel):
    """Returned when an input audio transcription segment is identified for an item."""
    """The segment identifier."""
    """The index of the input audio content part within the item."""
    """The ID of the item containing the input audio content."""
    """The detected speaker label for this segment."""
    """The text for this segment."""
    type: Literal["conversation.item.input_audio_transcription.segment"]
    """The event type, must be `conversation.item.input_audio_transcription.segment`."""
