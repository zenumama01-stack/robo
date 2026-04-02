__all__ = ["BatchFailedWebhookEvent", "Data"]
class BatchFailedWebhookEvent(BaseModel):
    """Sent when a batch API request has failed."""
    """The Unix timestamp (in seconds) of when the batch API request failed."""
    type: Literal["batch.failed"]
    """The type of the event. Always `batch.failed`."""
