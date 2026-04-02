__all__ = ["ChatSessionExpiresAfterParam"]
class ChatSessionExpiresAfterParam(TypedDict, total=False):
    """Controls when the session expires relative to an anchor timestamp."""
    """Base timestamp used to calculate expiration. Currently fixed to `created_at`."""
    """Number of seconds after the anchor when the session expires."""
