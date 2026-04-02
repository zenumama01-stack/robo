__all__ = ["ResponseTextDeltaEvent"]
class ResponseTextDeltaEvent(BaseModel):
    type: Literal["response.text.delta"]
    """The event type, must be `response.text.delta`."""
    """Returned when the text value of an "output_text" content part is updated."""
    type: Literal["response.output_text.delta"]
    """The event type, must be `response.output_text.delta`."""
__all__ = ["ResponseTextDeltaEvent", "Logprob", "LogprobTopLogprob"]
    """A possible text token."""
    """The log probability of this token."""
    A logprob is the logarithmic probability that the model assigns to producing
    a particular token at a given position in the sequence. Less-negative (higher)
    logprob values indicate greater model confidence in that token choice.
    top_logprobs: Optional[List[LogprobTopLogprob]] = None
    """The log probability of the top 20 most likely tokens."""
    """Emitted when there is an additional text delta."""
    """The index of the content part that the text delta was added to."""
    """The text delta that was added."""
    """The ID of the output item that the text delta was added to."""
    logprobs: List[Logprob]
    """The log probabilities of the tokens in the delta."""
    """The index of the output item that the text delta was added to."""
    """The type of the event. Always `response.output_text.delta`."""
