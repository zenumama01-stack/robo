__all__ = ["RequiredActionFunctionToolCall", "Function"]
class Function(BaseModel):
    """The function definition."""
    """The arguments that the model expects you to pass to the function."""
class RequiredActionFunctionToolCall(BaseModel):
    """Tool call objects"""
    """The ID of the tool call.
    This ID must be referenced when you submit the tool outputs in using the
    [Submit tool outputs to run](https://platform.openai.com/docs/api-reference/runs/submitToolOutputs)
    function: Function
    """The type of tool call the output is required for.
    For now, this is always `function`.
