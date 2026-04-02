__all__ = ["SessionUpdatedEvent"]
class SessionUpdatedEvent(BaseModel):
    type: Literal["session.updated"]
    """The event type, must be `session.updated`."""
__all__ = ["SessionUpdatedEvent", "Session"]
    Returned when a session is updated with a `session.update` event, unless
    there is an error.
