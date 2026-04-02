__all__ = ["InputAudioBufferClearEventParam"]
class InputAudioBufferClearEventParam(TypedDict, total=False):
    type: Required[Literal["input_audio_buffer.clear"]]
