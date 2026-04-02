    "RealtimeResponseCreateMcpTool",
    "AllowedTools",
    "AllowedToolsMcpToolFilter",
    "RequireApproval",
    "RequireApprovalMcpToolApprovalFilter",
    "RequireApprovalMcpToolApprovalFilterAlways",
    "RequireApprovalMcpToolApprovalFilterNever",
class AllowedToolsMcpToolFilter(BaseModel):
    """A filter object to specify which tools are allowed."""
    read_only: Optional[bool] = None
    """Indicates whether or not a tool modifies data or is read-only.
    If an MCP server is
    [annotated with `readOnlyHint`](https://modelcontextprotocol.io/specification/2025-06-18/schema#toolannotations-readonlyhint),
    it will match this filter.
    tool_names: Optional[List[str]] = None
    """List of allowed tool names."""
AllowedTools: TypeAlias = Union[List[str], AllowedToolsMcpToolFilter, None]
class RequireApprovalMcpToolApprovalFilterAlways(BaseModel):
class RequireApprovalMcpToolApprovalFilterNever(BaseModel):
class RequireApprovalMcpToolApprovalFilter(BaseModel):
    """Specify which of the MCP server's tools require approval.
    Can be
    `always`, `never`, or a filter object associated with tools
    that require approval.
    always: Optional[RequireApprovalMcpToolApprovalFilterAlways] = None
    never: Optional[RequireApprovalMcpToolApprovalFilterNever] = None
RequireApproval: TypeAlias = Union[RequireApprovalMcpToolApprovalFilter, Literal["always", "never"], None]
class RealtimeResponseCreateMcpTool(BaseModel):
    Give the model access to additional tools via remote Model Context Protocol
    (MCP) servers. [Learn more about MCP](https://platform.openai.com/docs/guides/tools-remote-mcp).
    """A label for this MCP server, used to identify it in tool calls."""
    type: Literal["mcp"]
    """The type of the MCP tool. Always `mcp`."""
    allowed_tools: Optional[AllowedTools] = None
    """List of allowed tool names or a filter object."""
    authorization: Optional[str] = None
    An OAuth access token that can be used with a remote MCP server, either with a
    custom MCP server URL or a service connector. Your application must handle the
    OAuth authorization flow and provide the token here.
    connector_id: Optional[
            "connector_dropbox",
            "connector_gmail",
            "connector_googlecalendar",
            "connector_googledrive",
            "connector_microsoftteams",
            "connector_outlookcalendar",
            "connector_outlookemail",
            "connector_sharepoint",
    """Identifier for service connectors, like those available in ChatGPT.
    One of `server_url` or `connector_id` must be provided. Learn more about service
    connectors
    [here](https://platform.openai.com/docs/guides/tools-remote-mcp#connectors).
    Currently supported `connector_id` values are:
    - Dropbox: `connector_dropbox`
    - Gmail: `connector_gmail`
    - Google Calendar: `connector_googlecalendar`
    - Google Drive: `connector_googledrive`
    - Microsoft Teams: `connector_microsoftteams`
    - Outlook Calendar: `connector_outlookcalendar`
    - Outlook Email: `connector_outlookemail`
    - SharePoint: `connector_sharepoint`
    defer_loading: Optional[bool] = None
    """Whether this MCP tool is deferred and discovered via tool search."""
    headers: Optional[Dict[str, str]] = None
    """Optional HTTP headers to send to the MCP server.
    Use for authentication or other purposes.
    require_approval: Optional[RequireApproval] = None
    """Specify which of the MCP server's tools require approval."""
    server_description: Optional[str] = None
    """Optional description of the MCP server, used to provide more context."""
    server_url: Optional[str] = None
    """The URL for the MCP server.
    One of `server_url` or `connector_id` must be provided.
