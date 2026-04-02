__all__ = ["ToolChoiceApplyPatch"]
class ToolChoiceApplyPatch(BaseModel):
    """Forces the model to call the apply_patch tool when executing a tool call."""
    """The tool to call. Always `apply_patch`."""
