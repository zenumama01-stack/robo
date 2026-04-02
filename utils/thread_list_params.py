__all__ = ["ThreadListParams"]
class ThreadListParams(TypedDict, total=False):
    """Filter threads that belong to this user identifier.
    Defaults to null to return all users.
