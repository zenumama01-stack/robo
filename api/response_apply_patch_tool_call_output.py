__all__ = ["ResponseApplyPatchToolCallOutput"]
class ResponseApplyPatchToolCallOutput(BaseModel):
    """The output emitted by an apply patch tool call."""
    """The unique ID of the apply patch tool call output.
    status: Literal["completed", "failed"]
    """The status of the apply patch tool call output. One of `completed` or `failed`."""
    type: Literal["apply_patch_call_output"]
    """The type of the item. Always `apply_patch_call_output`."""
    """The ID of the entity that created this tool call output."""
    """Optional textual output returned by the apply patch tool."""
