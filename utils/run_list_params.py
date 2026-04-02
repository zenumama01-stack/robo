__all__ = ["RunListParams"]
class RunListParams(TypedDict, total=False):
    """Identifier for the last run from the previous pagination request."""
    """Number of runs to retrieve."""
    """Sort order for runs by timestamp.
    status: Literal["queued", "in_progress", "completed", "canceled", "failed"]
    """Filter runs by status.
    One of `queued` | `in_progress` | `failed` | `completed` | `canceled`.
