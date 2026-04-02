__all__ = ["ResponseReasoningItem", "Summary", "Content"]
class Summary(BaseModel):
class ResponseReasoningItem(BaseModel):
    A description of the chain of thought used by a reasoning model while generating
    a response. Be sure to include these items in your `input` to the Responses API
    for subsequent turns of a conversation if you are manually
    [managing context](https://platform.openai.com/docs/guides/conversation-state).
    """The unique identifier of the reasoning content."""
    summary: List[Summary]
    """Reasoning summary content."""
    type: Literal["reasoning"]
    """The type of the object. Always `reasoning`."""
    """Reasoning text content."""
    encrypted_content: Optional[str] = None
    The encrypted content of the reasoning item - populated when a response is
    generated with `reasoning.encrypted_content` in the `include` parameter.
