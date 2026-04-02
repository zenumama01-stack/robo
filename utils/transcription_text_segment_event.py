__all__ = ["TranscriptionTextSegmentEvent"]
class TranscriptionTextSegmentEvent(BaseModel):
    Emitted when a diarized transcription returns a completed segment with speaker information. Only emitted when you [create a transcription](https://platform.openai.com/docs/api-reference/audio/create-transcription) with `stream` set to `true` and `response_format` set to `diarized_json`.
    """Speaker label for this segment."""
    """The type of the event. Always `transcript.text.segment`."""
