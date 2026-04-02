__all__ = ["ResponseErrorEvent"]
class ResponseErrorEvent(BaseModel):
    """Emitted when an error occurs."""
    """The error parameter."""
    """The type of the event. Always `error`."""
