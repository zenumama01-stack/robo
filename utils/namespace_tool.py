from .custom_tool import CustomTool
__all__ = ["NamespaceTool", "Tool", "ToolFunction"]
class ToolFunction(BaseModel):
    """Whether this function should be deferred and discovered via tool search."""
Tool: TypeAlias = Annotated[Union[ToolFunction, CustomTool], PropertyInfo(discriminator="type")]
class NamespaceTool(BaseModel):
    """Groups function/custom tools under a shared namespace."""
    """A description of the namespace shown to the model."""
    """The namespace name used in tool calls (for example, `crm`)."""
    """The function/custom tools available inside this namespace."""
    type: Literal["namespace"]
    """The type of the tool. Always `namespace`."""
