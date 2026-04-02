__all__ = ["OutputItemListParams"]
class OutputItemListParams(TypedDict, total=False):
    eval_id: Required[str]
    """Identifier for the last output item from the previous pagination request."""
    """Number of output items to retrieve."""
    """Sort order for output items by timestamp.
    status: Literal["fail", "pass"]
    """Filter output items by status.
    Use `failed` to filter by failed output items or `pass` to filter by passed
    output items.
