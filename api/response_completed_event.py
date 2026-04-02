__all__ = ["ResponseCompletedEvent"]
class ResponseCompletedEvent(BaseModel):
    """Emitted when the model response is complete."""
    """Properties of the completed response."""
    """The sequence number for this event."""
    type: Literal["response.completed"]
    """The type of the event. Always `response.completed`."""
