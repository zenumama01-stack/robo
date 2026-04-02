__all__ = ["ResponseFileSearchCallSearchingEvent"]
class ResponseFileSearchCallSearchingEvent(BaseModel):
    """Emitted when a file search is currently searching."""
    """The index of the output item that the file search call is searching."""
    type: Literal["response.file_search_call.searching"]
    """The type of the event. Always `response.file_search_call.searching`."""
