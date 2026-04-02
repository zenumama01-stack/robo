__all__ = ["RealtimeConversationItemFunctionCall"]
class RealtimeConversationItemFunctionCall(BaseModel):
    """A function call item in a Realtime conversation."""
    """The arguments of the function call.
    This is a JSON-encoded string representing the arguments passed to the function,
    for example `{"arg1": "value1", "arg2": 42}`.
    """The name of the function being called."""
    type: Literal["function_call"]
    """The type of the item. Always `function_call`."""
