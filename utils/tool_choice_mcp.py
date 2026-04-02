__all__ = ["ToolChoiceMcp"]
class ToolChoiceMcp(BaseModel):
    Use this option to force the model to call a specific tool on a remote MCP server.
    """The label of the MCP server to use."""
    """For MCP tools, the type is always `mcp`."""
    """The name of the tool to call on the server."""
