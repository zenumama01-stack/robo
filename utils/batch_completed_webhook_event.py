__all__ = ["BatchCompletedWebhookEvent", "Data"]
class BatchCompletedWebhookEvent(BaseModel):
    """Sent when a batch API request has been completed."""
    """The Unix timestamp (in seconds) of when the batch API request was completed."""
    type: Literal["batch.completed"]
    """The type of the event. Always `batch.completed`."""
