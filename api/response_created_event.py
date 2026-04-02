from .realtime_response import RealtimeResponse
__all__ = ["ResponseCreatedEvent"]
class ResponseCreatedEvent(BaseModel):
    response: RealtimeResponse
    """The response resource."""
    type: Literal["response.created"]
    """The event type, must be `response.created`."""
    """Returned when a new Response is created.
    The first event of response creation,
    where the response is in an initial state of `in_progress`.
    """An event that is emitted when a response is created."""
    """The response that was created."""
    """The type of the event. Always `response.created`."""
