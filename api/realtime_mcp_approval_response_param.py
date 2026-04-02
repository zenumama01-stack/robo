__all__ = ["RealtimeMcpApprovalResponseParam"]
class RealtimeMcpApprovalResponseParam(TypedDict, total=False):
    approval_request_id: Required[str]
    approve: Required[bool]
    type: Required[Literal["mcp_approval_response"]]
    reason: Optional[str]
