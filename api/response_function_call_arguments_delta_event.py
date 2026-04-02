__all__ = ["ResponseFunctionCallArgumentsDeltaEvent"]
class ResponseFunctionCallArgumentsDeltaEvent(BaseModel):
    """The ID of the function call."""
    """The arguments delta as a JSON string."""
    """The ID of the function call item."""
    type: Literal["response.function_call_arguments.delta"]
    """The event type, must be `response.function_call_arguments.delta`."""
    """Returned when the model-generated function call arguments are updated."""
    """Emitted when there is a partial function-call arguments delta."""
    """The function-call arguments delta that is added."""
    """The ID of the output item that the function-call arguments delta is added to."""
    The index of the output item that the function-call arguments delta is added to.
    """The type of the event. Always `response.function_call_arguments.delta`."""
