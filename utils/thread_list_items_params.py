__all__ = ["ThreadListItemsParams"]
class ThreadListItemsParams(TypedDict, total=False):
    """List items created after this thread item ID.
    Defaults to null for the first page.
    """List items created before this thread item ID.
    Defaults to null for the newest results.
    """Maximum number of thread items to return. Defaults to 20."""
    """Sort order for results by creation time. Defaults to `desc`."""
