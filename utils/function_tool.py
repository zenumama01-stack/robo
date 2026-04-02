from ..shared.function_definition import FunctionDefinition
__all__ = ["FunctionTool"]
class FunctionTool(BaseModel):
    function: FunctionDefinition
    type: Literal["function"]
    """The type of tool being defined: `function`"""
    """Defines a function in your own code the model can choose to call.
    Learn more about [function calling](https://platform.openai.com/docs/guides/function-calling).
    parameters: Optional[Dict[str, object]] = None
    """A JSON schema object describing the parameters of the function."""
    strict: Optional[bool] = None
    """Whether to enforce strict parameter validation. Default `true`."""
    """The type of the function tool. Always `function`."""
    """Whether this function is deferred and loaded via tool search."""
    """A description of the function.
    Used by the model to determine whether or not to call the function.
