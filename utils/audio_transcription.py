__all__ = ["AudioTranscription"]
class AudioTranscription(BaseModel):
    Current options are `whisper-1`, `gpt-4o-mini-transcribe`,
    `gpt-4o-mini-transcribe-2025-12-15`, `gpt-4o-transcribe`, and
    `gpt-4o-transcribe-diarize`. Use `gpt-4o-transcribe-diarize` when you need
    diarization with speaker labels.
    For `gpt-4o-transcribe` models (excluding `gpt-4o-transcribe-diarize`), the
    prompt is a free text string, for example "expect words related to technology".
