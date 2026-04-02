__all__ = ["ResponsePrompt", "Variables"]
Variables: TypeAlias = Union[str, ResponseInputText, ResponseInputImage, ResponseInputFile]
class ResponsePrompt(BaseModel):
    """The unique identifier of the prompt template to use."""
    variables: Optional[Dict[str, Variables]] = None
    """Optional map of values to substitute in for variables in your prompt.
    The substitution values can either be strings, or other Response input types
    like images or files.
    """Optional version of the prompt template."""
