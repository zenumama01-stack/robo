__all__ = ["ResponseInProgressEvent"]
class ResponseInProgressEvent(BaseModel):
    """Emitted when the response is in progress."""
    """The response that is in progress."""
    type: Literal["response.in_progress"]
    """The type of the event. Always `response.in_progress`."""
