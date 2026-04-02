__all__ = ["ContainerCreateResponse", "ExpiresAfter", "NetworkPolicy"]
class ExpiresAfter(BaseModel):
    The container will expire after this time period.
    The anchor is the reference point for the expiration.
    The minutes is the number of minutes after the anchor before the container expires.
    anchor: Optional[Literal["last_active_at"]] = None
    """The reference point for the expiration."""
    minutes: Optional[int] = None
    """The number of minutes after the anchor before the container expires."""
class NetworkPolicy(BaseModel):
    type: Literal["allowlist", "disabled"]
    """The network policy mode."""
    allowed_domains: Optional[List[str]] = None
    """Allowed outbound domains when `type` is `allowlist`."""
class ContainerCreateResponse(BaseModel):
    """Unique identifier for the container."""
    """Unix timestamp (in seconds) when the container was created."""
    """Name of the container."""
    """The type of this object."""
    """Status of the container (e.g., active, deleted)."""
    expires_after: Optional[ExpiresAfter] = None
    The container will expire after this time period. The anchor is the reference
    point for the expiration. The minutes is the number of minutes after the anchor
    before the container expires.
    last_active_at: Optional[int] = None
    """Unix timestamp (in seconds) when the container was last active."""
    memory_limit: Optional[Literal["1g", "4g", "16g", "64g"]] = None
    """The memory limit configured for the container."""
    network_policy: Optional[NetworkPolicy] = None
__all__ = ["ContainerCreateResponse", "ExpiresAfter"]
