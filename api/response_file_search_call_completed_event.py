__all__ = ["ResponseFileSearchCallCompletedEvent"]
class ResponseFileSearchCallCompletedEvent(BaseModel):
    """Emitted when a file search call is completed (results found)."""
    """The ID of the output item that the file search call is initiated."""
    """The index of the output item that the file search call is initiated."""
    type: Literal["response.file_search_call.completed"]
    """The type of the event. Always `response.file_search_call.completed`."""
