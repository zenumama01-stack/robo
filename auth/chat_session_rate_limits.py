__all__ = ["ChatSessionRateLimits"]
class ChatSessionRateLimits(BaseModel):
    """Active per-minute request limit for the session."""
    """Maximum allowed requests per one-minute window."""
