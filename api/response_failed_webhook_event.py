__all__ = ["ResponseFailedWebhookEvent", "Data"]
class ResponseFailedWebhookEvent(BaseModel):
    """Sent when a background response has failed."""
    """The Unix timestamp (in seconds) of when the model response failed."""
