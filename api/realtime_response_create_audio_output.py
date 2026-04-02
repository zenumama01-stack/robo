__all__ = ["RealtimeResponseCreateAudioOutput", "Output", "OutputVoice", "OutputVoiceID"]
class OutputVoiceID(BaseModel):
OutputVoice: TypeAlias = Union[
    str, Literal["alloy", "ash", "ballad", "coral", "echo", "sage", "shimmer", "verse", "marin", "cedar"], OutputVoiceID
class Output(BaseModel):
    voice: Optional[OutputVoice] = None
class RealtimeResponseCreateAudioOutput(BaseModel):
    """Configuration for audio input and output."""
    output: Optional[Output] = None
__all__ = ["RealtimeResponseCreateAudioOutput", "Output"]
    session once the model has responded with audio at least once.
