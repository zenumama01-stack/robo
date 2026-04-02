    "ConversationItemInputAudioTranscriptionCompletedEvent",
    "Usage",
    "UsageTranscriptTextUsageTokens",
    "UsageTranscriptTextUsageTokensInputTokenDetails",
    "UsageTranscriptTextUsageDuration",
    "Logprob",
class UsageTranscriptTextUsageTokensInputTokenDetails(BaseModel):
class UsageTranscriptTextUsageTokens(BaseModel):
    input_token_details: Optional[UsageTranscriptTextUsageTokensInputTokenDetails] = None
class UsageTranscriptTextUsageDuration(BaseModel):
Usage: TypeAlias = Union[UsageTranscriptTextUsageTokens, UsageTranscriptTextUsageDuration]
    token: str
    bytes: List[int]
    logprob: float
class ConversationItemInputAudioTranscriptionCompletedEvent(BaseModel):
    content_index: int
    """The index of the content part containing the audio."""
    """The ID of the user message item containing the audio."""
    type: Literal["conversation.item.input_audio_transcription.completed"]
    The event type, must be `conversation.item.input_audio_transcription.completed`.
    """Usage statistics for the transcription."""
    """The log probabilities of the transcription."""
from .log_prob_properties import LogProbProperties
    This event is the output of audio transcription for user audio written to the
    user audio buffer. Transcription begins when the input audio buffer is
    committed by the client or server (when VAD is enabled). Transcription runs
    asynchronously with Response creation, so this event may come before or after
    the Response events.
    Realtime API models accept audio natively, and thus input transcription is a
    separate process run on a separate ASR (Automatic Speech Recognition) model.
    The transcript may diverge somewhat from the model's interpretation, and
    should be treated as a rough guide.
    """The ID of the item containing the audio that is being transcribed."""
    Usage statistics for the transcription, this is billed according to the ASR
    model's pricing rather than the realtime model's pricing.
    logprobs: Optional[List[LogProbProperties]] = None
