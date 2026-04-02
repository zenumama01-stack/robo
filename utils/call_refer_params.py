__all__ = ["CallReferParams"]
class CallReferParams(TypedDict, total=False):
    target_uri: Required[str]
    """URI that should appear in the SIP Refer-To header.
    Supports values like `tel:+14155550123` or `sip:agent@example.com`.
