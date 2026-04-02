__all__ = ["InputAudioBufferAppendEventParam"]
class InputAudioBufferAppendEventParam(TypedDict, total=False):
    audio: Required[str]
    type: Required[Literal["input_audio_buffer.append"]]
