__all__ = ["RealtimeConversationItemFunctionCallOutput"]
class RealtimeConversationItemFunctionCallOutput(BaseModel):
    """A function call output item in a Realtime conversation."""
    """The ID of the function call this output is for."""
    The output of the function call, this is free text and can contain any
    information or simply be empty.
    """The type of the item. Always `function_call_output`."""
