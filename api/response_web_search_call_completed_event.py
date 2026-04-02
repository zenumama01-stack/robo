__all__ = ["ResponseWebSearchCallCompletedEvent"]
class ResponseWebSearchCallCompletedEvent(BaseModel):
    """Emitted when a web search call is completed."""
    """Unique ID for the output item associated with the web search call."""
    """The index of the output item that the web search call is associated with."""
    """The sequence number of the web search call being processed."""
    type: Literal["response.web_search_call.completed"]
    """The type of the event. Always `response.web_search_call.completed`."""
