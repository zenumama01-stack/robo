__all__ = ["ResponseCodeInterpreterCallCodeDoneEvent"]
class ResponseCodeInterpreterCallCodeDoneEvent(BaseModel):
    """Emitted when the code snippet is finalized by the code interpreter."""
    """The final code snippet output by the code interpreter."""
    """The index of the output item in the response for which the code is finalized."""
    type: Literal["response.code_interpreter_call_code.done"]
    """The type of the event. Always `response.code_interpreter_call_code.done`."""
