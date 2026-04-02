__all__ = ["RealtimeMcpListTools", "Tool"]
class RealtimeMcpListTools(BaseModel):
    """A Realtime item listing tools available on an MCP server."""
    tools: List[Tool]
