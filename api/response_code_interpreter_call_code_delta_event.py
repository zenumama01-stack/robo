__all__ = ["ResponseCodeInterpreterCallCodeDeltaEvent"]
class ResponseCodeInterpreterCallCodeDeltaEvent(BaseModel):
    """Emitted when a partial code snippet is streamed by the code interpreter."""
    """The partial code snippet being streamed by the code interpreter."""
    """The unique identifier of the code interpreter tool call item."""
    The index of the output item in the response for which the code is being
    streamed.
    """The sequence number of this event, used to order streaming events."""
    type: Literal["response.code_interpreter_call_code.delta"]
    """The type of the event. Always `response.code_interpreter_call_code.delta`."""
