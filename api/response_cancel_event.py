__all__ = ["ResponseCancelEvent"]
class ResponseCancelEvent(BaseModel):
    type: Literal["response.cancel"]
    """The event type, must be `response.cancel`."""
    A specific response ID to cancel - if not provided, will cancel an in-progress
    response in the default conversation.
