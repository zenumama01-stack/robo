__all__ = ["ResponseWebSearchCallInProgressEvent"]
class ResponseWebSearchCallInProgressEvent(BaseModel):
    """Emitted when a web search call is initiated."""
    type: Literal["response.web_search_call.in_progress"]
    """The type of the event. Always `response.web_search_call.in_progress`."""
