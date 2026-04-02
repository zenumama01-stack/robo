from ..._utils import PropertyInfo
__all__ = ["Transcription", "Logprob", "Usage", "UsageTokens", "UsageTokensInputTokenDetails", "UsageDuration"]
class Logprob(BaseModel):
    token: Optional[str] = None
    """The token in the transcription."""
    bytes: Optional[List[float]] = None
    """The bytes of the token."""
    logprob: Optional[float] = None
    """The log probability of the token."""
class UsageTokensInputTokenDetails(BaseModel):
    """Details about the input tokens billed for this request."""
    """Number of audio tokens billed for this request."""
    text_tokens: Optional[int] = None
    """Number of text tokens billed for this request."""
class UsageTokens(BaseModel):
    """Usage statistics for models billed by token usage."""
    """Number of input tokens billed for this request."""
    """Number of output tokens generated."""
    """Total number of tokens used (input + output)."""
    type: Literal["tokens"]
    """The type of the usage object. Always `tokens` for this variant."""
    input_token_details: Optional[UsageTokensInputTokenDetails] = None
class UsageDuration(BaseModel):
    """Usage statistics for models billed by audio input duration."""
    seconds: float
    """Duration of the input audio in seconds."""
    type: Literal["duration"]
    """The type of the usage object. Always `duration` for this variant."""
Usage: TypeAlias = Annotated[Union[UsageTokens, UsageDuration], PropertyInfo(discriminator="type")]
class Transcription(BaseModel):
    Represents a transcription response returned by model, based on the provided input.
    """The transcribed text."""
    logprobs: Optional[List[Logprob]] = None
    """The log probabilities of the tokens in the transcription.
    Only returned with the models `gpt-4o-transcribe` and `gpt-4o-mini-transcribe`
    if `logprobs` is added to the `include` array.
    """Token usage statistics for the request."""
