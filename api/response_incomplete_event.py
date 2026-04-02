__all__ = ["ResponseIncompleteEvent"]
class ResponseIncompleteEvent(BaseModel):
    """An event that is emitted when a response finishes as incomplete."""
    """The response that was incomplete."""
    type: Literal["response.incomplete"]
    """The type of the event. Always `response.incomplete`."""
