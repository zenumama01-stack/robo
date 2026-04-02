__all__ = ["RateLimitsUpdatedEvent", "RateLimit"]
class RateLimit(BaseModel):
    limit: Optional[int] = None
    """The maximum allowed value for the rate limit."""
    name: Optional[Literal["requests", "tokens"]] = None
    """The name of the rate limit (`requests`, `tokens`)."""
    remaining: Optional[int] = None
    """The remaining value before the limit is reached."""
    reset_seconds: Optional[float] = None
    """Seconds until the rate limit resets."""
class RateLimitsUpdatedEvent(BaseModel):
    rate_limits: List[RateLimit]
    """List of rate limit information."""
    type: Literal["rate_limits.updated"]
    """The event type, must be `rate_limits.updated`."""
    """Emitted at the beginning of a Response to indicate the updated rate limits.
    When a Response is created some tokens will be "reserved" for the output
    tokens, the rate limits shown here reflect that reservation, which is then
    adjusted accordingly once the Response is completed.
