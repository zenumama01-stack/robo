__all__ = ["ToolSearchTool"]
class ToolSearchTool(BaseModel):
    """Hosted or BYOT tool search configuration for deferred tools."""
    type: Literal["tool_search"]
    """The type of the tool. Always `tool_search`."""
    """Description shown to the model for a client-executed tool search tool."""
    """Whether tool search is executed by the server or by the client."""
    """Parameter schema for a client-executed tool search tool."""
