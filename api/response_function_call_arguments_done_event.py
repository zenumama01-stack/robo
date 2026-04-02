__all__ = ["ResponseFunctionCallArgumentsDoneEvent"]
class ResponseFunctionCallArgumentsDoneEvent(BaseModel):
    """The final arguments as a JSON string."""
    type: Literal["response.function_call_arguments.done"]
    """The event type, must be `response.function_call_arguments.done`."""
    Returned when the model-generated function call arguments are done streaming.
    """The name of the function that was called."""
    """Emitted when function-call arguments are finalized."""
    """The function-call arguments."""
    """The index of the output item."""
