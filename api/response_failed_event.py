__all__ = ["ResponseFailedEvent"]
class ResponseFailedEvent(BaseModel):
    """An event that is emitted when a response fails."""
    """The response that failed."""
    type: Literal["response.failed"]
    """The type of the event. Always `response.failed`."""
