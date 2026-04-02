__all__ = ["RealtimeMcpApprovalRequestParam"]
class RealtimeMcpApprovalRequestParam(TypedDict, total=False):
    server_label: Required[str]
    type: Required[Literal["mcp_approval_request"]]
