__all__ = ["InputAudioBufferCommitEventParam"]
class InputAudioBufferCommitEventParam(TypedDict, total=False):
    type: Required[Literal["input_audio_buffer.commit"]]
