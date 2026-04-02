__all__ = ["RealtimeAudioFormatsParam", "AudioPCM", "AudioPCMU", "AudioPCMA"]
class AudioPCM(TypedDict, total=False):
    rate: Literal[24000]
    type: Literal["audio/pcm"]
class AudioPCMU(TypedDict, total=False):
    type: Literal["audio/pcmu"]
class AudioPCMA(TypedDict, total=False):
    type: Literal["audio/pcma"]
RealtimeAudioFormatsParam: TypeAlias = Union[AudioPCM, AudioPCMU, AudioPCMA]
