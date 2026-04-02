    "SessionCreateResponse",
class ClientSecret(BaseModel):
    """Timestamp for when the token expires.
    Currently, all tokens expire after one minute.
    value: str
    Ephemeral key usable in client environments to authenticate connections to the
    Realtime API. Use this in client-side environments rather than a standard API
    token, which should only be used server-side.
    """The model to use for transcription."""
    """Amount of audio to include before the VAD detected speech (in milliseconds).
    """Duration of silence to detect speech stop (in milliseconds).
    Defaults to 500ms. With shorter values the model will respond more quickly, but
    may jump in on short pauses from the user.
    """Activation threshold for VAD (0.0 to 1.0), this defaults to 0.5.
    """Type of turn detection, only `server_vad` is currently supported."""
class SessionCreateResponse(BaseModel):
    """Ephemeral key returned by the API."""
    input_audio_format: Optional[str] = None
    """The format of input audio. Options are `pcm16`, `g711_ulaw`, or `g711_alaw`."""
    asynchronously and should be treated as rough guidance rather than the
    representation understood by the model.
    output_audio_format: Optional[str] = None
    """Configuration for turn detection.
    Can be set to `null` to turn off. Server VAD means that the model will detect
    the start and end of speech based on audio volume and respond at the end of user
    speech.
