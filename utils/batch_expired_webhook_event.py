__all__ = ["BatchExpiredWebhookEvent", "Data"]
class BatchExpiredWebhookEvent(BaseModel):
    """Sent when a batch API request has expired."""
    """The Unix timestamp (in seconds) of when the batch API request expired."""
    type: Literal["batch.expired"]
    """The type of the event. Always `batch.expired`."""
