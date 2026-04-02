__all__ = ["ChatCompletionAudioParam", "Voice", "VoiceID"]
class ChatCompletionAudioParam(TypedDict, total=False):
    """Parameters for audio output.
    Required when audio output is requested with
    `modalities: ["audio"]`. [Learn more](https://platform.openai.com/docs/guides/audio).
    format: Required[Literal["wav", "aac", "mp3", "flac", "opus", "pcm16"]]
    """Specifies the output audio format.
    Must be one of `wav`, `mp3`, `flac`, `opus`, or `pcm16`.
    `fable`, `nova`, `onyx`, `sage`, `shimmer`, `marin`, and `cedar`. You may also
    provide a custom voice object with an `id`, for example
    `{ "id": "voice_1234" }`.
__all__ = ["ChatCompletionAudioParam"]
    `fable`, `nova`, `onyx`, `sage`, `shimmer`, `marin`, and `cedar`.
