__all__ = ["ResponseCodeInterpreterCallInProgressEvent"]
class ResponseCodeInterpreterCallInProgressEvent(BaseModel):
    """Emitted when a code interpreter call is in progress."""
    is in progress.
    type: Literal["response.code_interpreter_call.in_progress"]
    """The type of the event. Always `response.code_interpreter_call.in_progress`."""
