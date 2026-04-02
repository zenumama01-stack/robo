__all__ = ["ResponseRefusalDoneEvent"]
class ResponseRefusalDoneEvent(BaseModel):
    """Emitted when refusal text is finalized."""
    """The index of the content part that the refusal text is finalized."""
    """The ID of the output item that the refusal text is finalized."""
    """The index of the output item that the refusal text is finalized."""
    """The refusal text that is finalized."""
    type: Literal["response.refusal.done"]
    """The type of the event. Always `response.refusal.done`."""
