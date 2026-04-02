__all__ = ["RealtimeResponseUsage", "InputTokenDetails", "OutputTokenDetails"]
class InputTokenDetails(BaseModel):
    """The number of audio tokens used in the Response."""
    """The number of cached tokens used in the Response."""
    """The number of text tokens used in the Response."""
class OutputTokenDetails(BaseModel):
class RealtimeResponseUsage(BaseModel):
    input_token_details: Optional[InputTokenDetails] = None
    """Details about the input tokens used in the Response."""
    input_tokens: Optional[int] = None
    The number of input tokens used in the Response, including text and audio
    tokens.
    output_token_details: Optional[OutputTokenDetails] = None
    """Details about the output tokens used in the Response."""
    output_tokens: Optional[int] = None
    The number of output tokens sent in the Response, including text and audio
    total_tokens: Optional[int] = None
    The total number of tokens in the Response including input and output text and
    audio tokens.
from .realtime_response_usage_input_token_details import RealtimeResponseUsageInputTokenDetails
from .realtime_response_usage_output_token_details import RealtimeResponseUsageOutputTokenDetails
__all__ = ["RealtimeResponseUsage"]
    A
    Realtime API session will maintain a conversation context and append new
    Items to the Conversation, thus output from previous turns (text and
    audio tokens) will become the input for later turns.
    input_token_details: Optional[RealtimeResponseUsageInputTokenDetails] = None
    """Details about the input tokens used in the Response.
    Cached tokens are tokens from previous turns in the conversation that are
    included as context for the current response. Cached tokens here are counted as
    a subset of input tokens, meaning input tokens will include cached and uncached
    output_token_details: Optional[RealtimeResponseUsageOutputTokenDetails] = None
