__all__ = ["BatchCancelledWebhookEvent", "Data"]
    """Event data payload."""
    """The unique ID of the batch API request."""
class BatchCancelledWebhookEvent(BaseModel):
    """Sent when a batch API request has been cancelled."""
    """The unique ID of the event."""
    """The Unix timestamp (in seconds) of when the batch API request was cancelled."""
    data: Data
    type: Literal["batch.cancelled"]
    """The type of the event. Always `batch.cancelled`."""
    object: Optional[Literal["event"]] = None
    """The object of the event. Always `event`."""
