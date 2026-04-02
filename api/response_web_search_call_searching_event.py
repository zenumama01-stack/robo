__all__ = ["ResponseWebSearchCallSearchingEvent"]
class ResponseWebSearchCallSearchingEvent(BaseModel):
    """Emitted when a web search call is executing."""
    type: Literal["response.web_search_call.searching"]
    """The type of the event. Always `response.web_search_call.searching`."""
