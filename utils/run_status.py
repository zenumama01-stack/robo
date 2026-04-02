__all__ = ["RunStatus"]
RunStatus: TypeAlias = Literal[
    "queued",
    "in_progress",
    "requires_action",
    "cancelling",
    "cancelled",
    "failed",
    "completed",
    "incomplete",
    "expired",
