__all__ = ["ResponseFileSearchCallInProgressEvent"]
class ResponseFileSearchCallInProgressEvent(BaseModel):
    """Emitted when a file search call is initiated."""
    type: Literal["response.file_search_call.in_progress"]
    """The type of the event. Always `response.file_search_call.in_progress`."""
