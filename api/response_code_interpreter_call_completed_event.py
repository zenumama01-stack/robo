__all__ = ["ResponseCodeInterpreterCallCompletedEvent"]
class ResponseCodeInterpreterCallCompletedEvent(BaseModel):
    """Emitted when the code interpreter call is completed."""
    The index of the output item in the response for which the code interpreter call
    is completed.
    type: Literal["response.code_interpreter_call.completed"]
    """The type of the event. Always `response.code_interpreter_call.completed`."""
