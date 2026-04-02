__all__ = ["ResponseRefusalDeltaEvent"]
class ResponseRefusalDeltaEvent(BaseModel):
    """Emitted when there is a partial refusal text."""
    """The index of the content part that the refusal text is added to."""
    """The refusal text that is added."""
    """The ID of the output item that the refusal text is added to."""
    """The index of the output item that the refusal text is added to."""
    type: Literal["response.refusal.delta"]
    """The type of the event. Always `response.refusal.delta`."""
