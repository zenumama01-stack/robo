__all__ = ["FunctionToolCall", "Function"]
    """The definition of the function that was called."""
    """The arguments passed to the function."""
    """The output of the function.
    This will be `null` if the outputs have not been
    [submitted](https://platform.openai.com/docs/api-reference/runs/submitToolOutputs)
    yet.
class FunctionToolCall(BaseModel):
    This is always going to be `function` for this type of tool call.
