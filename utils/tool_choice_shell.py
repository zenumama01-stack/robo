__all__ = ["ToolChoiceShell"]
class ToolChoiceShell(BaseModel):
    """Forces the model to call the shell tool when a tool call is required."""
    """The tool to call. Always `shell`."""
