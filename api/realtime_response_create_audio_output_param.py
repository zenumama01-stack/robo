__all__ = ["RealtimeResponseCreateAudioOutputParam", "Output", "OutputVoice", "OutputVoiceID"]
class OutputVoiceID(TypedDict, total=False):
class Output(TypedDict, total=False):
    voice: OutputVoice
class RealtimeResponseCreateAudioOutputParam(TypedDict, total=False):
    output: Output
__all__ = ["RealtimeResponseCreateAudioOutputParam", "Output"]
