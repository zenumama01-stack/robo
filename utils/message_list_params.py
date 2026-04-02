__all__ = ["MessageListParams"]
class MessageListParams(TypedDict, total=False):
    run_id: str
    """Filter messages by the run ID that generated them."""
    """Identifier for the last message from the previous pagination request."""
    """Number of messages to retrieve."""
    """Sort order for messages by timestamp.
