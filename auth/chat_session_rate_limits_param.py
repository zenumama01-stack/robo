__all__ = ["ChatSessionRateLimitsParam"]
class ChatSessionRateLimitsParam(TypedDict, total=False):
    """Controls request rate limits for the session."""
    """Maximum number of requests allowed per minute for the session. Defaults to 10."""
