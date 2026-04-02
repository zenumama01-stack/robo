__all__ = ["RealtimeAudioConfigOutput", "Voice", "VoiceID"]
class VoiceID(BaseModel):
class RealtimeAudioConfigOutput(BaseModel):
    """The format of the output audio."""
    The speed of the model's spoken response as a multiple of the original speed.
    This parameter is a post-processing adjustment to the audio after it is
    generated, it's also possible to prompt the model to speak faster or slower.
    voice: Optional[Voice] = None
    Supported built-in voices are `alloy`, `ash`, `ballad`, `coral`, `echo`, `sage`,
    object with an `id`, for example `{ "id": "voice_1234" }`. Voice cannot be
    changed during the session once the model has responded with audio at least
    once. We recommend `marin` and `cedar` for best quality.
__all__ = ["RealtimeAudioConfigOutput"]
    `shimmer`, `verse`, `marin`, and `cedar`. Voice cannot be changed during the
    session once the model has responded with audio at least once. We recommend
