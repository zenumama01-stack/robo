__all__ = ["ResponseQueuedEvent"]
class ResponseQueuedEvent(BaseModel):
    """Emitted when a response is queued and waiting to be processed."""
    """The full response object that is queued."""
    type: Literal["response.queued"]
    """The type of the event. Always 'response.queued'."""
