__all__ = ["ResponseCancelledWebhookEvent", "Data"]
    """The unique ID of the model response."""
class ResponseCancelledWebhookEvent(BaseModel):
    """Sent when a background response has been cancelled."""
    """The Unix timestamp (in seconds) of when the model response was cancelled."""
    type: Literal["response.cancelled"]
    """The type of the event. Always `response.cancelled`."""
