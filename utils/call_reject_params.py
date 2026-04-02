__all__ = ["CallRejectParams"]
class CallRejectParams(TypedDict, total=False):
    """SIP response code to send back to the caller.
    Defaults to `603` (Decline) when omitted.
