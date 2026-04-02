__all__ = ["ResponseIncompleteWebhookEvent", "Data"]
class ResponseIncompleteWebhookEvent(BaseModel):
    """Sent when a background response has been interrupted."""
    """The Unix timestamp (in seconds) of when the model response was interrupted."""
