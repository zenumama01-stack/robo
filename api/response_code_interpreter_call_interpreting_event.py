__all__ = ["ResponseCodeInterpreterCallInterpretingEvent"]
class ResponseCodeInterpreterCallInterpretingEvent(BaseModel):
    """Emitted when the code interpreter is actively interpreting the code snippet."""
    The index of the output item in the response for which the code interpreter is
    interpreting code.
    type: Literal["response.code_interpreter_call.interpreting"]
    """The type of the event. Always `response.code_interpreter_call.interpreting`."""
