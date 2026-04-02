__all__ = ["RealtimeAudioConfigOutputParam", "Voice", "VoiceID"]
class RealtimeAudioConfigOutputParam(TypedDict, total=False):
    voice: Voice
__all__ = ["RealtimeAudioConfigOutputParam"]
    voice: Union[str, Literal["alloy", "ash", "ballad", "coral", "echo", "sage", "shimmer", "verse", "marin", "cedar"]]
